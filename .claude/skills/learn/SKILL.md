---
name: learn
description: Learn from the repository's existing patterns and propose (never auto-apply) candidate skills, rules, CLAUDE.md improvements, and ADRs. Use for "learn from this repo", "what patterns should we codify", or /learn.
---

# Skill: learn

## Purpose
Mine the codebase for recurring patterns, conventions, and implicit decisions, and propose ways to codify them — improving the workspace over time. **Proposals only; require approval before any change is applied.**

## Activation criteria
- User runs `/learn`, or asks to extract patterns / improve the workspace / codify conventions.
- After several features land and conventions have stabilized.

## Workflow
1. Survey representative code: Domain aggregates, Application slices, Infrastructure adapters, frontend features, tests, infra.
2. Detect **reusable patterns** (e.g., slice shape, Result mapping, provider routing, query-key strategy), **coding conventions** (naming, file layout, error handling), and **architectural decisions** made implicitly in code but not written down.
3. Compare against current `.claude/CLAUDE.md`, skills, and `docs/`; find gaps, drift, or contradictions.
4. Draft proposals — do not edit anything yet.

## Expected output (proposals, await approval)
- **Candidate Skills** — new skills worth adding (name, purpose, trigger), with rationale.
- **Candidate Rules** — additions/edits to CLAUDE.md standards or naming, quoted as exact text to insert.
- **Candidate CLAUDE.md improvements** — clarifications/corrections where memory and code disagree.
- **Candidate ADRs** — decisions visible in code that should be recorded (title + 2-line context).
- **Apply?** — end by asking which proposals to apply; never apply automatically.

## Example
> /learn
**Candidate Rules** — "All endpoint groups must call `RequireAuthorization()`; anonymous routes listed explicitly." (observed consistently; not yet in CLAUDE.md).
**Candidate Skills** — `new-slice` scaffolder generating command/handler/validator/response + test stubs.
**Candidate ADRs** — "Use UUID v7 for all primary keys" (seen in `Identifiers.cs`, undocumented).
**Apply?** — Which of these should I apply? (no changes made yet)
