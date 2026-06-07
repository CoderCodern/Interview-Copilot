# 0001. Record architecture decisions using ADRs

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** Architecture
- **Tags:** process, documentation

## Context
The repository's CLAUDE.md and `docs/` require that significant or hard-to-reverse decisions be documented, but no ADR practice existed. Decisions were implicit in code and the architecture suite, making rationale hard to trace.

## Decision
We will record significant architectural decisions as Architecture Decision Records in `.claude/docs/adr/`, one file per decision (`NNNN-title.md`), using `TEMPLATE.md`. An ADR is required for: new dependencies, data-model changes, AI provider/tier policy changes, infrastructure changes, and any decision that is costly to reverse. The `documentation` skill and `architect` subagent create/maintain ADRs; the SessionStart hook surfaces active ADRs.

## Consequences
- **Positive:** durable, reviewable rationale; faster onboarding; consistent decisions.
- **Negative / costs:** small authoring overhead per significant change.
- **Follow-ups:** keep statuses current; supersede rather than delete.

## Alternatives considered
- **Wiki/Notion** — splits knowledge from the repo; rejected (decisions should version with code).
- **Only docs/ prose** — good for design, weak for point-in-time decisions + status; ADRs complement it.
