# Interview Copilot AI

> **Walk in prepared.** An AI-native SaaS platform that turns interview preparation into a guided, personalized workflow: analyze a company, a resume, and a job description, then generate a tailored prep plan and practice in a scored AI mock interview.

This repository contains the **complete architecture blueprint** plus a **production-shaped scaffold** for the backend (.NET 10), frontend (Next.js 16), and infrastructure (Docker + Terraform + GitHub Actions).

---

## What's inside

```
interview-copilot/
├── README.md                  ← you are here
├── DESIGN.md                  ← design system (tokens, components, motion, a11y)
├── docs/                      ← 16-document architecture suite (start at 00-overview.md)
├── backend/                   ← .NET 10 · Clean + Vertical Slice + CQRS + EF Core + pgvector
│   ├── InterviewCopilot.slnx
│   ├── src/{Domain,Application,Infrastructure,Api}
│   └── tests/{Domain.UnitTests,Architecture.Tests}
├── frontend/                  ← Next.js 16 · React 19 · Tailwind 4 · TanStack Query · Zustand
│   ├── app/  components/  lib/  hooks/  stores/
├── infra/                     ← docker-compose, Dockerfiles, Terraform (AWS), OTel config
└── .github/workflows/         ← CI (backend, frontend) + CD (OIDC → ECS)
```

## Read the architecture first

The design is documented as a numbered suite in [`docs/`](docs/). Start with the overview, which carries a requirements-traceability matrix and links every deliverable:

| # | Document | # | Document |
|---|---|---|---|
| 00 | [Overview](docs/00-overview.md) | 08 | [Deployment](docs/08-deployment-architecture.md) |
| 01 | [System Architecture](docs/01-system-architecture.md) | 09 | [CI/CD](docs/09-cicd-architecture.md) |
| 02 | [C4 Diagrams](docs/02-c4-diagrams.md) | 10 | [Security](docs/10-security-architecture.md) |
| 03 | [Domain Model](docs/03-domain-model.md) | 11 | [Observability](docs/11-observability.md) |
| 04 | [Database Design](docs/04-database-design.md) | 12 | [Development Roadmap](docs/12-development-roadmap.md) |
| 05 | [API Design](docs/05-api-design.md) | 13 | [Risk Analysis](docs/13-risk-analysis.md) |
| 06 | [Frontend Architecture](docs/06-frontend-architecture.md) | 14 | [Cost Estimation](docs/14-cost-estimation.md) |
| 07 | [AI Architecture](docs/07-ai-architecture.md) | 15 | [MVP Plan](docs/15-mvp-plan.md) |
| | [DESIGN.md](DESIGN.md) | 16 | [Sprint Plan](docs/16-sprint-plan.md) |

## Core features

1. **Company Analysis** — URL/PDF/DOCX/image/text → overview, products, clients, culture, locations, hiring process, interview style, likely questions.
2. **Resume Analysis** — parse to structured skills, experience, projects, education, certifications.
3. **Job Description Analysis** — required skills, keywords, technologies, experience, and resume-vs-JD gaps.
4. **Interview Preparation** — fuse the three inputs into technical + behavioral questions, follow-ups, STAR answers, tips, and a learning roadmap.
5. **AI Mock Interview** — stateful, scored, coached practice.

## Tech stack

**Backend:** .NET 10 · ASP.NET Core · Clean + Vertical Slice Architecture · CQRS · MediatR · FluentValidation · EF Core 10 · PostgreSQL 16 · pgvector · OpenTelemetry · Serilog
**Frontend:** Next.js 16 · React 19 · TypeScript · Tailwind 4 · shadcn/ui · Framer Motion · TanStack Query · Zustand
**AI:** provider-agnostic abstraction routing across OpenAI, Anthropic Claude, and Google Gemini with tiered model selection, prompt caching, RAG, and per-call cost metering
**Infra:** Docker · AWS (ECS Fargate, RDS, ElastiCache, SQS, S3, CloudFront) · Terraform · GitHub Actions (OIDC)

## Quick start (local)

> The scaffold pins versions and shows structure. Provider SDK calls and a few adapters are marked `TODO` (see Doc 07/08); fill credentials in env/secrets before running for real.

```bash
# 1) Bring up backing services + apps
cd infra && docker compose up --build
#    Postgres+pgvector :5432 · Redis :6379 · LocalStack :4566 · OTel :4317
#    API :8080 · Web :3000

# 2) Backend dev loop
cd backend
dotnet restore
dotnet build
dotnet test                      # unit + architecture (dependency-rule) tests

# 3) Frontend dev loop
cd frontend
npm install
npm run dev                      # http://localhost:3000
```

## Architectural highlights

- **Dependency rule enforced in CI.** `tests/InterviewCopilot.Architecture.Tests` fails the build if Domain reaches outward or Application references Infrastructure.
- **AI cost is a first-class concern.** A `ModelCatalog` (config, not code) + `AiModelRouter` pick the cheapest model meeting each task's quality tier, with cross-provider failover and per-call cost metering. See [Doc 07](docs/07-ai-architecture.md) and the cost model in [Doc 14](docs/14-cost-estimation.md).
- **Async by design.** Uploads → SQS → workers → SSE live updates, with a transactional outbox for reliable domain events.
- **Secure & observable by default.** Tenant isolation via owner-scoped query filters, JWT auth, encrypted storage, full OTel tracing including custom `ai.complete` spans.

## Status

Architecture: **complete**. Scaffold: **structural** — buildable shape with representative implementations of one full vertical slice (Resume Analysis) end-to-end; remaining features follow the same patterns per the [sprint plan](docs/16-sprint-plan.md).
