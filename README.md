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

## Quick start (local — no Docker)

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org) + [pnpm](https://pnpm.io/installation)
- PostgreSQL 16 with the [pgvector extension](https://github.com/pgvector/pgvector)

### 1 — Backend

```bash
cd backend

# Restore the dotnet-ef local tool
dotnet tool restore          # reads .config/dotnet-tools.json

# Set your real DB password (stored in User Secrets, never committed)
dotnet user-secrets set "ConnectionStrings:Postgres" \
  "Host=localhost;Port=5432;Database=interviewcopilot;Username=postgres;Password=YOUR_PASSWORD" \
  --project src/InterviewCopilot.Api

# Optional — add AI provider keys for live AI calls
dotnet user-secrets set "Ai:Providers:OpenAi:ApiKey" "sk-..." --project src/InterviewCopilot.Api
dotnet user-secrets set "Ai:Providers:Claude:ApiKey" "sk-ant-..." --project src/InterviewCopilot.Api
dotnet user-secrets set "Ai:Providers:Gemini:ApiKey" "AIza..." --project src/InterviewCopilot.Api

# Create the database schema (first time only, and after each new migration)
dotnet ef migrations add InitialCreate \
  -p src/InterviewCopilot.Infrastructure \
  -s src/InterviewCopilot.Api
dotnet ef database update -s src/InterviewCopilot.Api

# Build, test, and run
dotnet build
dotnet test                                    # unit + architecture tests
dotnet run --project src/InterviewCopilot.Api  # http://localhost:8080
# Interactive API explorer → http://localhost:8080/scalar/v1
```

> **Dev mode is auth-free.** When `ASPNETCORE_ENVIRONMENT=Development` (the default for `dotnet run`), a `DevAuthHandler` auto-authenticates every request as a fixed dev candidate — no IDP required. S3 is also replaced with a local-disk blob store under `$TEMP/interview-copilot-blobs`.

### 2 — Frontend

```bash
cd frontend
pnpm install
pnpm dev    # http://localhost:3000
```

`frontend/.env.local` already points at `http://localhost:8080/api/v1`. Edit it if your BE runs on a different port.

### Docker (later)

```bash
cd infra && docker compose up --build
# Postgres+pgvector :5432 · OTel :4317 · API :8080 · Web :3000
```

## Architectural highlights

- **Dependency rule enforced in CI.** `tests/InterviewCopilot.Architecture.Tests` fails the build if Domain reaches outward or Application references Infrastructure.
- **AI cost is a first-class concern.** A `ModelCatalog` (config, not code) + `AiModelRouter` pick the cheapest model meeting each task's quality tier, with cross-provider failover and per-call cost metering. See [Doc 07](docs/07-ai-architecture.md) and the cost model in [Doc 14](docs/14-cost-estimation.md).
- **Async by design.** Uploads → SQS → workers → SSE live updates, with a transactional outbox for reliable domain events.
- **Secure & observable by default.** Tenant isolation via owner-scoped query filters, JWT auth, encrypted storage, full OTel tracing including custom `ai.complete` spans.

## Status

Architecture: **complete**. Scaffold: **structural** — buildable shape with representative implementations of one full vertical slice (Resume Analysis) end-to-end; remaining features follow the same patterns per the [sprint plan](docs/16-sprint-plan.md).
