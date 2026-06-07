---
name: deploy-checklist
description: Validate release readiness across backend (build, tests, migrations), frontend (build, bundle size), and infrastructure (env vars, secrets, deploy configs) and produce a Deployment Readiness Report. Use before shipping a release, or /deploy-checklist.
---

# Skill: deploy-checklist

## Purpose
Gate a release: verify the change is safe to deploy and produce a go/no-go report aligned with the CI/CD design in `docs/09-cicd-architecture.md`.

## Activation criteria
- User runs `/deploy-checklist`, or says "ready to ship / deploy / release".
- Before promoting to staging or production.

## Workflow
1. **Backend**
   - Build clean with warnings-as-errors (`dotnet build -c Release`).
   - All tests pass (unit + integration + architecture).
   - Migrations: present for schema changes, **expand/contract** (backward-compatible), and applied as a pre-traffic step. No destructive migration in a single release.
2. **Frontend**
   - `npm run build` succeeds (Turbopack); typecheck + lint clean.
   - Bundle size within budget; generated API client in sync with OpenAPI.
3. **Infrastructure**
   - Required env vars present for the target environment; secrets in Secrets Manager/SSM (none in code).
   - `terraform validate`/`plan` clean; deploy configs (task defs, health checks `/health/ready`) correct.
   - Rollback path confirmed (previous image + backward-compatible DB).
4. **Cross-cutting** — observability wired (traces/metrics/logs), feature flags set for risky features, AI budgets/limits configured.

## Expected output — Deployment Readiness Report
- **Decision:** GO / NO-GO.
- **Backend:** build / tests / migrations — pass|fail + notes.
- **Frontend:** build / bundle / client-sync — pass|fail + notes.
- **Infrastructure:** env / secrets / IaC / rollback — pass|fail + notes.
- **Blockers:** explicit list if NO-GO.
- **Post-deploy checks:** smoke (`/health/ready`), canary journey, dashboards/alerts to watch.

## Example
**Decision:** NO-GO.
**Backend:** build pass; tests pass; **migration adds NOT NULL without default — not expand/contract.** Fix: add column nullable -> backfill -> enforce in a later release.
**Frontend:** pass. **Infrastructure:** env pass; rollback confirmed.
**Blockers:** migration safety. **Post-deploy:** watch async completion p95 + AI cost dashboard.
