#!/usr/bin/env bash
# SessionStart hook: load project orientation + active ADRs into context (stdout is added to context).
set -uo pipefail
ROOT="${CLAUDE_PROJECT_DIR:-.}"

echo "== Interview Copilot AI — session context =="
echo "Standards: .claude/CLAUDE.md (Clean+Vertical Slice+CQRS, naming, testing 80%+, docs rules)."
echo "Blueprint: docs/00-overview.md .. docs/16-sprint-plan.md ; Design: DESIGN.md"
echo "Reference slice: backend/src/InterviewCopilot.Application/Features/Resumes/"
echo "Commands: /code-review /review /update-test /document /deploy-checklist /learn"
echo ""
if [ -d "$ROOT/.claude/docs/adr" ]; then
  echo "Architecture Decision Records:"
  for f in "$ROOT"/.claude/docs/adr/*.md; do
    [ -e "$f" ] || continue
    base="$(basename "$f")"
    [ "$base" = "TEMPLATE.md" ] && continue
    title="$(grep -m1 '^# ' "$f" 2>/dev/null | sed 's/^# //')"
    status="$(grep -m1 'Status' "$f" 2>/dev/null | sed -E 's/.*\*\* *//')"
    echo "  - ${base}: ${title} [${status}]"
  done
fi
exit 0
