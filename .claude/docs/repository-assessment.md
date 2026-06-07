# Repository Assessment

> Phase 1 output of the Claude Code workspace bootstrap. A snapshot of the repository as analyzed, the standards detected, and the gaps this `.claude/` workspace closes.

## Summary

Interview Copilot AI is an AI-native interview-preparation SaaS. The repository contains a **complete architecture blueprint** (16 numbered docs + `DESIGN.md`) and a **production-shaped scaffold** for backend, frontend, and infrastructure. It is early-stage: structure and patterns are established, one vertical slice (Resume Analysis) is implemented end-to-end, and most feature breadth is still to be built against the documented patterns.

## Detected architecture & stack

| Area | Finding |
|---|---|
| Backend style | Clean Architecture (Domain / Application / Infrastructure / Api) + Vertical Slice + CQRS via MediatR; `Result<T>` over exceptions |
| Backend stack | .NET 10, ASP.NET Core minimal APIs, EF Core 10, PostgreSQL 16 + pgvector |
| Domain | DDD tactical patterns — aggregates, value objects, domain events, strongly-typed IDs |
| Frontend | Next.js 16 (App Router), React 19, TypeScript, Tailwind 4, shadcn/ui, TanStack Query, Zustand, Framer Motion |
| AI | Provider-agnostic abstraction (`IChatCompletionService`) + `AiModelRouter` + config `ModelCatalog` across OpenAI / Claude / Gemini; tiered routing, cost metering |
| Infra | Docker (compose + multi-stage), Terraform (AWS: VPC/ECS/RDS/ALB), GitHub Actions (CI + OIDC CD) |
| Observability | OpenTelemetry + Serilog, custom `ai.complete` spans |
| Tests | xUnit unit tests (Domain) + NetArchTest architecture tests enforcing the dependency rule |

## Conventions detected

- One public type per file; file-scoped namespaces; `record` for VOs/DTOs; sealed handlers.
- Vertical slices under `Application/Features/<Context>/<UseCase>/` (command + handler + validator + response).
- Strongly-typed IDs (`ResumeId`, `CandidateId`, ...) to avoid primitive obsession.
- Central Package Management (`Directory.Packages.props`), analyzers-as-errors, nullable enabled.
- Frontend: server components by default; TanStack Query owns server state, Zustand owns ephemeral UI state; design tokens in `globals.css` mirror `DESIGN.md`.

## Existing documentation

Strong. `docs/00..16` cover system architecture, C4, domain model, DB design, API contracts, frontend, AI, deployment, CI/CD, security, observability, roadmap, risk, cost, MVP, sprints. `DESIGN.md` defines the design system.

## Gaps this workspace closes

1. No machine-enforced contributor guardrails -> `CLAUDE.md` + hooks encode standards and block dangerous ops.
2. No repeatable review/test/doc/deploy workflows -> skills + slash commands.
3. No role specialization for AI assistance -> 7 subagents.
4. ADRs referenced but absent -> `.claude/docs/adr/` with template + seed records.
5. MCP usage undocumented -> `.claude/mcp/` config templates with setup + security notes.
6. Integration & frontend tests not yet present -> `update-test` skill + `tester` agent target 80%+.

## Risks to watch (engineering)

- Architecture erosion as features grow -> architecture tests + Pre/PostToolUse hooks.
- Provider/prompt regressions on AI changes -> `ai-engineer` agent + golden-set eval (docs/07).
- Migration safety (expand/contract) -> `deploy-checklist` gate.
