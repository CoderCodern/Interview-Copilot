# Interview Copilot AI — Engineering Memory (canonical)

This file is the operating contract for Claude Code in this repository. Follow it for every change. When in doubt, prefer the patterns already in `docs/` and the existing Resume vertical slice.

---

## 1. Project overview

**Interview Copilot AI** is an AI-native SaaS that helps candidates prepare for interviews. Users upload a URL / PDF / DOCX / image / text; the platform analyzes **companies**, **job descriptions**, and **resumes**, then generates company insights, tailored interview questions, personalized prep plans, mock interviews, and learning roadmaps.

**Business goals:** personalization (grounded in the user's real resume + target company + JD), trust (explainable, cited, no fabrication), and AI cost discipline (the largest variable cost).

**Architecture (backend):** Clean Architecture boundaries + Vertical Slice features + CQRS.
- `Domain` — aggregates, value objects, domain events, `Result<T>`. Zero dependencies.
- `Application` — CQRS slices (command/query + handler + validator + response), ports (interfaces), MediatR pipeline behaviors.
- `Infrastructure` — EF Core, AI providers, S3, adapters that implement Application ports.
- `Api` — ASP.NET Core minimal API endpoint groups, composition root, middleware.
Dependency rule points inward: `Api -> Application -> Domain`, `Infrastructure -> Application/Domain`. Enforced by `backend/tests/InterviewCopilot.Architecture.Tests`.

**Tech stack:** .NET 10, EF Core 10, PostgreSQL 16 + pgvector · Next.js 16 / React 19 / TypeScript / Tailwind 4 / shadcn/ui / TanStack Query / Zustand / Framer Motion · Docker, Terraform (AWS ECS/RDS), GitHub Actions · OpenAI + Claude + Gemini behind a provider abstraction.

**Map:** `docs/` (16-doc blueprint), `DESIGN.md` (design system), `backend/`, `frontend/`, `infra/`, `.github/workflows/`.

## 2. Engineering standards (enforced)

- **Clean Architecture** — never let the dependency rule point outward. Domain references nothing; Application defines ports, Infrastructure implements them. No EF/HTTP/SDK types in Domain or Application.
- **SOLID** — single-responsibility slices/handlers; depend on abstractions (ports), not concretions.
- **DRY** — cross-cutting concerns live once in MediatR behaviors (logging, validation, unit-of-work), not per handler.
- **KISS** — prefer the simplest design that satisfies the slice; no speculative abstraction.
- **YAGNI** — build the current use case, not imagined future ones. New providers/features go through tiers/config, not forks.
- **Errors** — return `Result<T>`; do not throw for expected business outcomes. Map errors to RFC 9457 ProblemDetails at the edge with a stable `code`.
- **Async** — all I/O is async with `CancellationToken`; long work goes to the queue/worker, never blocks a request.
- **Quality** — nullable enabled, warnings-as-errors, analyzers green, Central Package Management. One public type per file, file-scoped namespaces, `record` for VOs/DTOs, sealed handlers.

## 3. Naming conventions

**Backend (.NET)**
- Commands: `CreateResumeCommand`, `AnalyzeCompanyCommand`
- Queries: `GetResumeQuery`, `ListPreparationsQuery`
- Handlers: `CreateResumeCommandHandler` (sealed)
- Validators: `CreateResumeCommandValidator`
- Responses/DTOs: `ResumeResponse`, `ResumeDetail` (records)
- Aggregates/entities: `Resume`, `CompanyAnalysis`; value objects: `AnalysisSource`, `BlobReference`
- Strongly-typed IDs: `ResumeId`, `CandidateId`
- Slice folders: `Application/Features/<Context>/<UseCase>/`
- Domain events: past tense — `ResumeUploaded`, `PreparationCompleted`

**Frontend (TS/React)**
- Components: `ResumeCard.tsx` (PascalCase)
- Hooks: `useResume.ts` (camelCase, `use` prefix)
- Services/clients: `resumeService.ts`, `apiClient.ts`
- Stores: `ui.store.ts` (Zustand)
- Query keys: centralized in `lib/query/keys.ts`
- Types from OpenAPI: `types/api.d.ts` (generated, never hand-edited)

## 4. Testing requirements

- **Backend unit tests** (xUnit + FluentAssertions): all Domain behavior and Application handlers (ports faked with NSubstitute).
- **Backend integration tests**: API + EF Core against real Postgres via Testcontainers (incl. pgvector paths).
- **Architecture tests** (NetArchTest): dependency rule + naming; must stay green.
- **Frontend component tests** (Vitest + Testing Library): components and hooks; MSW for API contract.
- **E2E** (Playwright) for critical journeys (signup -> analysis -> prep -> mock).
- **Coverage goal: 80%+** on Domain + Application; prioritize business logic, edge cases, validations, and failure paths over getters.
- Every new slice ships with its tests in the same change. Tests must pass before a change is considered done.

## 5. Documentation rules

When architecture or contracts change, update in the **same change**:
- **README** — when setup, structure, or commands change.
- **ADR** (`.claude/docs/adr/`) — for any significant or hard-to-reverse decision (new dependency, data model shift, provider/tier policy, infra change). Use `TEMPLATE.md`.
- **API documentation** — the OpenAPI spec / `docs/05-api-design.md` when endpoints or contracts change; regenerate the frontend client.
- **Architecture docs** (`docs/01..16`) and **DESIGN.md** when their subject changes.
Diagrams use **Mermaid** in markdown.

## 6. Workflow expectations

1. Before coding, read the relevant `docs/` section and mirror the existing Resume slice.
2. Make the change as a vertical slice; wire ports in Infrastructure, expose via an endpoint group.
3. Add/Update tests (unit + integration/component) to 80%+.
4. Update docs/ADR/API as required by section 5.
5. Run `/code-review` on your changes and `/deploy-checklist` before release.
6. Use the right subagent for the job (see below).

## 7. Subagents (in `.claude/agents/`)

`architect` (system design, domain/db modeling) · `backend` (.NET, CQRS, EF Core, APIs) · `frontend` (Next.js, React, shadcn, Tailwind) · `reviewer` (code review, security, refactoring) · `tester` (test generation, coverage, QA) · `devops` (Docker, Actions, AWS, IaC) · `ai-engineer` (prompts, RAG, embeddings, provider routing).

## 8. Skills & commands

Skills (`.claude/skills/`) and their slash commands (`.claude/commands/`):
`/code-review` -> code-review · `/review` -> review (PR) · `/update-test` -> update-test · `/document` -> documentation · `/deploy-checklist` -> deploy-checklist · `/learn` -> learn.

## 9. Hard rules (do / don't)

- DO keep Domain dependency-free; DON'T import EF Core/MediatR/HTTP into Domain or Application.
- DO route all model calls through `IChatCompletionService`/`AiModelRouter`; DON'T call a provider SDK directly from a handler.
- DO scope every query to the owner (tenant); DON'T return data across candidates.
- DO put secrets in env/Secrets Manager; DON'T commit secrets or hardcode connection strings/keys.
- DO use `Result<T>`; DON'T use exceptions for control flow.
- DON'T run destructive ops (`rm -rf`, `git push --force`, dropping DBs) — hooks will block these.
- DON'T hand-edit generated files (`frontend/types/api.d.ts`, EF migrations after apply).
