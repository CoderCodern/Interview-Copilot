# Cost Estimation

> **Document 14 of 16** · Depends on: [07-ai-architecture](07-ai-architecture.md), [08-deployment-architecture](08-deployment-architecture.md) · Deliverable: Cost Estimation

Two cost drivers: **AI tokens** (variable, the dominant lever) and **AWS infrastructure** (semi-fixed). All AI prices are **list prices as of June 2026** (per 1M tokens) and live in the `ModelCatalog` config so they update without code changes (Doc 07 §2). Numbers are planning estimates, not quotes.

---

## 1. AI model price reference (June 2026, per 1M tokens)

| Provider | Model | Input | Output | Tier | Notes |
|---|---|---|---|---|---|
| Google | Gemini 3.1 Flash-Lite | $0.25 | $1.50 | Economy | Cheapest workhorse |
| OpenAI | GPT-5.4 Nano | $0.20 | $1.25 | Economy | Ultra-budget |
| Anthropic | Claude Haiku 4.5 | $1.00 | $5.00 | Economy | Fast, strong quality |
| Google | Gemini 3.5 Flash | $1.50 | $9.00 | Standard | |
| Google | Gemini 3.1 Pro | $2.00 | $12.00 | Standard/Premium | Batch: $1.00 / $6.00 |
| OpenAI | GPT-5.4 | $2.50 | $15.00 | Standard | |
| Anthropic | Claude Sonnet 4.6 | $3.00 | $15.00 | Standard | Default for synthesis |
| Anthropic | Claude Opus 4.8 | $5.00 | $25.00 | Premium | Flagship reasoning |
| OpenAI | GPT-5.5 | $5.00 | $30.00 | Premium | Flagship |

**Cost levers** (Doc 07 §7): prompt caching cuts cached input 75–90%; Batch API ~50%; RAG cuts input tokens by retrieving top-k instead of full documents; response cache makes re-runs free.

## 2. Per-operation token & cost model

Assumed token sizes per operation and the tier/model the router would pick:

| Operation | Model (tier) | Input tok | Output tok | Cost/op |
|---|---|---|---|---|
| Resume parse | Gemini 3.1 Flash-Lite (Economy) | 4,000 | 2,000 | **$0.0040** |
| JD analysis | Gemini 3.1 Flash-Lite (Economy) | 3,000 | 1,500 | **$0.0030** |
| Company analysis | Claude Sonnet 4.6 (Standard) | 9,000 | 4,000 | **$0.087** |
| Preparation (fusion) | Claude Sonnet 4.6 (Standard/Prem) | 12,000 | 6,000 | **$0.126** |
| Mock turn (×10/session) | Claude Sonnet 4.6 (Standard) | 3,000 | 600 | **$0.18** (10 turns) |
| Embeddings (per analysis) | embedding model | ~small | — | ~$0.001 |

**One full candidate journey** (1 resume + 1 JD + 1 company + 1 prep + a 10-turn mock):

- **Without optimization:** ≈ **$0.40**
- **With prompt caching on company + prep** (≈60% of input cacheable at 90% off): ≈ **$0.366**

Caching the stable prompt prefix and retrieved context drops Preparation from **$0.126 → $0.107** and Company from **$0.087 → $0.072** per op.

### Premium routing comparison (Preparation, 12k in / 6k out)

| Model | Cost/prep |
|---|---|
| Gemini 3.1 Pro | $0.096 |
| Claude Sonnet 4.6 | $0.126 |
| Claude Opus 4.8 | $0.210 |
| GPT-5.5 | $0.240 |

> The router defaults Preparation to Sonnet 4.6 for value; it can promote to Opus/GPT-5.5 for "deep mode" (a paid feature) or demote to Gemini Pro when budget-constrained — same code, config policy.

## 3. AI cost at scale (optimized journey ≈ $0.366, 2 journeys/active user/mo)

| Active users | Journeys/mo | AI spend/mo | Per active user |
|---|---|---|---|
| 1,000 | 2,000 | **≈ $730** | $0.73 |
| 10,000 | 20,000 | **≈ $7,300** | $0.73 |
| 50,000 | 100,000 | **≈ $36,600** | $0.73 |

Batch processing for non-interactive work and heavier caching can push the per-user figure lower; "deep mode" on premium models pushes a subset higher (priced accordingly).

## 4. AWS infrastructure (monthly, planning estimates)

### Production (modest scale, Multi-AZ)

| Service | Config | Est. $/mo |
|---|---|---|
| ECS Fargate | api/web/worker, autoscaled baseline ~6 tasks | $300–600 |
| RDS PostgreSQL | db.r6g.large, Multi-AZ + storage/backup | $400–550 |
| ElastiCache Redis | cache.t4g.medium | $90–120 |
| ALB | + LCUs | $20–40 |
| NAT Gateway | 1–2 AZ + data | $35–70 |
| S3 | storage + requests | $10–40 |
| CloudFront | CDN + egress | $20–100 |
| SQS | jobs + DLQ | < $5 |
| Secrets Manager / SSM | secrets | $5–15 |
| CloudWatch / OTel backend | logs/metrics/traces | $50–200 |
| **Prod subtotal** | | **≈ $950–1,750/mo** |

### Non-production

| Env | Est. $/mo |
|---|---|
| staging (prod-like, smaller) | $300–500 |
| dev (single-AZ, small) | $150–300 |

> A managed observability/IdP vendor (Datadog/Honeycomb, Auth0) would add SaaS line items; the self-managed (CloudWatch + Cognito) path is assumed above to keep the floor low.

## 5. Blended cost & unit economics

Example at **10,000 active users**:

| Item | $/mo |
|---|---|
| AI (optimized) | ~$7,300 |
| Infra (prod) | ~$1,400 |
| Non-prod | ~$700 |
| 3rd-party SaaS (email, error tracking, etc.) | ~$300 |
| **Total** | **≈ $9,700/mo** |
| **Cost per active user** | **≈ $0.97/mo** |

At a Pro price point of, say, $15–20/mo, gross margin on AI+infra is healthy provided **cost-per-completed-prep stays tracked** (Doc 11 §8) and free-tier usage is bounded by quotas (Doc 10 §8). The free tier's cost (≈$0.40–0.73/journey) is the main subsidy to manage — capped via per-plan budgets.

## 6. Cost controls in place (recap)

1. **Tiered routing** — Economy models for the high-volume extraction tasks (the bulk of calls) keep average cost low.
2. **Prompt caching + RAG** — shrink input tokens on the expensive synthesis tasks.
3. **Batch API** — ~50% off for non-interactive jobs (bulk re-embedding, async company analysis).
4. **Response cache** — re-running the same input is free.
5. **Per-plan budgets** — soft-degrade (tier down / defer to batch) before hard stop; alerts at 80% (Doc 11).
6. **Unit-economics dashboard** — cost per completed prep tracked per release; regressions are visible immediately.
7. **Reserved/Savings Plans + Fargate Spot (worker)** — once baseline load is known, commit-based discounts and Spot for interruptible workers cut infra 20–50%.

## 7. Cost forecasting

Forecast = (projected active users) × (journeys/user) × (per-journey AI cost by feature mix) + (infra step function at scale tiers). The model lives as a spreadsheet fed by the `token_usage` table actuals so estimates self-correct against real data. Provider price changes are a single `ModelCatalog` edit and immediately reflected in projections.
