# Claude Code — Developer Guideline

This document explains every **slash command**, **subagent**, and **hook** available in this workspace, with usage examples and notes on when to reach for each one.

> **TL;DR** — The most important things to know:
> 1. Run `/code-review` before every commit.
> 2. Run `/update-test` after writing new logic.
> 3. Run `/deploy-checklist` before shipping a release.
> 4. Name the right subagent when a task is heavy (e.g., "use the `backend` agent to add a new vertical slice").

---

## Slash Commands (Skills)

Slash commands are invoked by typing `/command-name` in the Claude Code chat. Each one runs a structured workflow against your current working tree.

### `/code-review`

**What it does:** Reviews uncommitted or staged changes against this repo's standards — architecture compliance, naming, dead/duplicated code, security issues, missing validations, and missing logging.

**When to use:**
- Before every `git commit`
- After writing a new handler, endpoint, or component
- When you want a second opinion on a change

**Example:**
```
/code-review
```
Or with a specific target:
```
/code-review — just review the backend changes
```

**What it checks:**
- Clean Architecture dependency rule (Domain ← Application ← Infrastructure ← Api)
- Vertical slice shape: command/query + handler + validator + response
- No EF Core / HTTP / SDK types leaking into Domain or Application
- Parameter naming, `Result<T>` pattern, `CancellationToken` propagation
- Missing `[LoggerMessage]` or `[LoggerMessage]` delegates
- Security: no hardcoded secrets, all user input validated
- Test coverage for the changed slice

---

### `/update-test`

**What it does:** Generates or updates tests for changed code. Covers backend unit + integration tests and frontend component + hook tests.

**When to use:**
- After adding a new handler, validator, or domain method
- When coverage has dropped below 80%
- After renaming a method or changing an interface

**Example:**
```
/update-test
```
Or targeted:
```
/update-test — add integration tests for the UploadResume endpoint
```

**What it produces:**
- **Unit tests** (xUnit + FluentAssertions): Domain invariants/transitions; Application handlers with faked ports (NSubstitute)
- **Integration tests** (Testcontainers + real Postgres): endpoint → handler → EF round-trip
- Covers success path, validation failure, not-found, ownership/tenant isolation, and AI/parse failure paths

> **Note:** The PostToolUse hook reminds you to run `/update-test` every time a backend source file changes. This is intentional — tests must ship with the slice.

---

### `/review`

**What it does:** Full pull-request review — analyzes the entire PR diff, commits, branch naming, tests, and documentation, then produces an explicit approval recommendation.

**When to use:**
- When a branch is ready to merge
- To get a risk assessment before opening a PR
- When you want commit hygiene checked

**Example:**
```
/review
```
Or with a PR number:
```
/review PR #42
```

**Output includes:**
- Architecture, security, and quality findings across the whole diff
- Commit message quality (Conventional Commits)
- Test adequacy assessment
- Documentation completeness check
- **APPROVE / REQUEST CHANGES / BLOCK** decision with reasoning

---

### `/document`

**What it does:** Creates and maintains technical documentation — README updates, ADRs, API docs, Mermaid diagrams.

**When to use:**
- When you change an endpoint or data model (hook will remind you)
- When making a significant architectural decision (write an ADR)
- After a feature lands and the README or docs fall out of date

**Examples:**
```
/document — write an ADR for switching to pgvector ANN index
/document — update docs/05-api-design.md for the new AnalyzeResume endpoint
/document — add a sequence diagram for the resume ingestion flow
```

**ADR location:** `.claude/docs/adr/NNNN-title.md` (uses the TEMPLATE.md there)

> **Note:** The PostToolUse hook reminds you to update `docs/05-api-design.md` whenever an endpoint file changes.

---

### `/deploy-checklist`

**What it does:** Gate-checks a release across backend (build, tests, migrations), frontend (build, bundle), and infra (env vars, secrets, deploy configs). Produces a go/no-go report.

