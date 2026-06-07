---
description: Generate or update backend + frontend tests for changed code toward 80%+ coverage.
argument-hint: "[optional path/feature to target]"
---

Use the **update-test** skill (`.claude/skills/update-test/SKILL.md`).

Target: $ARGUMENTS (if empty, infer from `git diff`).

Add/Update xUnit unit + integration tests (Testcontainers) for backend and Vitest component/hook tests (MSW) for frontend, prioritizing business logic, edge cases, validations, and failure paths. Run the suite, iterate to green, and report coverage delta + remaining gaps. Prefer delegating to the `tester` subagent.
