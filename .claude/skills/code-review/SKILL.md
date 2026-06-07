---
name: code-review
description: Review modified/uncommitted files in this repo for architecture compliance, naming conventions, dead/duplicated code, security issues, missing validations, and missing logging. Use before committing or when the user says "review my changes", "code review", or runs /code-review.
---

# Skill: code-review

## Purpose
Give a fast, rigorous review of the **working changes** (uncommitted or staged) against this repo's standards in `.claude/CLAUDE.md`, before they become a commit/PR.

## Activation criteria
- User runs `/code-review` or asks to "review my changes / this file / before I commit".
- After a non-trivial edit the user wants checked.
- Scope = changed files only (`git diff` / `git diff --staged`), not the whole repo.

## Workflow
1. Determine scope: `git diff --name-only HEAD` (and `--staged`); read the diffs and the full changed files for context.
2. Check **architecture compliance** (Clean + Vertical Slice + CQRS):
   - Domain has no outward dependencies; Application references no Infrastructure types.
   - New use cases are vertical slices (command/query + handler + validator + response).
   - Model calls go through `IChatCompletionService`/router, not provider SDKs.
   - Queries are owner-scoped (tenant isolation).
3. Check **naming conventions** (section 3 of CLAUDE.md) for commands/queries/handlers/validators/components/hooks/services.
4. Detect **dead code** (unused types/members/exports), **duplicated code** (extract candidates), and over-complex logic.
5. Detect **security issues**: secrets in code, missing authz/ownership checks, SQL built by string, unvalidated input, PII in logs, prompt-injection-unsafe handling of uploaded text.
6. Detect **missing validations** (FluentValidation on commands; Zod on forms) and **missing logging/observability** (structured logs, `ai.complete`/custom spans where relevant).
7. Confirm **tests** exist for the change and **docs/ADR** impact (section 5).

## Expected output
Produce exactly these sections (omit empty ones except Critical/Documentation):

- **Critical Issues** — must fix before merge (architecture violations, security, data-loss). Each: file:line, problem, fix.
- **Warnings** — should fix (naming, duplication, missing tests/validation/logging).
- **Suggestions** — nice-to-have (readability, perf, idiom).
- **Documentation Impact** — which README/ADR/API/docs need updating, or "none".
- **Branch Naming Recommendation** — e.g. `feat/company-analysis-url-ingestion`.
- **Commit Message Recommendation** — Conventional Commits, e.g. `feat(company): add URL ingestion slice`.

Be specific and cite file:line. If clean, say so plainly and still give branch/commit recommendations.

## Example
> /code-review
Scope: `Application/Features/Companies/AnalyzeCompany/*`, `Api/Endpoints/CompanyEndpoints.cs`

**Critical Issues**
- `AnalyzeCompanyHandler.cs:42` calls `OpenAIClient` directly — violates the provider abstraction (CLAUDE.md §9). Inject `IChatCompletionService` and route by `AiTask.CompanyOverview`.

**Warnings**
- `CompanyEndpoints.cs:18` missing ownership scope on the GET; rely on the query handler + global filter and assert in a test.
- No validator for `AnalyzeCompanyCommand` — add `AnalyzeCompanyCommandValidator`.

**Suggestions**
- Extract section-mapping duplication in `CompanyAnalysis.Complete` into a private helper.

**Documentation Impact** — update `docs/05-api-design.md` (new endpoint) + regenerate OpenAPI client.
**Branch Naming Recommendation** — `feat/company-analysis-url-ingestion`
**Commit Message Recommendation** — `feat(company): add URL company-analysis vertical slice`