**When to use:**
- Before promoting to staging or production
- Before tagging a release

**Example:**
```
/deploy-checklist
```

**Checks include:**
- `dotnet build -c Release` with `TreatWarningsAsErrors`
- All tests passing (unit + integration + architecture)
- EF migrations present and backward-compatible (expand/contract pattern)
- No hardcoded secrets or connection strings
- Frontend builds and bundle stays within budget (≤150 KB JS gzipped for landing)
- Docker image builds and health checks respond

---

### `/learn`

**What it does:** Mines the codebase for recurring patterns and proposes improvements — new skills, rule additions, `CLAUDE.md` updates, ADRs. **Never applies changes automatically.**

**When to use:**
- After several features have landed and conventions have stabilized
- When you suspect a convention is being repeated but not codified

**Example:**
```
/learn
```

**Output:** A list of proposals (candidate patterns, gaps, drift) for you to review and approve individually before anything is changed.

---

## Subagents

Subagents are specialist AIs with specific tools and models. Claude Code delegates to them automatically for heavy tasks, or you can name one explicitly.

### How to invoke a subagent

Just tell Claude what you want and mention the agent:

```
Use the backend agent to add a GetJobDescription vertical slice.
Use the architect agent to review the data model for the Preparation bounded context.
```

Or Claude will pick the right one automatically based on the task.

---

### `architect` — System Design & Domain Modeling
**Model:** Opus (deepest reasoning)
**Tools:** Read, Grep, Glob, WebSearch, WebFetch

**Use for:**
- Designing a new bounded context or aggregate
- Evaluating architectural trade-offs (e.g., outbox vs direct event dispatch)
- Writing or reviewing an ADR
- Database schema decisions (normalization, indexing strategy, pgvector setup)
- C4 / ERD / sequence diagrams

**Example prompts:**
```
Use architect — design the data model for PrepPlan and MockInterview bounded contexts.
Use architect — evaluate whether to use CQRS read models or direct EF queries for the dashboard.
```

---

### `backend` — .NET 10 Implementation
**Model:** Sonnet (best coding)
**Tools:** Read, Edit, Write, Bash, Grep, Glob

**Use for:**
- Adding a new vertical slice (command/query + handler + validator + response + endpoint)
- Wiring Infrastructure adapters (repositories, AI providers, blob store)
- EF Core migrations and entity configurations
- MediatR pipeline behaviors
- ASP.NET Core minimal API endpoint groups

**Example prompts:**
```
Use backend — implement the AnalyzeJobDescription vertical slice following the Resume slice pattern.
Use backend — add a ResumeRepository.GetPagedAsync method with cursor-based pagination.
```

> **Note:** The backend agent follows the exact slice structure in `Application/Features/Resumes/`. Always reference this as the canonical example.

---

### `frontend` — Next.js 16 / React 19
**Model:** Sonnet
**Tools:** Read, Edit, Write, Bash, Grep, Glob

**Use for:**
- New pages, layouts, and components (App Router)
- TanStack Query hooks for data fetching
- Zustand store slices
- shadcn/ui component customization
- Tailwind 4 styling
- Framer Motion animations

**Example prompts:**
```
Use frontend — build the ResumeUploadPage with a dropzone and upload progress indicator.
Use frontend — add a useJobDescription hook with TanStack Query and optimistic update on save.
```

---

### `reviewer` — Code Review & Security
**Model:** Opus
**Tools:** Read, Grep, Glob, Bash

**Use for:**
- Independent review before a PR (less context bias than the author)
- Security audit of auth, file handling, or payment code
- Refactoring proposals with tradeoff analysis
- Hardening suggestions

**Example prompts:**
```
Use reviewer — audit the GlobalExceptionHandler and ResultExtensions for security issues.
Use reviewer — review the AiModelRouter for thread-safety and fallback correctness.
```

---

### `tester` — Test Generation & Coverage
**Model:** Sonnet
**Tools:** Read, Edit, Write, Bash, Grep, Glob

