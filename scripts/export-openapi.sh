#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/backend/src/HrPortal.Api/HrPortal.Api.csproj"
OUTPUT_DIR="$ROOT_DIR/docs/openapi"
OUTPUT_FILE="$OUTPUT_DIR/hrportal-v1.json"
API_URL="${API_URL:-http://localhost:5000/swagger/v1/swagger.json}"

mkdir -p "$OUTPUT_DIR"

export_openapi_via_cli() {
  if ! command -v dotnet >/dev/null 2>&1; then
    return 1
  fi

  export PATH="$PATH:$HOME/.dotnet/tools"

  if ! command -v swagger >/dev/null 2>&1; then
    echo "Installing Swashbuckle.AspNetCore.Cli..."
    dotnet tool install -g Swashbuckle.AspNetCore.Cli || return 1
    export PATH="$PATH:$HOME/.dotnet/tools"
  fi

  swagger tofile "$PROJECT" "$OUTPUT_FILE" v1
}

export_openapi_via_curl() {
  echo "Fetching OpenAPI spec from $API_URL"
  curl -fsSL "$API_URL" -o "$OUTPUT_FILE"
}

if export_openapi_via_cli 2>/dev/null; then
  echo "Exported OpenAPI spec via Swashbuckle CLI to $OUTPUT_FILE"
elif export_openapi_via_curl; then
  echo "Exported OpenAPI spec via curl to $OUTPUT_FILE"
else
  echo "Failed to export OpenAPI spec. Start the API (docker compose up) or install Swashbuckle.AspNetCore.Cli." >&2
  exit 1
fi
