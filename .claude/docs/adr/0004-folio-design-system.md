# 0004. Adopt "Folio" warm-editorial design system

- **Status:** Accepted
- **Date:** 2026-06-10
- **Deciders:** Frontend, Design
- **Tags:** frontend, design-system

## Context

Interview Copilot AI launched with a monochrome "Mainframe" template (pure white canvas, black ink, Helvetica Now, hairline borders, pill buttons). While technically crisp, it tested as cold and developer-tool-like — misaligned with the brand intent of *calm, trustworthy, premium preparation*. The font stack relied on a CDN-hosted Helvetica Now which introduced a FOUT risk and a third-party dependency. The design had no dark mode, a single surface tier, and flat elevation (no shadow system), making it difficult to build the layered card-grid layouts needed for dashboard and study-track pages. See `DESIGN.md` for the full token specification.

## Decision

We will adopt the **"Folio" warm-editorial design system** across the full frontend:

- **Canvas:** warm paper (`#FAF9F6`) replacing pure white; 4-tier surface hierarchy (background → surface → surface-raised → hover).
- **Accent:** a single bronze tone (`#8C7B68`) for all affordance: active nav, primary buttons, progress fills, icon chips, completion marks. No blue, no purple, no neon.
- **Typography:** `Newsreader` (serif) for headings and display numbers + `Inter` for body and UI, both loaded via `next/font` (self-hosted, no FOUT). Geist Mono for code.
- **Elevation:** warm-tinted layered shadows (`shadow-color: 56, 46, 36`) + 1px warm borders + hairline inner highlight (`--inset-line`) so panels read as paper, not wireframes.
- **Dual theme:** light ("a high-quality paper notebook in daylight") and dark ("a premium notebook on a wooden desk at night"), toggled by Zustand store persisted to `localStorage` with an anti-FOUC blocking script. WCAG AA verified: 16.4:1 (light) / 10.9:1 (dark) for foreground; 5.0:1 / 5.6:1 for muted-foreground.
- **Reusable primitives** in `globals.css`: `.panel`, `.panel-interactive`, `.eyebrow`, `.btn`, `.btn-primary`, `.btn-ghost`, `.badge`, `.meter`.
- **Token names are stable** — all existing `bg-background`, `text-foreground`, `border-border` utilities continue to work; only the resolved values changed.
- Reference implementation: `docs/design/interview-prep-dashboard.html`.

## Consequences

- **Positive:** visual identity that matches brand personality (calm, trustworthy, editorial); dark mode ships day one; warm shadow + surface hierarchy enable rich card layouts; self-hosted fonts eliminate FOUT and CDN dependency; WCAG AA contrast verified in both themes; `.panel` primitives DRY up repeated card boilerplate.
- **Negative / costs:** CSS token surface grew substantially (73 → 328 lines); any future contributor must learn the token vocabulary before adding surfaces; the `--shadow-color` approach (RGB tuple as a custom property) is non-standard and can surprise; paper-grain texture in `body::before` adds a tiny SVG to every page render.
- **Follow-ups:** run automated contrast audit (axe / Pa11y) in CI against the new palette; add `SiteNav` and `StatusPill` component tests; build out remaining dashboard surfaces (stat strip, study-track cards) using `.panel`/`.panel-interactive` primitives.

## Alternatives considered

- **Keep Mainframe, add warmth incrementally** — would require two design languages coexisting during migration; high drift risk. Rejected.
- **Use a pre-built component library theme (Radix, shadcn default)** — locks us into a token vocabulary we cannot fully own; makes the "notebook" feel impossible. Rejected.
- **System-level dark mode only (`prefers-color-scheme`)** — no user control, no toggle in-app, conflicts with the targeted calm/focused UX. Rejected in favor of the explicit Zustand toggle.