**Use for:**
- Writing xUnit unit + integration tests for a handler
- Writing Vitest component/hook tests
- Filling coverage gaps after implementing a feature
- Analyzing what failure paths are not yet tested

**Example prompts:**
```
Use tester — write xUnit tests for AnalyzeResumeHandler covering success, parse failure, and AI unavailable paths.
Use tester — write a Vitest test for useResume hook including loading, error, and refetch states.
```

---

### `devops` — Docker, GitHub Actions, AWS, Terraform
**Model:** Sonnet
**Tools:** Read, Edit, Write, Bash, Grep, Glob

**Use for:**
- Docker and `docker-compose` changes
- GitHub Actions CI/CD pipeline changes
- Terraform / AWS ECS / RDS / S3 configuration
- Observability wiring (OTel collector, Grafana, CloudWatch)
- Release and environment setup

**Example prompts:**
```
Use devops — add a GitHub Actions workflow that runs dotnet test with Testcontainers on PR.
Use devops — update the Terraform ECS task definition to add the new GEMINI_API_KEY env var.
```

---

### `ai-engineer` — Prompts, RAG, Embeddings, Provider Routing
**Model:** Opus
**Tools:** Read, Edit, Write, Bash, Grep, Glob, WebSearch

**Use for:**
- Prompt engineering and structured-output schema design
- pgvector similarity search tuning (ANN index, distance metric)
- Embedding pipeline work (chunking strategy, dimension pinning)
- Provider routing / tier / cost optimization in `AiCatalogOptions`
- Evaluating model output quality and hallucination risk

**Example prompts:**
```
Use ai-engineer — design the structured-output schema for the CompanyInsights analysis prompt.
Use ai-engineer — optimize the resume embedding pipeline: chunk size, overlap, and similarity threshold for retrieval.
```

---

## Hooks (Automatic — No Action Required)

These run silently in the background. Understanding them prevents confusion.

| Hook | When | What it does |
|------|------|--------------|
| **SessionStart** | Every new session | Injects project context + active ADRs |
| **PreToolUse (Bash)** | Before any shell command | Blocks dangerous ops (`rm -rf`, `git push --force`, `DROP TABLE`) |
| **PostToolUse (Edit/Write)** | After editing a backend file | Reminds to write/update xUnit tests and update API docs if an endpoint changed |
| **Stop** | When Claude finishes a session | Prints a wrap-up checklist: tests passing, docs updated, no secrets committed |

If you see a reminder message after an edit, it's the PostToolUse hook — not an error. It's telling you something needs to follow from your change.

---

## Typical Development Loop

```
1. DESIGN      — Use architect if the feature touches a new bounded context or data model.

2. IMPLEMENT   — Use backend (or frontend) to build the vertical slice.
                 Mirror the existing Resume slice shape exactly.

3. TEST        — Run /update-test (or use tester) to bring coverage to 80%+.
                 Every slice ships with its tests in the same commit.

4. REVIEW      — Run /code-review before committing.
                 Use reviewer for security-sensitive changes.

5. DOCUMENT    — Run /document if architecture, endpoints, or data model changed.
                 Write an ADR for significant or hard-to-reverse decisions.

6. SHIP        — Run /deploy-checklist before promoting to staging or production.

7. CODIFY      — Run /learn periodically to propose codifying emerging patterns.
```

---

## Quick Reference Card

| I want to... | Use |
|---|---|
| Review my uncommitted changes | `/code-review` |
| Add tests for new code | `/update-test` |
| Review a PR before merging | `/review` |
| Write/update an ADR or API doc | `/document` |
| Check if a release is safe to ship | `/deploy-checklist` |
| Propose improvements to standards | `/learn` |
| Design a new bounded context | `architect` agent |
| Implement a .NET slice/handler/endpoint | `backend` agent |
| Build a React page/component/hook | `frontend` agent |
| Security audit or independent review | `reviewer` agent |
| Write xUnit or Vitest tests | `tester` agent |
| CI/CD, Docker, AWS, Terraform | `devops` agent |
| Prompts, RAG, embeddings, AI cost | `ai-engineer` agent |

