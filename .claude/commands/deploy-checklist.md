---
description: Validate release readiness (backend, frontend, infra) and produce a Deployment Readiness Report.
argument-hint: "[target environment: staging|prod]"
---

Use the **deploy-checklist** skill (`.claude/skills/deploy-checklist/SKILL.md`).

Target environment: $ARGUMENTS (default: staging).

Verify backend (build, tests, expand/contract migrations), frontend (build, bundle size, client sync), and infrastructure (env vars, secrets, IaC, rollback path), then output a Deployment Readiness Report with a GO / NO-GO decision, blockers, and post-deploy checks. Consider the `devops` subagent. Do not deploy — assess only.
