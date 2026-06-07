# Interview Copilot AI — Solution Overview

> **Document 00 of 16** · Architecture Blueprint · Status: Baseline v1.0 · Last updated: June 2026

This is the entry point to the Interview Copilot AI architecture suite. It states the product vision, scope, guiding principles, and maps every project requirement to the document that satisfies it.

---

## 1. Vision

Interview Copilot AI is an AI-native SaaS platform that turns the scattered, anxiety-driven work of interview preparation into a guided, personalized workflow. A candidate uploads their resume, points the system at a target company and a job description, and receives a tailored preparation plan: likely questions, gap analysis, STAR-structured answer scaffolding, a learning roadmap, and an interactive AI mock interview that scores their answers and coaches them toward improvement.

The product wins on three axes:

- **Personalization** — every artifact is grounded in the candidate's actual resume, the specific company, and the exact job description, not generic advice.
- **Trust** — analysis is explainable, cites its sources, and never fabricates company facts it cannot ground.
- **Cost discipline** — AI is the largest variable cost, so the architecture treats model selection, caching, and token budgeting as first-class engineering concerns.

## 2. Target users

| Persona | Need | Primary features |
|---|---|---|
| **Active job seeker** | Prepare quickly for a specific upcoming interview | Company Analysis, JD Analysis, Interview Prep, Mock Interview |
| **Career switcher** | Understand skill gaps for a new domain | Resume Analysis, JD gap analysis, Learning Roadmap |
| **New graduate** | Learn how interviews work and practice | Mock Interview, Prep tips |
| **Passive candidate** | Light, periodic readiness checks | Saved analyses, re-runs |

## 3. Product scope (what the platform does)

The five core capabilities, each detailed in later documents:

