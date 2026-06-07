#!/usr/bin/env bash
# Stop hook: nudge a session wrap-up. Non-blocking (exit 0); shown in transcript.
set -uo pipefail
echo "== Session wrap-up reminder =="
echo "Before you finish: (1) run /code-review on changes, (2) ensure tests pass + 80% on touched logic,"
echo "(3) update README/ADR/API docs if architecture/contracts changed (CLAUDE.md S5),"
echo "(4) consider /learn to capture any new reusable patterns (proposals only)."
exit 0