---

## Notes

- **Agents use Opus for design/review** (architect, reviewer, ai-engineer) and **Sonnet for implementation** (backend, frontend, tester, devops). This is intentional — use the right cost tier for the task.
- **Skills are read-only by nature** — `/code-review`, `/review`, and `/deploy-checklist` never write files. `/update-test`, `/document`, and `/learn` (proposals only) may write files.
- **Never hand-edit** `frontend/types/api.d.ts` (OpenAPI-generated) or EF Core migrations after they have been applied to any database.
- **The build gate is `TreatWarningsAsErrors=true`** — all analyzer warnings are hard errors. If the build breaks, use `dotnet build 2>&1` to get the full list, then fix in order (Domain → Application → Infrastructure → Api).
- **ADRs live at** `.claude/docs/adr/` — copy `TEMPLATE.md`, fill it in, and keep status current (`Proposed` → `Accepted` → `Deprecated`).

---

## Local Setup — Install, Build & Run

Three ways to run the stack locally. Pick the one that fits your goal:

| Option | Best for | Effort |
|--------|----------|--------|
| **A — Docker Desktop (full stack)** | Smoke-testing the whole system, demoing, onboarding | One command |
| **B — Manual (backing services in Docker, code on host)** | Day-to-day development with hot-reload | Medium |
| **C — VS Code debug (F5)** | Stepping through breakpoints in BE or FE code | Medium + one-time VS Code setup |

---

### Prerequisites

Install all tools before proceeding. Every option below requires them.