1. **Company Analysis** — ingest a URL, PDF, DOCX, image, or raw text; extract and synthesize company overview, products & services, clients & industries, culture, locations, hiring process, interview style, and likely questions.
2. **Resume Analysis** — parse PDF/DOCX/image resumes into structured skills, experience, projects, education, and certifications, persisted for reuse.
3. **Job Description Analysis** — parse a JD into required skills, keywords, technologies, experience expectations, and (against the candidate's resume) a knowledge-gap report.
4. **Interview Preparation** — fuse Company + Resume + JD into technical and behavioral questions, follow-ups, STAR answer suggestions, prep tips, and a learning roadmap.
5. **AI Mock Interview** — a stateful, role-played chat interview that scores answers and returns improvement feedback.

## 4. Architectural principles

These principles are non-negotiable and every downstream decision is justified against them.

**Domain-first.** The business model (analyses, candidates, preparations) is expressed in a dependency-free Domain layer using DDD tactical patterns. Infrastructure and frameworks orbit the domain, never the reverse.

**Vertical slices over horizontal layers for features.** Inside the Application layer, each use case is a self-contained slice (command/query + handler + validator + response) so a feature can be understood and changed in one place. Clean Architecture provides the macro boundaries; vertical slices provide the micro organization.

**Provider-agnostic AI.** No business code references OpenAI, Claude, or Gemini directly. All model calls flow through an `IChatCompletionService` / `IEmbeddingService` abstraction with a routing policy that picks the cheapest model that meets the quality bar for each task.

**Async, evented, observable.** Long-running ingestion and AI work runs in the background, emits domain events, and is fully traced. Every request carries a correlation ID from the browser through to the model call.

**Secure and private by default.** Candidate documents are sensitive PII. Encryption in transit and at rest, least-privilege access, tenant isolation, and PII-aware logging are baseline, not add-ons.

**Cost as a feature.** Token accounting, prompt caching, model tiering, and batch processing are designed in, monitored, and budgeted (see Doc 14).

## 5. High-level technology stack

| Layer | Technology | Version target (June 2026) |
|---|---|---|
| Backend runtime | .NET / ASP.NET Core | **.NET 10 (LTS, GA Nov 2025)** |
| Architecture | Clean + Vertical Slice + CQRS | MediatR, FluentValidation |
| Persistence | EF Core + PostgreSQL + pgvector | EF Core 10 (LTS), PostgreSQL 16, pgvector 0.8+ |
| Observability | OpenTelemetry + Serilog | OTLP export |
| Frontend | Next.js / React / TypeScript | **Next.js 16.2, React 19.2** |
| UI | Tailwind, shadcn/ui, Framer Motion | Tailwind 4 |
| Client state/data | Zustand, TanStack Query | v5 |
| AI providers | OpenAI, Anthropic Claude, Google Gemini | via provider abstraction |
| Infra | Docker, AWS, Terraform | ECS Fargate, RDS, S3 |
| CI/CD | GitHub Actions | OIDC to AWS |

Concrete versions and rationale live in Doc 01; current model pricing in Doc 14.

## 6. Requirements traceability

Every numbered requirement and named deliverable from the brief, mapped to where it is satisfied.

| # | Requirement | Satisfied in |
|---|---|---|
| 1 | Complete system architecture | [01-system-architecture](01-system-architecture.md) |
| 2 | Folder structure | [01-system-architecture](01-system-architecture.md) §Folder layout + `/backend`, `/frontend`, `/infra` scaffolds |
| 3 | Domain model | [03-domain-model](03-domain-model.md) |
| 4 | Database schema | [04-database-design](04-database-design.md) |
| 5 | API design | [05-api-design](05-api-design.md) |
| 6 | Frontend architecture | [06-frontend-architecture](06-frontend-architecture.md) |
| 7 | Deployment architecture | [08-deployment-architecture](08-deployment-architecture.md) |
| 8 | CI/CD architecture | [09-cicd-architecture](09-cicd-architecture.md) |
| 9 | Security architecture | [10-security-architecture](10-security-architecture.md) |
| 10 | Phased implementation roadmap | [12-development-roadmap](12-development-roadmap.md) |
| 11 | MVP scope + future roadmap | [15-mvp-plan](15-mvp-plan.md) |
| 12 | DESIGN.md | [../DESIGN.md](../DESIGN.md) |
| 13 | Enterprise coding standards | [01-system-architecture](01-system-architecture.md) §Engineering standards |
| 14 | Maintainability/scalability/observability/AI cost | Docs 01, 07, 11, 14 |
| 15 | DDD principles | [03-domain-model](03-domain-model.md) |
| — | C4 diagrams | [02-c4-diagrams](02-c4-diagrams.md) |
| — | AI architecture | [07-ai-architecture](07-ai-architecture.md) |
| — | Observability | [11-observability](11-observability.md) |
| — | Risk analysis | [13-risk-analysis](13-risk-analysis.md) |
| — | Cost estimation | [14-cost-estimation](14-cost-estimation.md) |
| — | Sprint plan | [16-sprint-plan](16-sprint-plan.md) |

## 7. Document index

| Doc | Title | What it covers |
|---|---|---|
| 00 | Overview | This document |
| 01 | System Architecture | Principles, layers, folder structure, standards |
| 02 | C4 Diagrams | Context, container, component, key sequences |
| 03 | Domain Model | Bounded contexts, aggregates, entities, events |
| 04 | Database Design | ERD, DDL, pgvector, migrations |
| 05 | API Design | REST resources, contracts, errors, versioning |
| 06 | Frontend Architecture | App Router, state, data, design integration |
| 07 | AI Architecture | Provider abstraction, RAG, prompts, cost |
| 08 | Deployment Architecture | AWS topology, IaC, environments |
| 09 | CI/CD Architecture | Pipelines, gates, releases |
| 10 | Security Architecture | AuthN/Z, threat model, data protection |
| 11 | Observability | Tracing, logging, metrics, SLOs |
| 12 | Development Roadmap | Phased delivery plan |
| 13 | Risk Analysis | Risk register + mitigations |
| 14 | Cost Estimation | Infra + AI cost model |
| 15 | MVP Plan | MVP scope vs future |
| 16 | Sprint Plan | Sprint-by-sprint breakdown |

## 8. Glossary

- **Analysis** — a generated, structured artifact (company/resume/JD) owned by a candidate.
- **Preparation (Prep)** — the fused output combining a company analysis, a resume, and a JD analysis.
- **Mock session** — a stateful AI interview conversation with turn-by-turn scoring.
- **Ingestion** — the pipeline that turns an uploaded artifact (file/URL/text) into clean text and structured data.
- **Provider** — an external LLM vendor (OpenAI/Claude/Gemini) behind the AI abstraction.
- **Slice** — a vertical feature unit in the Application layer (one use case).
- **Embedding** — a vector representation of text stored in pgvector for semantic retrieval.
