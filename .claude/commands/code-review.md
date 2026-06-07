---
description: Review uncommitted/working changes for architecture, naming, security, tests, and docs impact.
argument-hint: "[optional path or file to focus on]"
---

Use the **code-review** skill (`.claude/skills/code-review/SKILL.md`) to review the current working changes.

Scope: $ARGUMENTS (if empty, review all uncommitted + staged changes via `git diff` / `git diff --staged`).

Follow the skill exactly and produce: Critical Issues, Warnings, Suggestions, Documentation Impact, Branch Naming Recommendation, and Commit Message Recommendation. Cite file:line. Do not modify code unless asked — review only.
