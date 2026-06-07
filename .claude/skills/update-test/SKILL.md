---
name: update-test
description: Generate or update tests for changed code — backend unit + integration (xUnit/Testcontainers) and frontend component + hook tests (Vitest/Testing Library), targeting 80%+ coverage on business logic, edge cases, validations, and failure paths. Use for "write tests", "update tests", "improve coverage", or /update-test.
---

# Skill: update-test

## Purpose
Bring test coverage up to standard for new or modified code, focusing on behavior that matters: business logic, edge cases, validations, and failure paths — not trivial getters.

## Activation criteria
- User runs `/update-test`, or asks to add/update tests or raise coverage.
- A new slice/handler/component/hook was added without tests.

## Workflow
1. Identify changed code in scope (`git diff`); locate the matching test project/folder.
2. **Backend**
   - Unit tests (xUnit + FluentAssertions): Domain invariants/transitions; Application handlers with ports faked via NSubstitute. Cover success, validation failure, not-found/ownership, and AI/parse failure paths.
   - Integration tests (Testcontainers + Postgres): endpoint -> handler -> EF round-trip, including pgvector paths where relevant.
   - Keep architecture tests green.
3. **Frontend**
   - Component tests (Vitest + Testing Library): render states — loading/skeleton, empty, error, success; user interactions.
   - Hook tests: query/mutation hooks with MSW mocking the API per the OpenAPI contract.
4. Name tests by behavior: `Method_condition_expectedResult` (backend) / `renders X when Y` (frontend).
5. Run the suite; iterate until green. Report coverage delta and remaining gaps.

## Coverage goal
**80%+** on Domain + Application (and meaningful coverage on critical frontend flows). Prioritize edge cases, validations, and failures.

## Expected output
- New/updated test files following repo conventions.
- A short report: what was covered, success/edge/failure cases added, coverage before/after (estimate if not measurable), and any gaps left with rationale.

## Example
> /update-test  (after adding the Company analysis slice)
Adds `AnalyzeCompanyHandlerTests` (success, provider-failure -> Fail, not-owned -> NotFound), `CompanyAnalysisTests` (status transitions), and `CompanyReport.test.tsx` (loading/empty/error/success). Coverage on Application est. 72% -> 84%. Gap: URL-fetch timeout path deferred to integration suite (noted).
