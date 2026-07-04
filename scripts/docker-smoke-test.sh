#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

BACKEND_PORT="${BACKEND_PORT:-5000}"
export BACKEND_PORT

SKIP_UP=false
for arg in "$@"; do
  case "$arg" in
    --skip-up) SKIP_UP=true ;;
    *) echo "Unknown argument: $arg" >&2; exit 1 ;;
  esac
done

API_BASE="${API_BASE:-http://localhost:${BACKEND_PORT}}"
NGINX_BASE="${NGINX_BASE:-http://localhost}"
KEYCLOAK_BASE="${KEYCLOAK_BASE:-http://localhost:8080}"
TENANT_ID="${TENANT_ID:-demo}"
DEPARTMENT_CODE="SMOKE-$(date +%s)"
STORAGE_MARKER=".smoke-test-$(date +%s)"

log() {
  echo "[smoke-test] $*"
}

fail() {
  echo "[smoke-test] ERROR: $*" >&2
  exit 1
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || fail "Required command not found: $1"
}

get_access_token() {
  local response
  response="$(curl -sf -X POST "${NGINX_BASE}/auth/realms/hrportal/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=password" \
    -d "client_id=hrportal-api" \
    -d "client_secret=hrportal-api-secret" \
    -d "username=admin@demo.local" \
    -d "password=admin123")"
  echo "$response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["access_token"])'
}

if [[ "$SKIP_UP" == false ]]; then
  log "Starting full stack..."
  docker compose up --build --wait -d
fi

require_command curl
require_command python3
require_command docker

log "Checking backend health endpoints..."
curl -sf "${API_BASE}/health" >/dev/null
curl -sf "${API_BASE}/ready" >/dev/null

log "Checking nginx routing..."
curl -sf "${NGINX_BASE}/health" >/dev/null
curl -sf "${NGINX_BASE}/" | grep -qi '<html' || fail "Frontend HTML not returned via nginx"

log "Checking Keycloak realm..."
curl -sf "${KEYCLOAK_BASE}/realms/hrportal" >/dev/null

log "Checking demo tenant seed..."
tenants_response="$(curl -sf "${API_BASE}/api/v1/tenants")"
echo "$tenants_response" | python3 -c '
import json, sys
tenants = json.load(sys.stdin)
if not any(t.get("slug") == "demo" for t in tenants):
    raise SystemExit("Demo tenant not found")
'

log "Obtaining JWT from Keycloak..."
ACCESS_TOKEN="$(get_access_token)"

log "Creating department for persistence test..."
create_response="$(curl -sf -X POST "${API_BASE}/api/v1/departments" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: ${TENANT_ID}" \
  -d "{\"name\":\"Smoke Test Dept\",\"code\":\"${DEPARTMENT_CODE}\"}")"
DEPARTMENT_ID="$(echo "$create_response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["id"])')"

log "Writing storage marker via backend container..."
docker compose exec -T backend sh -c "echo '${STORAGE_MARKER}' > /app/storage/${STORAGE_MARKER}"

log "Restarting stack without removing volumes..."
docker compose down
docker compose up -d --wait

log "Verifying database persistence..."
departments_after_restart="$(curl -sf "${API_BASE}/api/v1/departments" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "X-Tenant-Id: ${TENANT_ID}")"
echo "$departments_after_restart" | python3 -c "
import json, sys
departments = json.load(sys.stdin)
target_id = '${DEPARTMENT_ID}'
target_code = '${DEPARTMENT_CODE}'
if not any(d.get('id') == target_id and d.get('code') == target_code for d in departments):
    raise SystemExit('Department not found after restart')
"

log "Verifying storage volume persistence..."
docker compose exec -T backend sh -c "test -f /app/storage/${STORAGE_MARKER}" \
  || fail "Storage marker missing after restart"

log "Checking nginx API proxy..."
curl -sf "${NGINX_BASE}/api/v1/tenants" >/dev/null

log "All smoke tests passed."
