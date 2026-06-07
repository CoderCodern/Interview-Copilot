#!/usr/bin/env bash
# PreToolUse hook: block dangerous operations and obvious architecture violations.
# Protocol: exit 2 => block (stderr is returned to Claude); exit 0 => allow.
# Fails open (exit 0) if the payload can't be parsed, to avoid wedging the session.
set -uo pipefail

INPUT="$(cat)"
command -v python3 >/dev/null 2>&1 || exit 0

python3 - "$INPUT" <<'PY'
import json, re, sys
try:
    data = json.loads(sys.argv[1] or "{}")
except Exception:
    sys.exit(0)

tool = data.get("tool_name", "")
ti   = data.get("tool_input", {}) or {}

# --- 1) Dangerous shell commands -------------------------------------------------
if tool == "Bash":
    cmd = ti.get("command", "") or ""
    dangerous = [
        r"\brm\s+-rf\s+(/|~|\.\s*$|\$HOME)",
        r"git\s+push\s+.*--force\b", r"git\s+push\s+.*\s-f\b",
        r"dotnet\s+ef\s+database\s+drop",
        r"\bterraform\s+destroy\b",
        r"\bDROP\s+DATABASE\b", r"\bTRUNCATE\b",
        r"\bchmod\s+-R?\s*777\b",
        r"curl\b.*\|\s*(ba)?sh", r"wget\b.*\|\s*(ba)?sh",
        r":\(\)\s*\{.*\};\s*:",  # fork bomb
    ]
    for pat in dangerous:
        if re.search(pat, cmd, re.IGNORECASE):
            print(f"BLOCKED by pre-tool-use hook: command matches dangerous pattern /{pat}/.\n"
                  f"Command: {cmd}\n"
                  f"If this is truly intended, ask the user to run it manually.", file=sys.stderr)
            sys.exit(2)

# --- 2) Architecture guard: keep Domain/Application pure -------------------------
if tool in ("Write", "Edit", "MultiEdit"):
    path = ti.get("file_path", "") or ""
    content = ti.get("content", "") or ti.get("new_string", "") or ""
    forbidden_in_domain = ["using Microsoft.EntityFrameworkCore", "using MediatR",
                           "using Microsoft.AspNetCore", "using OpenAI", "using Anthropic", "using Amazon."]
    if "InterviewCopilot.Domain/" in path:
        for u in forbidden_in_domain:
            if u in content:
                print(f"BLOCKED by pre-tool-use hook: Domain must stay dependency-free "
                      f"(found '{u}' in {path}). Move this to Application (port) / Infrastructure (adapter). "
                      f"See .claude/CLAUDE.md section 1 and 9.", file=sys.stderr)
                sys.exit(2)
    if "InterviewCopilot.Application/" in path:
        if "InterviewCopilot.Infrastructure" in content:
            print(f"BLOCKED by pre-tool-use hook: Application must not depend on Infrastructure "
                  f"({path}). Define a port here and implement it in Infrastructure.", file=sys.stderr)
            sys.exit(2)

sys.exit(0)
PY
