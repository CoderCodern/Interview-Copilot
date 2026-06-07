---
name: documentation
description: Create and maintain technical documentation — README updates, ADRs, API docs, and Mermaid sequence/architecture diagrams — keeping docs in sync when architecture or contracts change. Use for "document this", "write an ADR", "update the README/API docs", or /document.
---

# Skill: documentation

## Purpose
Keep the repository's documentation accurate and useful, and produce new docs (ADRs, API references, diagrams) when architecture or contracts change.

## Activation criteria
- User runs `/document` or asks to write/update docs, an ADR, API docs, or a diagram.
- A change altered architecture, data model, endpoints, infra, or AI policy (triggers CLAUDE.md §5).

## Workflow
1. Identify what changed and which docs are affected: `README.md`, `docs/01..16`, `DESIGN.md`, `.claude/docs/adr/`, OpenAPI/`docs/05-api-design.md`.
2. **README** — update setup, structure, commands, status when they change.
3. **ADR** — for significant/irreversible decisions, copy `.claude/docs/adr/TEMPLATE.md` to the next `NNNN-title.md`; fill Context, Decision, Consequences, Alternatives; status `Proposed` -> `Accepted`.
4. **API docs** — reflect endpoint/contract changes in `docs/05-api-design.md`; ensure OpenAPI spec + generated client stay in sync.
5. **Diagrams** — author **Mermaid** (sequence, flowchart, ER, C4) inside the relevant doc; keep fences balanced and declarations valid.
6. Match the existing house style (prose-first, minimal bullet noise, tables where they help). Verify internal links resolve.

## Expected output
- Updated/created markdown with correct cross-links and valid Mermaid.
- For an ADR: a complete record using the template.
- A one-line note of every file touched and why.

## Example
> /document  (after adding mock-interview streaming)
Updates `docs/05-api-design.md` (SignalR turn streaming), adds a Mermaid sequence diagram of a streamed turn, and writes `.claude/docs/adr/0003-signalr-for-mock-streaming.md`. Touched: 2 files + 1 ADR.
