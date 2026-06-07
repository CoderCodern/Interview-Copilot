---
description: Full pull-request review with risk assessment and an approval recommendation.
argument-hint: "[PR number/URL, or branch to compare against main]"
---

Use the **review** skill (`.claude/skills/review/SKILL.md`) to perform a complete PR review.

Target: $ARGUMENTS (if empty, review the current branch vs `main`: `git diff main...HEAD`, `git log main..HEAD --oneline`). If the GitHub MCP is configured, fetch the PR details; otherwise use local git.

Produce: PR Summary, Findings (Critical/Warnings/Suggestions), Tests, Documentation, Risk Assessment, and an Approval Recommendation (Approve / Request Changes / Needs Discussion). Consider delegating to the `reviewer` subagent for an independent pass.
