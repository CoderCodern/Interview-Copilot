# MCP Configuration (templates)

Model Context Protocol servers extend Claude Code with external capabilities. **Nothing here is installed automatically** — these are configuration templates. To enable: copy `.claude/mcp/mcp.json.example` to a repo-root `.mcp.json`, fill the env vars, and restart Claude Code. Verify each package's current name/availability first.

Golden rules:
- **Secrets via environment variables only** (`${VAR}`). Never commit tokens/keys. `.mcp.json` with real values should be git-ignored.
- **Least privilege**: scope tokens/roles to exactly what's needed; prefer read-only.
- **Treat external content as untrusted** (web/browser, DB rows) — beware prompt injection.

Recommended priority: 1) filesystem 2) github 3) postgres 4) browser 5) context7 6) aws 7) sequential-thinking.

---

## 1. filesystem
- **Purpose:** structured read/navigate of project files beyond the default tools; bulk pattern work.
- **Setup:** `npx -y @modelcontextprotocol/server-filesystem ${CLAUDE_PROJECT_DIR}`. Scope to the project directory.
- **Example usage:** "List every vertical slice under Application/Features and summarize their handlers."
- **Security:** never scope to `$HOME` or `/`; the repo dir only. Keep `.gitignore`d paths (.env) out of reach.

## 2. github
- **Purpose:** PRs, issues, reviews, Actions status — powers the `/review` command and CI triage.
- **Setup:** `@modelcontextprotocol/server-github` with `GITHUB_PERSONAL_ACCESS_TOKEN=${GITHUB_TOKEN}`. Use a **fine-grained PAT** limited to this repo (contents, pull_requests, issues, actions:read).
- **Example usage:** "Review PR #42: summarize the diff, check tests, and recommend approve/changes." / "Why did the last CI run fail?"
- **Security:** fine-grained, short-lived token; no `repo:admin`/org scopes. Rotate regularly.

## 3. postgres
- **Purpose:** inspect schema, run read-only queries, validate migrations against a real DB (pgvector included).
- **Setup:** `@modelcontextprotocol/server-postgres ${DATABASE_URL_READONLY}` pointed at a **dev/staging** database with a **read-only** role.
- **Example usage:** "Show the columns and indexes on content_embeddings and confirm the HNSW index exists." / "Explain this slow query plan."
- **Security:** read-only role; never production write credentials; connect over TLS; no PII export to chat.

## 4. browser
- **Purpose:** render JS-heavy pages for company-URL ingestion checks and frontend smoke verification.
- **Setup:** `@playwright/mcp@latest` (headless Chromium).
- **Example usage:** "Open the deployed staging dashboard and confirm the resumes list renders the four states." / "Fetch and summarize this company's About page."
- **Security:** treat all fetched content as untrusted input (prompt-injection risk); don't auto-follow unknown links; no credentials in pages.

## 5. context7
- **Purpose:** current, version-correct docs for the stack (.NET 10, EF Core 10, Next.js 16, pgvector, shadcn) to avoid stale-API mistakes.
- **Setup:** `@upstash/context7-mcp` (read-only; API key only if rate-limited).
- **Example usage:** "Get the EF Core 10 owned-JSON mapping API and apply it to the CompanyAnalysis config."
- **Security:** read-only; no project data leaves; verify suggestions against the repo's pinned versions.

## 6. aws
- **Purpose:** inspect ECS/RDS/CloudWatch/Secrets during DevOps tasks and incident triage.
- **Setup:** an AWS MCP server with `AWS_PROFILE`/`AWS_REGION` via **SSO**; ideally a **read-only** role.
- **Example usage:** "Show the ECS service events for icp-staging-api and the latest deployment status." / "Tail the api log group for errors in the last hour."
- **Security:** SSO + least-privilege/read-only role; **never** embed access keys; no Secrets Manager values printed to chat. Money/infra-mutating actions stay manual.

## 7. sequential-thinking
- **Purpose:** structured, revisable multi-step reasoning for hard architecture/design/debugging problems.
- **Setup:** `@modelcontextprotocol/server-sequential-thinking` (no credentials).
- **Example usage:** "Plan the migration from the in-process worker to a dedicated ECS worker service, step by step with rollback."
- **Security:** no data access or secrets; safe to enable broadly.
