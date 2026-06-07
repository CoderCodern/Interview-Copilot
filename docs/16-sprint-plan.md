# Sprint Plan

> **Document 16 of 16** · Implements: Sprint Plan deliverable · Pairs with [12-development-roadmap](12-development-roadmap.md), [15-mvp-plan](15-mvp-plan.md)

Two-week sprints for a 4–5 person team. Sprints 0–6 reach the **MVP** (end of Phase 3); 7–12 cover Mock Interview, hardening, and growth. Each sprint has a goal, key stories with rough points, and a demoable acceptance.

---

## Sprint cadence & definition of done

- **Cadence:** 2-week sprints, planning Monday, demo + retro Friday of week 2.
- **DoD:** merged to `main`, tests + architecture tests green, deployed to staging, observable (traces/metrics), docs/ADR updated, security checklist items satisfied, feature-flagged if risky.

```mermaid
flowchart LR
    s0[S0 Foundation] --> s1[S1 Ingestion]
    s1 --> s2[S2 Resume]
    s2 --> s3[S3 Company]
    s3 --> s4[S4 JD + RAG]
    s4 --> s5[S5 Prep engine]
    s5 --> s6[S6 Prep view + MVP]
    s6 --> s7[S7 Mock core]
    s7 --> s8[S8 Mock UX]
    s8 --> s9[S9 Cost/observability]
    s9 --> s10[S10 Security/SOC2]
    s10 --> s11[S11 Billing]
    s11 --> s12[S12 Teams/analytics]
```

---

## Phase 0–3 → MVP

### Sprint 0 — Foundation (walking skeleton)
**Goal:** deployable empty app with auth + pipeline scaffolding.
- Monorepo, solution + 4 projects, `Directory.*.props`, `.editorconfig`, CPM. (5)
- Next.js app shell, design tokens from DESIGN.md, providers. (5)
- CI (build/test/lint), security scans, Terraform dev env, ECS deploy. (8)
- IdP integration, `candidates`, ProblemDetails, OTel+Serilog, health checks. (8)
- Architecture tests (dependency rule). (3)
**Accept:** authenticated user sees empty dashboard on staging; one end-to-end trace.

### Sprint 1 — Ingestion backbone
**Goal:** the async path, proven.
- Presigned uploads + S3 + content validation. (5)
- Ingestion extractors: PDF/DOCX/text (+image stub). (8)
- SQS + worker + transactional outbox + idempotency. (8)
- SSE `/events` + frontend `useSSE`. (5)
- AI abstraction + router + **provider #1** + structured-output plumbing. (8)
**Accept:** a file uploads → worker processes a no-op job → client gets a live "completed" event.

### Sprint 2 — Resume Analysis
**Goal:** first real AI feature.
- Resume aggregate + persistence + `is_current` rule. (5)
- `AnalyzeResume` slice: parse → structured profile (schema + validate + repair). (8)
- Token metering → `token_usage`. (3)
- Resume list/detail/edit UI; live status. (8)
- Embeddings + pgvector write for resume chunks. (5)
**Accept:** upload resume → structured, editable profile; tokens/cost recorded.

### Sprint 3 — Company Analysis
**Goal:** company report from URL/text/PDF.
- URL ingestion (fetch + clean) + company aggregate. (8)
- Section synthesis: overview, products/services, clients/industries, culture, locations, hiring process, interview style + likely questions (Standard tier). (8)
- Grounding/anti-hallucination prompts + citations. (5)
- Company report UI (sections + questions). (5)
- Embeddings for company sections. (3)
**Accept:** submit a company URL → grounded sectioned report with likely questions.

### Sprint 4 — JD Analysis + RAG + provider #2
**Goal:** third input, retrieval, failover.
- JD slice: required skills, keywords, technologies, experience expectations. (5)
- Deterministic `GapAnalyzer` (resume vs JD). (5)
- RAG retrieval (owner-scoped HNSW) wired into generation. (8)
- **Provider #2** + circuit breaker + in-tier failover + prompt caching. (8)
- JD UI + gap preview. (5)
**Accept:** JD analyzed; gaps shown; failover demonstrated by disabling provider #1.

### Sprint 5 — Preparation engine (fusion)
**Goal:** the core generative outcome.
- `PreparationFactory` (ownership + completeness + input snapshot). (5)
- Generation: technical + behavioral questions, follow-ups, STAR, tips, learning roadmap (RAG-grounded, schema-constrained). (13)
- AI eval golden-set harness in CI. (5)
- Tier-routing tuning + cost-per-prep metric. (3)
**Accept:** company+resume+JD → complete structured Preparation; eval gate active.

### Sprint 6 — Preparation view + export → **MVP**
**Goal:** ship the MVP.
- Flagship Preparation UI (tabs, follow-ups, STAR, gaps, roadmap timeline). (8)
- Export to PDF/Markdown. (5)
- **Provider #3** added to catalog. (3)
- Quotas/rate limits; free + paid plan gating. (5)
- E2E (signup→prep) + staging→prod hardening. (5)
**Accept:** **MVP live** — full journey works in prod; Doc 15 readiness checklist green.

---

## Phase 4–6 → Mock, Hardening, Growth

### Sprint 7 — Mock Interview core
- `MockSession` aggregate, turn loop, rubric scoring, feedback, final score. (13)
- Modes (technical/behavioral/mixed/system-design). (3)
- Scoring eval cases. (5)
**Accept:** API-driven scored mock session grounded in a prep.

### Sprint 8 — Mock Interview UX
- Chat UI + optional token streaming (SignalR). (8)
- `mock.store` draft/timer; results + score visualization. (5)
- Feature-flag ramp. (3)
**Accept:** candidate completes a scored mock in the UI.

### Sprint 9 — Cost & observability completeness
- Budgets + soft-degrade + batch path; response cache. (8)
- Cost/quality dashboards, unit economics, SLO alerts. (8)
- Load test + autoscaling tuning. (5)
**Accept:** budgets enforce; dashboards live; SLOs hold under load test.

### Sprint 10 — Security & compliance
- WAF tuning, security headers/CSP, rate-limit polish. (5)
- Data-subject rights (export + erasure) flows. (5)
- SOC 2 control implementation + pen-test remediation. (8)
- DR restore drill. (3)
**Accept:** security checklist green; erasure works; restore drill passes.

### Sprint 11 — Billing & monetization
- Stripe integration, plans, metered usage, usage UI. (13)
- Quota enforcement tied to plan. (5)
**Accept:** a user can subscribe and is billed/quota'd correctly.

### Sprint 12 — Teams & analytics
- Team accounts, sharing, admin console. (13)
- Product analytics + A/B prompt testing. (5)
**Accept:** team plan usable; prompt experiments measurable.

---

## Capacity & risk notes

- Points are relative (Fibonacci); a 4–5 person team carries ≈ 30–40 pts/sprint — adjust to measured velocity.
- **Front-load risk:** the AI abstraction, async pipeline, and structured-output discipline land in S1–S2 so the riskiest mechanics are proven before feature breadth.
- **Eval gate from S5** prevents prompt/model regressions as features multiply.
- Buffer ~15% per sprint for spillover, infra surprises, and provider-API changes (Doc 13 R4).
