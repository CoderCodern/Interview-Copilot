# 0002. Backend uses Clean Architecture + Vertical Slice + CQRS on .NET 10

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** Architecture, Backend
- **Tags:** backend, architecture

## Context
Interview Copilot AI needs a maintainable, testable backend that scales in feature count without growing god-services, while keeping business rules independent of frameworks. See `docs/01-system-architecture.md` and `docs/03-domain-model.md`.

## Decision
We will combine **Clean Architecture** (Domain / Application / Infrastructure / Api with an inward dependency rule) for macro boundaries with **Vertical Slice Architecture** for feature organization, using **CQRS via MediatR**. Domain stays dependency-free; Application defines ports and hosts slices (command/query + handler + validator + response); Infrastructure implements adapters (EF Core, AI providers, S3); Api is the composition root. The dependency rule is enforced by NetArchTest architecture tests and the PreToolUse hook. Business outcomes use `Result<T>` rather than exceptions.

## Consequences
- **Positive:** cohesive, independently evolvable features; pure, fast-to-test domain; replaceable infrastructure; provider-agnostic AI.
- **Negative / costs:** more upfront structure and ceremony (markers, behaviors, mapping); contributors must learn the slice pattern.
- **Follow-ups:** every new feature follows the Resume slice as the reference; keep architecture tests green.

## Alternatives considered
- **Traditional N-layer (Controllers/Services/Repositories)** — tends toward god-services and cross-folder feature edits; rejected.
- **Pure vertical slices without Clean boundaries** — risks infrastructure leaking into business logic; rejected.
- **Exceptions for control flow** — less explicit error contracts; rejected in favor of `Result<T>`.
