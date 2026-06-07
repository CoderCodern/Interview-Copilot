#!/usr/bin/env bash
# PostToolUse hook: advisory reminders about tests and docs after code edits.
# Adds context for Claude via JSON (non-blocking).
set -uo pipefail

INPUT="$(cat)"
command -v python3 >/dev/null 2>&1 || exit 0

python3 - "$INPUT" <<'PY'
import json, sys
try:
    data = json.loads(sys.argv[1] or "{}")
except Exception:
    sys.exit(0)

ti   = data.get("tool_input", {}) or {}
path = ti.get("file_path", "") or ""
msgs = []

if path.endswith(".cs") and "/src/" in path and "/tests/" not in path:
    msgs.append("A backend source file changed. Ensure matching xUnit tests exist/updated "
                "(unit for handlers/domain, integration for endpoints) toward the 80% goal — see /update-test.")
if (path.endswith(".tsx") or path.endswith(".ts")) and "/frontend/" in path and ".test." not in path:
    msgs.append("A frontend file changed. Add/Update Vitest component/hook tests (loading/empty/error/success).")
if "Api/Endpoints" in path or path.endswith("05-api-design.md"):
    msgs.append("API surface may have changed: update docs/05-api-design.md + OpenAPI and regenerate the frontend client (npm run gen:api).")
if any(k in path for k in ["AppDbContext", "Configurations/", "Migrations/"]):
    msgs.append("Persistence changed: add an EF migration (expand/contract) and update docs/04-database-design.md.")

if msgs:
    out = {"hookSpecificOutput": {"hookEventName": "PostToolUse",
           "additionalContext": "Reminders (.claude post-tool-use): " + " | ".join(msgs)}}
    print(json.dumps(out))
sys.exit(0)
PY
