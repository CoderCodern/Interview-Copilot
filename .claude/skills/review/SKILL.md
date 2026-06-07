---
name: review
description: Full pull-request review — analyze PR changes, commits, branch name, tests, and documentation, then give a risk assessment and an approval recommendation. Use for "review this PR", a PR URL/number, or /review.
---

# Skill: review

## Purpose
Perform a complete **pull-request** review (broader than `code-review`, which targets working changes): the whole diff plus commit hygiene, branch naming, test adequacy, and documentation completeness, ending in an explicit approval decision.

## Activation criteria
- User runs `/review`, gives a PR URL/number, or asks to "review this PR / before merge".
- A branch is ready for merge.

## Workflow
1. Gather PR context: changed files (`git diff main...HEAD`), commit list (`git log main..HEAD --oneline`), branch name, PR title/description. If GitHub MCP is available, fetch the PR; otherwise use local git.
2. Run the `code-review` checks across the full PR diff (architecture, naming, dead/duplicated code, security, validations, logging).
3. Review **commits**: Conventional Commits, atomic, no WIP/secrets, message quality.
4. Review **branch name**: matches `feat|fix|chore|refactor|docs/<scope>-<slug>`.
5. Review **tests**: new/changed logic covered; unit + integration/component present; coverage not regressed; failure paths tested.
6. Review **documentation**: README/ADR/API/docs updated per CLAUDE.md §5; ADR present for significant decisions.
7. Assess blast radius: migrations, public API/contract changes, infra changes, AI cost/prompt changes.

## Expected output
- **PR Summary** — what changed and why, in 3-6 lines.
- **Findings** — Critical / Warnings / Suggestions (reuse code-review format, cite file:line).
- **Tests** — adequate? gaps?
- **Documentation** — complete? what's missing?
- **Risk Assessment** — Low / Medium / High with the specific risks (data, security, contract, cost, rollout) and required mitigations.
- **Approval Recommendation** — one of: **Approve** · **Request Changes** · **Needs Discussion**, with a one-line justification and the top blocking items if not Approve.

## Example
**PR Summary** — Adds JD analysis slice + gap analyzer; new `/job-descriptions` endpoints; 3 commits, branch `feat/jd-analysis`.
**Findings** — *Critical:* none. *Warnings:* missing integration test for gap analyzer edge case (empty skills). *Suggestions:* memoize tier lookup.
**Tests** — unit present; add the empty-skills integration case.
**Documentation** — `docs/05` updated; add ADR for the deterministic gap-analyzer choice.
**Risk Assessment** — Low; no migration, additive endpoints.
**Approval Recommendation** — **Request Changes**: add the missing test + ADR, then approve.
