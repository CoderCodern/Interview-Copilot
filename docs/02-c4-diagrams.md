# C4 Architecture Diagrams

> **Document 02 of 16** · Depends on: [01-system-architecture](01-system-architecture.md)

This document presents the system using the [C4 model](https://c4model.com): System Context (Level 1), Containers (Level 2), Components (Level 3), plus key dynamic diagrams. All diagrams are Mermaid and render on GitHub.

---

## Level 1 — System Context

Who and what interacts with Interview Copilot AI.

```mermaid
flowchart TB
    candidate["👤 Candidate<br/><i>Job seeker preparing for interviews</i>"]
    admin["👤 Operator/Admin<br/><i>Support, billing, content moderation</i>"]

    subgraph platform["Interview Copilot AI"]
        sys["AI interview-prep SaaS<br/><i>Analyzes companies, resumes, JDs;<br/>generates prep plans; runs mock interviews</i>"]
    end

    openai["OpenAI API"]
    claude["Anthropic Claude API"]
    gemini["Google Gemini API"]
    auth["Identity Provider<br/><i>(Auth0/Cognito)</i>"]
    payments["Stripe<br/><i>Billing & subscriptions</i>"]
    email["Email provider<br/><i>(SES/Resend)</i>"]
    web["Company websites<br/><i>(URL ingestion)</i>"]

    candidate -->|uploads resume, runs analyses,<br/>practices mock interviews| sys
    admin -->|manages users, monitors usage| sys
    sys -->|chat & embeddings| openai
    sys -->|chat & embeddings| claude
    sys -->|chat & embeddings| gemini
    sys -->|authN/Z| auth
    sys -->|subscriptions, usage billing| payments
    sys -->|transactional email| email
    sys -->|fetches public pages| web

    style sys fill:#4f46e5,stroke:#312e81,color:#fff
    style platform fill:#0b1220,stroke:#4f46e5,color:#fff
```

## Level 2 — Container Diagram

The runnable/deployable units and their conversations.

```mermaid
flowchart TB
    candidate["👤 Candidate (browser)"]

    subgraph aws["AWS (VPC)"]
        spa["Web App<br/><b>Next.js 16 / React 19</b><br/><i>SSR + client; served via CloudFront</i>"]
        api["API<br/><b>ASP.NET Core 10</b><br/><i>REST, auth, CQRS via MediatR</i>"]
        worker["Worker<br/><b>.NET BackgroundService</b><br/><i>Ingestion + AI generation</i>"]
        db[("PostgreSQL 16 + pgvector<br/><b>Amazon RDS</b><br/><i>relational + embeddings</i>")]
        cache[("Redis<br/><b>ElastiCache</b><br/><i>cache, rate limits, locks</i>")]
        queue["Queue<br/><b>Amazon SQS</b><br/><i>jobs + DLQ</i>"]
        blob[("Object storage<br/><b>Amazon S3</b><br/><i>uploads, exports</i>")]
    end

    idp["Identity Provider"]
    llm["LLM Providers<br/>OpenAI · Claude · Gemini"]

    candidate -->|HTTPS| spa
    spa -->|HTTPS /api/v1 + Bearer JWT| api
    spa -. SSE/WebSocket live updates .- api
    api -->|read/write| db
    api -->|cache, rate-limit| cache
    api -->|enqueue jobs| queue
    api -->|presigned PUT/GET| blob
    api -->|validate JWT / JWKS| idp
    queue -->|dequeue| worker
    worker -->|read/write| db
    worker -->|read files| blob
    worker -->|chat & embeddings| llm
    worker -->|completion events| api

    style spa fill:#0ea5e9,stroke:#075985,color:#fff
    style api fill:#4f46e5,stroke:#312e81,color:#fff
    style worker fill:#7c3aed,stroke:#4c1d95,color:#fff
```

**Container responsibilities**

| Container | Tech | Responsibility | Scaling |
|---|---|---|---|
| Web App | Next.js 16 | UI, SSR, auth session, data fetching | CloudFront + Fargate/edge |
| API | ASP.NET Core 10 | Synchronous request handling, validation, authz, enqueue | Horizontal (stateless) on ECS Fargate |
| Worker | .NET worker | Async ingestion + AI generation | Horizontal by SQS depth |
| PostgreSQL | RDS + pgvector | System of record + vector search | Vertical + read replicas |
| Redis | ElastiCache | Cache, rate limiting, distributed locks | Cluster mode |
| SQS | SQS + DLQ | Durable job buffer | Managed |
| S3 | S3 | Document blobs, exports | Managed |

## Level 3 — Component Diagram (API container)

Inside the ASP.NET Core API, mapped to the Clean + Vertical Slice layers.

```mermaid
flowchart TB
    subgraph api["API container (ASP.NET Core 10)"]
        ep["Endpoint groups<br/><i>Companies, Resumes, JDs, Preparations, Mock</i>"]
        mw["Middleware<br/><i>exception→ProblemDetails, correlation, auth</i>"]

        subgraph appl["Application"]
            med["MediatR dispatcher + behaviors<br/><i>logging, validation, authz, UoW, perf</i>"]
            slices["Vertical slices<br/><i>commands/queries + handlers + validators</i>"]
            ports["Ports (interfaces)<br/><i>IChatCompletionService, IEmbeddingService,<br/>repositories, IUnitOfWork, IBlobStore</i>"]
        end

        subgraph dom["Domain"]
            agg["Aggregates + VOs + domain events"]
        end

        subgraph infra["Infrastructure (adapters)"]
            efc["EF Core repositories + DbContext"]
            airouter["AI model router + provider clients"]
            store["S3 blob store"]
            outbox["Outbox dispatcher"]
        end
    end

    ep --> mw --> med --> slices
    slices --> ports
    slices --> agg
    ports -. implemented by .-> efc
    ports -. implemented by .-> airouter
    ports -. implemented by .-> store
    efc --> agg
    efc --> outbox

    style dom fill:#064e3b,stroke:#10b981,color:#fff
    style appl fill:#1e3a8a,stroke:#60a5fa,color:#fff
    style infra fill:#78350f,stroke:#f59e0b,color:#fff
```

## Level 3 — Component Diagram (AI subsystem)

The provider-agnostic AI layer (detailed in Doc 07).

```mermaid
flowchart LR
    caller["Application slice<br/><i>e.g. GenerateInterviewPrep</i>"]
    router["AiModelRouter<br/><i>task → model policy</i>"]
    cache["Prompt/response cache<br/><i>Redis + provider prompt caching</i>"]
    budget["Token budgeter + cost meter"]
    factory["ProviderFactory"]

    oai["OpenAiClient"]
    cla["ClaudeClient"]
    gem["GeminiClient"]
    embed["EmbeddingService<br/>→ pgvector"]

    caller --> router
    router --> cache
    router --> budget
    router --> factory
    factory --> oai
    factory --> cla
    factory --> gem
    caller --> embed

    style router fill:#4f46e5,stroke:#312e81,color:#fff
```

## Dynamic — Company Analysis from a URL

```mermaid
sequenceDiagram
    autonumber
    actor U as Candidate
    participant W as Web App
    participant A as API
    participant Q as SQS
    participant K as Worker
    participant I as Ingestion
    participant R as AI Router
    participant P as LLM Provider
    participant D as Postgres+pgvector

    U->>W: Submit company URL
    W->>A: POST /api/v1/companies/analyses {sourceType:url}
    A->>D: insert CompanyAnalysis (status=Pending)
    A->>Q: enqueue AnalyzeCompanyJob(id)
    A-->>W: 202 Accepted {id, status:Pending}
    W-->>U: show "Analyzing…" with live updates
    Q->>K: deliver job
    K->>I: fetch + clean page text
    K->>R: structure(company text, task=company_overview)
    R->>P: chat completion (cheapest qualifying model)
    P-->>R: structured sections
    R-->>K: CompanyAnalysisResult
    K->>D: upsert sections + embeddings; status=Completed
    K->>A: publish completion event
    A-->>W: SSE: analysis.completed {id}
    W-->>U: render company report
```

## Dynamic — AI Mock Interview turn

```mermaid
sequenceDiagram
    autonumber
    actor U as Candidate
    participant W as Web App
    participant A as API
    participant R as AI Router
    participant P as LLM Provider
    participant D as Postgres

    U->>W: Type answer to interviewer question
    W->>A: POST /api/v1/mock-sessions/{id}/messages {answer}
    A->>D: load session state + rubric + transcript
    A->>R: score(answer, rubric) + nextQuestion(context)
    R->>P: chat (structured: score, feedback, follow-up)
    P-->>R: {score, feedback, followUp}
    R-->>A: turn result
    A->>D: append turn, update running score
    A-->>W: 200 {score, feedback, nextQuestion}
    W-->>U: show feedback + next question
```

## Deployment view (preview)

Full topology, IaC, and environments are in [08-deployment-architecture](08-deployment-architecture.md). At a glance: CloudFront → ALB → ECS Fargate services (api, worker) in private subnets → RDS, ElastiCache, SQS, S3, Secrets Manager, all inside one VPC across two AZs.
