#!/usr/bin/env python3
"""Generate Postman collection from OpenAPI spec."""

import json
import sys
from pathlib import Path

TENANT_HEADER = {"key": "X-Tenant-Id", "value": "{{tenantId}}", "type": "text"}
NO_TENANT_PATHS = {"/api/v1/tenants"}

EXAMPLE_BODIES = {
    ("POST", "/api/v1/tenants"): {
        "name": "Acme Corp",
        "slug": "acme"
    },
    ("POST", "/api/v1/employees"): {
        "firstName": "Mario",
        "lastName": "Rossi",
        "email": "mario.rossi@demo.local",
        "hireDate": "2024-01-15",
        "jobTitle": "Developer"
    },
    ("PUT", "/api/v1/employees/{id}"): {
        "firstName": "Mario",
        "lastName": "Rossi",
        "email": "mario.rossi@demo.local",
        "jobTitle": "Senior Developer"
    },
    ("POST", "/api/v1/departments"): {
        "name": "Engineering",
        "code": "ENG",
        "description": "Software development"
    },
    ("PUT", "/api/v1/departments/{id}"): {
        "name": "Engineering",
        "code": "ENG",
        "description": "Software development"
    },
    ("POST", "/api/v1/leave-requests"): {
        "employeeId": "{{employeeId}}",
        "startDate": "2025-07-01",
        "endDate": "2025-07-05",
        "type": "Annual",
        "reason": "Summer holiday"
    },
    ("PUT", "/api/v1/leave-requests/{id}/reject"): {
        "reason": "Insufficient coverage"
    },
    ("POST", "/api/v1/attendance/check-in"): {
        "employeeId": "{{employeeId}}",
        "date": "2025-07-04",
        "time": "09:00:00"
    },
    ("POST", "/api/v1/attendance/check-out"): {
        "employeeId": "{{employeeId}}",
        "date": "2025-07-04",
        "time": "18:00:00"
    },
}

ID_VARS = {
    "employees": "employeeId",
    "departments": "departmentId",
    "documents": "documentId",
    "leave-requests": "leaveRequestId",
}


def normalize_path(path: str) -> str:
    for segment, var in ID_VARS.items():
        if f"/api/v1/{segment}/{{id}}" in path or f"/api/v1/{segment}/{{id}}/" in path:
            return path.replace("{id}", f"{{{{{var}}}}}")
    return path


def build_url(path: str, query: dict | None = None) -> dict:
    normalized = normalize_path(path)
    parts = normalized.strip("/").split("/")
    raw = "{{baseUrl}}/" + "/".join(parts)
    url = {"raw": raw, "host": ["{{baseUrl}}"], "path": parts[1:]}
    if query:
        url["query"] = [{"key": k, "value": v} for k, v in query.items()]
    return url


def make_request(method: str, path: str, summary: str, anonymous: bool = False) -> dict:
    headers = []
    if path not in NO_TENANT_PATHS and not anonymous:
        headers.append(TENANT_HEADER)

    body = EXAMPLE_BODIES.get((method.upper(), path))
    request = {
        "method": method.upper(),
        "header": headers,
        "url": build_url(path),
        "description": summary,
    }

    if body:
        request["header"].append({"key": "Content-Type", "value": "application/json"})
        request["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, indent=2),
        }

    if path == "/api/v1/documents" and method.upper() == "POST":
        request["header"] = [h for h in request["header"] if h["key"] != "Content-Type"]
        request["body"] = {
            "mode": "formdata",
            "formdata": [
                {"key": "employeeId", "value": "{{employeeId}}", "type": "text"},
                {"key": "category", "value": "Contract", "type": "text"},
                {"key": "file", "type": "file", "src": []},
            ],
        }

    if path == "/api/v1/attendance/reports" and method.upper() == "GET":
        request["url"] = build_url(path, {"from": "2025-07-01", "to": "2025-07-31"})

    return {"name": summary, "request": request}


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    spec_path = root / "docs/openapi/hrportal-v1.json"
    out_path = root / "docs/postman/HR-Portal.postman_collection.json"

    spec = json.loads(spec_path.read_text())
    folders: dict[str, list] = {}

    for path, methods in spec["paths"].items():
        for method, op in methods.items():
            if method not in {"get", "post", "put", "delete", "patch"}:
                continue
            tag = op.get("tags", ["Other"])[0]
            summary = op.get("summary", f"{method.upper()} {path}")
            anonymous = tag == "Tenants"
            folders.setdefault(tag, []).append(make_request(method, path, summary, anonymous))

    collection = {
        "info": {
            "name": "HR Portal API",
            "description": "Multi-tenant HR Portal API. Set accessToken (Bearer JWT from Keycloak) and tenantId (default: demo).",
            "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
        },
        "auth": {
            "type": "bearer",
            "bearer": [{"key": "token", "value": "{{accessToken}}", "type": "string"}],
        },
        "variable": [
            {"key": "baseUrl", "value": "http://localhost:5000"},
            {"key": "tenantId", "value": "demo"},
            {"key": "accessToken", "value": ""},
            {"key": "employeeId", "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"},
            {"key": "departmentId", "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"},
            {"key": "documentId", "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"},
            {"key": "leaveRequestId", "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"},
        ],
        "item": [
            {"name": tag, "item": requests}
            for tag, requests in sorted(folders.items())
        ],
    }

    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(json.dumps(collection, indent=2))
    print(f"Wrote {out_path} ({sum(len(v) for v in folders.values())} requests)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
