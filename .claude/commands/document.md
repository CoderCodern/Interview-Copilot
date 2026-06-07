---
description: Create/update technical docs — README, ADRs, API docs, and Mermaid diagrams — for recent changes.
argument-hint: "[what changed, or doc/ADR to write]"
---

Use the **documentation** skill (`.claude/skills/documentation/SKILL.md`).

Subject: $ARGUMENTS (if empty, infer affected docs from recent changes).

Update README/`docs/01..16`/`DESIGN.md` as needed, write an ADR from `.claude/docs/adr/TEMPLATE.md` for any significant decision, reflect API/contract changes in `docs/05-api-design.md` (+ regenerate the client), and author Mermaid diagrams where they clarify. List every file touched. Consider the `architect` subagent for ADRs.