| Tool | Required version | Install |
|------|-----------------|---------|
| **Docker Desktop** | Latest stable | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop) |
| **.NET SDK** | 10.0.x | [dot.net/download](https://dot.net/download) |
| **Node.js** | 22 LTS | [nodejs.org](https://nodejs.org) |
| **dotnet-ef** (global tool) | Latest | `dotnet tool install -g dotnet-ef` |
| **VS Code** | Latest | [code.visualstudio.com](https://code.visualstudio.com) *(Option C only)* |

Verify everything is installed and on the correct version:

```bash
dotnet --version        # expect 10.x.x
node --version          # expect v22.x.x
docker info             # must show "Server" block — means Docker Desktop is running
dotnet ef --version     # expect Entity Framework Core tools x.x.x
```

---

### Option A — Docker Desktop (Full Stack)

Runs the entire application — Postgres, Redis, LocalStack, OTel collector, the compiled .NET API, and the Next.js frontend — inside Docker. No local .NET or Node.js needed to actually run it (only to build).

#### A1 — Install & start Docker Desktop

1. Download **Docker Desktop** from [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop) and install it.
2. Open Docker Desktop. Wait until the bottom-left status icon turns **green** and shows "Docker Desktop is running".
3. (macOS) Allow Docker Desktop in System Settings → Privacy & Security if prompted.

Confirm it is running from terminal:

```bash
docker info       # must print server info, not an error
docker version    # shows Client and Server versions
```

#### A2 — Start the full stack

```bash
cd infra
docker compose up --build
```

`--build` compiles both the .NET API and the Next.js frontend images on first run (takes 2–4 minutes). On subsequent runs you can omit it to reuse cached layers:

```bash
docker compose up
```

**What starts:**

| Container | What it is | Local URL |
|-----------|-----------|-----------|
| `postgres` | PostgreSQL 16 + pgvector | `localhost:5432` |
| `redis` | Redis 7 (cache + session) | `localhost:6379` |
| `localstack` | AWS S3 + SQS emulator | `localhost:4566` |
| `otel-collector` | OpenTelemetry collector | `localhost:4317` (gRPC) |
| `api` | .NET 10 API | `http://localhost:8080` |
| `web` | Next.js 16 frontend | `http://localhost:3000` |

The API is ready when you see:

```
api    | Now listening on: http://[::]:8080
```

Open the app at **http://localhost:3000**.

#### A3 — View logs in Docker Desktop

Open **Docker Desktop** → **Containers** tab. You will see the `interview-copilot` group. Click any container to see its live log stream, filter by keyword, or download the log file.

From terminal, follow logs for a specific service:

```bash
cd infra
docker compose logs -f api       # API logs only
docker compose logs -f web       # Frontend logs only
docker compose logs -f postgres  # Database logs
```

#### A4 — Connect to the database (Postgres)

Use any database GUI tool. Recommended: **DBeaver** (free, cross-platform) or **TablePlus** (Mac).

| Field | Value |
|-------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `interviewcopilot` |
| Username | `app` |
| Password | `localdev` |

From Docker Desktop → Containers → `postgres` → **Exec** tab, you can also run `psql` directly:

```bash
psql -U app -d interviewcopilot
```

#### A5 — Stop and reset

Stop all containers but keep the Postgres data volume:

```bash
cd infra
docker compose down
```

Full reset — destroys all containers **and** the Postgres volume (wipes all data):

```bash
docker compose down -v
```

Restart after a full reset re-runs from a clean database (no migrations applied).

#### A6 — Troubleshooting Docker Desktop

| Symptom | Fix |
|---------|-----|
| `Cannot connect to Docker daemon` | Docker Desktop is not running — open the app and wait for green status |
| Port `5432` already in use | A local Postgres is running — stop it: `brew services stop postgresql` (mac) or `sudo service postgresql stop` (linux) |
| Port `8080` / `3000` already in use | Find and kill the process: `lsof -i :8080` then `kill -9 <PID>` |
| `api` container exits immediately | Check logs: `docker compose logs api` — usually a missing env var or migration error |
| Images won't rebuild | Force rebuild: `docker compose build --no-cache` |
| Out of disk space | Prune unused Docker data: `docker system prune -f` |

---

### Option B — Manual Setup (hot-reload development)

Backing services (Postgres, Redis, etc.) run in Docker; the API and frontend run directly on your machine. You get full hot-reload on code changes without waiting for Docker image rebuilds.

#### B1 — Start only the backing services

```bash
cd infra
docker compose up postgres redis localstack otel-collector -d
```

The `-d` flag runs them in the background (detached). Confirm Postgres is healthy:

```bash
docker compose ps
# postgres should show "(healthy)" in the STATUS column
```

#### B2 — Configure backend secrets (first time only)

The API reads sensitive values from [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) locally. **Never put credentials in `appsettings.json`.**

```bash
cd backend/src/InterviewCopilot.Api

# Postgres (matches docker-compose default credentials)
dotnet user-secrets set "ConnectionStrings:Postgres" \
  "Host=localhost;Port=5432;Database=interviewcopilot;Username=app;Password=localdev"

# Auth (use any OIDC provider — Auth0, Keycloak, Azure AD, Cognito)
dotnet user-secrets set "Auth:Authority" "https://YOUR_IDP_DOMAIN/"
dotnet user-secrets set "Auth:Audience"  "interview-copilot-api"

# AI provider API keys (set only the ones you are using)
dotnet user-secrets set "Ai:OpenAi:ApiKey"  "sk-..."
dotnet user-secrets set "Ai:Claude:ApiKey"  "sk-ant-..."
dotnet user-secrets set "Ai:Gemini:ApiKey"  "AIza..."
```

Verify stored secrets:

```bash
dotnet user-secrets list
```

Secrets are stored in `~/.microsoft/usersecrets/<project-guid>/secrets.json` — never committed to git.

#### B3 — Restore and build backend

```bash
cd backend
dotnet restore
dotnet build
```

Both commands target `InterviewCopilot.slnx` and compile all projects at once.

#### B4 — Apply EF Core database migrations

Create the initial migration (only needed if no `Migrations/` folder exists yet):

```bash
cd backend
dotnet ef migrations add InitialCreate \
  --project src/InterviewCopilot.Infrastructure \
  --startup-project src/InterviewCopilot.Api
```

Apply all pending migrations to the running Postgres container:

```bash
dotnet ef database update \
  --project src/InterviewCopilot.Infrastructure \
  --startup-project src/InterviewCopilot.Api
```

> Postgres must be running (Step B1) for `database update` to succeed. The connection string comes from `appsettings.json` + user secrets.

#### B5 — Run the API

```bash
cd backend/src/InterviewCopilot.Api
dotnet run
```

API available at **http://localhost:8080**.
OpenAPI spec (development only): **http://localhost:8080/openapi/v1.json**.

For hot-reload on file save:

```bash
dotnet watch
```

#### B6 — Set up and run the frontend

```bash
cd frontend
npm install
```

Create your local environment file:

```bash
cp .env.example .env.local
```

Edit `.env.local` and set at minimum:

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:8080/api/v1
```

Generate the TypeScript API client from the OpenAPI spec (first time, and after any backend endpoint change):

```bash
npm run gen:api
```

Start the dev server:

```bash
npm run dev
```

Frontend available at **http://localhost:3000**.

---

### Option C — VS Code Debug (Breakpoints — F5)

Use this option when you need to step through code, inspect variables, or trace execution in the debugger. The `.vscode/` folder in this repo is pre-configured — no manual JSON editing required.

#### C1 — Install required VS Code extensions

Open VS Code in the project root, then open the Extensions panel (`Ctrl+Shift+X` / `Cmd+Shift+X`) and install all **workspace recommendations** (VS Code will prompt automatically):

| Extension | Purpose |
|-----------|---------|
| **C# Dev Kit** (`ms-dotnettools.csdevkit`) | .NET debugger, IntelliSense, test runner |
| **C#** (`ms-dotnettools.csharp`) | Language support (dependency of Dev Kit) |
| **ESLint** (`dbaeumer.vscode-eslint`) | JS/TS linting |
| **Prettier** (`esbenp.prettier-vscode`) | Code formatting |
| **Tailwind CSS IntelliSense** (`bradlc.vscode-tailwindcss`) | Class autocomplete |
| **Docker** (`ms-azuretools.vscode-docker`) | Manage containers from VS Code |
| **SQLTools + PG driver** (`mtxr.sqltools`, `mtxr.sqltools-driver-pg`) | Query Postgres without leaving VS Code |
| **GitLens** (`eamodio.gitlens`) | Inline git blame and history |
| **Error Lens** (`usernamehw.errorlens`) | Inline error display |

Or install all at once from the command line:

```bash
code --install-extension ms-dotnettools.csdevkit \
     --install-extension ms-dotnettools.csharp \
     --install-extension dbaeumer.vscode-eslint \
     --install-extension esbenp.prettier-vscode \
     --install-extension bradlc.vscode-tailwindcss \
     --install-extension ms-azuretools.vscode-docker \
     --install-extension mtxr.sqltools \
     --install-extension mtxr.sqltools-driver-pg \
     --install-extension eamodio.gitlens \
     --install-extension usernamehw.errorlens
```

#### C2 — Start backing services first

The debugger launches the API process directly on your machine — it still needs Postgres and Redis running:

```bash
cd infra
docker compose up postgres redis localstack otel-collector -d
```

Configure user secrets if you have not done so already (see Step B2 above).

Apply database migrations if needed (see Step B4 above).

#### C3 — Debug the backend (BE)

1. Open VS Code at the project root: `code "/Users/viethoang/Claude/Projects/Interview Copilot"`.
2. Open the **Run & Debug** panel (`Ctrl+Shift+D` / `Cmd+Shift+D`).
3. Select **`BE: Launch API (Debug)`** from the dropdown at the top.
4. Press **F5** (or click the green play button).

VS Code will:
- Build the backend solution (`BE: build` preLaunchTask)
- Launch `InterviewCopilot.Api.dll` with the CoreCLR debugger attached
- Open `http://localhost:8080` in your browser automatically when ready

**Set breakpoints** by clicking the gutter (left of the line number) in any `.cs` file. Execution will pause there when the line is hit.

**Useful debug actions:**
- `F5` — Continue
- `F10` — Step Over
- `F11` — Step Into
- `Shift+F11` — Step Out
- `Ctrl+Shift+F5` — Restart
- `Shift+F5` — Stop

#### C4 — Debug the frontend (FE)

##### Server-side debugging (API routes, server components)

1. In the Run & Debug panel, select **`FE: Next.js Server (Debug)`**.
2. Press **F5**.

VS Code launches `npm run dev` with `--inspect` (port 9229) and attaches the Node.js debugger. Set breakpoints in any server-side file (`app/` route handlers, server components, `lib/` utilities) and they will be hit.

##### Client-side debugging (React components, browser code)

Client-side breakpoints work best through the browser's built-in DevTools (`F12` → Sources tab). However, VS Code can also attach to Chrome:

1. Launch Chrome with remote debugging enabled:

   ```bash
   # macOS
   open -a "Google Chrome" --args --remote-debugging-port=9222

   # Windows
   chrome.exe --remote-debugging-port=9222
   ```

2. With the Next.js server already running (Step above), select **`FE: Attach Chrome (Client-Side)`** in VS Code and press **F5**.

VS Code will attach to the Chrome tab at `http://localhost:3000`. Breakpoints in `.tsx` / `.ts` files (client components) will be hit in VS Code.

#### C5 — Debug both at the same time (Full Stack)

1. In the Run & Debug panel, select **`Full Stack: BE + FE`** from the dropdown.
2. Press **F5**.

This compound configuration launches both `BE: Launch API (Debug)` and `FE: Next.js Server (Debug)` simultaneously. You can set breakpoints in both `.cs` and `.tsx`/`.ts` files in the same VS Code session.

To stop both: press `Shift+F5` (stops the active configuration) — or click the **Stop** button twice (once for each process in the debug toolbar).

#### C6 — Run VS Code tasks without debugging

Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`) → **Tasks: Run Task**. All project tasks are listed:

| Task | What it does |
|------|-------------|
| `BE: build` | `dotnet build` on the full solution |
| `BE: watch` | `dotnet watch` with hot-reload |
| `BE: test` | `dotnet test` with console output |
| `BE: EF apply migrations` | `dotnet ef database update` |
| `FE: install` | `npm install` in `frontend/` |
| `FE: gen:api` | Regenerates `types/api.d.ts` from OpenAPI spec |
| `Infra: start backing services` | `docker compose up postgres redis localstack otel-collector -d` |
| `Infra: start full stack (Docker)` | `docker compose up --build` |
| `Infra: stop` | `docker compose down` |

#### C7 — Troubleshooting VS Code debugging

| Symptom | Fix |
|---------|-----|
| `preLaunchTask 'BE: build' terminated with exit code 1` | Build failed — check the Problems panel for compiler errors. Run `dotnet build` in the terminal for the full output. |
| Breakpoints show as grey circles ("unverified") | The build has not been run yet, or the DLL path is stale. Run the `BE: build` task first, then F5. |
| `Cannot connect to runtime process, timeout` (FE) | Port 9229 is in use by another Node process. Kill it: `lsof -i :9229 \| awk 'NR>1 {print $2}' \| xargs kill -9` |
| API starts but returns 500 on all requests | Postgres is not running or migrations have not been applied. Run `Infra: start backing services` task, then `BE: EF apply migrations`. |
| Next.js starts but shows "API unreachable" | The API is not running, or `NEXT_PUBLIC_API_BASE_URL` in `.env.local` is wrong. Confirm `http://localhost:8080/health/live` returns 200. |
| C# extension shows "Loading…" for a long time | C# Dev Kit is still indexing. Wait 30–60 seconds after opening the project. If it stalls, run **C#: Restart Language Server** from the Command Palette. |

---

### Running Tests

#### Backend

```bash
cd backend

# All tests (unit + architecture)
dotnet test

# Specific project
dotnet test tests/InterviewCopilot.Domain.UnitTests
dotnet test tests/InterviewCopilot.Architecture.Tests

# Verbose output
dotnet test --logger "console;verbosity=normal"
```

> Integration tests (`Testcontainers`) spin up a real Postgres container automatically — Docker Desktop must be running.

#### Frontend

```bash
cd frontend

# Unit / component tests (Vitest)
npm test

# Watch mode
npm run test:watch

# Type-check (no emit)
npm run typecheck

# E2E (Playwright) — API must be running first
npm run e2e
```

---

### Common Commands Reference

| Task | Command |
|------|---------|
| Build backend (debug) | `cd backend && dotnet build` |
| Build backend (release, strict) | `cd backend && dotnet build -c Release -p:TreatWarningsAsErrors=true` |
| Run backend with hot-reload | `cd backend/src/InterviewCopilot.Api && dotnet watch` |
| Run all backend tests | `cd backend && dotnet test` |
| Add EF migration | `cd backend && dotnet ef migrations add <Name> --project src/InterviewCopilot.Infrastructure --startup-project src/InterviewCopilot.Api` |
| Apply EF migrations | `cd backend && dotnet ef database update --project src/InterviewCopilot.Infrastructure --startup-project src/InterviewCopilot.Api` |
| Revert last EF migration | `cd backend && dotnet ef migrations remove --project src/InterviewCopilot.Infrastructure --startup-project src/InterviewCopilot.Api` |
| Install frontend deps | `cd frontend && npm install` |
| Start frontend dev server | `cd frontend && npm run dev` |
| Frontend type-check | `cd frontend && npm run typecheck` |
| Frontend lint | `cd frontend && npm run lint` |
| Generate OpenAPI client types | `cd frontend && npm run gen:api` |
| Run frontend tests | `cd frontend && npm test` |
| Start backing services only | `cd infra && docker compose up postgres redis localstack otel-collector -d` |
| Start full stack in Docker | `cd infra && docker compose up --build` |
| View container logs | `cd infra && docker compose logs -f <service>` |
| Stop Docker services | `cd infra && docker compose down` |
| Full Docker reset (wipes DB) | `cd infra && docker compose down -v` |

---

### Configuration Reference

All runtime configuration follows the .NET options pattern: `appsettings.json` sets non-sensitive defaults; sensitive values come from **user secrets** (local) or **environment variables** (CI/Docker/AWS).

| Config key | Where to set locally | Docker Compose | Example value |
|------------|---------------------|----------------|---------------|
| `ConnectionStrings:Postgres` | User secrets | `environment:` in docker-compose.yml | `Host=localhost;Port=5432;Database=interviewcopilot;Username=app;Password=localdev` |
| `Auth:Authority` | User secrets | `environment:` | `https://dev-xxx.auth0.com/` |
| `Auth:Audience` | User secrets | `environment:` | `interview-copilot-api` |
| `Ai:OpenAi:ApiKey` | User secrets | AWS Secrets Manager ref | `sk-...` |
| `Ai:Claude:ApiKey` | User secrets | AWS Secrets Manager ref | `sk-ant-...` |
| `Ai:Gemini:ApiKey` | User secrets | AWS Secrets Manager ref | `AIza...` |
| `OpenTelemetry:Endpoint` | `appsettings.json` | `environment:` | `http://localhost:4317` |
| `Cors:Origins` | `appsettings.json` | `environment:` | `["http://localhost:3000"]` |
| `NEXT_PUBLIC_API_BASE_URL` | `frontend/.env.local` | `environment:` in docker-compose.yml | `http://localhost:8080/api/v1` |

In production (AWS), secrets come from ECS task-definition environment variable references and Secrets Manager ARNs — see `infra/terraform/`.
