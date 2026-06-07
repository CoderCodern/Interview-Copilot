# DESIGN.md — Interview Copilot AI Design System

> The visual and interaction language for Interview Copilot AI. Inspired by the restraint and precision of premium AI/developer SaaS (Linear, Vercel, Stripe, Anthropic). Implemented with **Tailwind 4 + shadcn/ui + Framer Motion** (Doc 06). This file is the single source of truth for tokens; `app/globals.css` mirrors it.

---

## 1. Design principles

1. **Calm confidence.** Interview prep is stressful; the UI should feel like a composed coach, not a noisy dashboard. Generous whitespace, quiet color, one clear action per view.
2. **Content is the interface.** The candidate's resume, the company report, the questions — these are the product. Chrome recedes; content gets contrast and space.
3. **Show the work, earn trust.** Grounded answers cite sources; AI states are explicit (analyzing, completed, low-confidence). Never a silent magic box.
4. **Fast and legible.** Motion is functional and quick; type is set for long-form reading; nothing blocks the first paint.
5. **Accessible by default.** WCAG 2.2 AA contrast, keyboard-complete, reduced-motion aware, semantic structure.

## 2. Brand

- **Personality:** sharp, supportive, modern. A senior mentor who is precise but warm.
- **Logomark:** a stylized "compass + chat" mark (guidance + conversation). Wordmark in the display font, tight tracking.
- **Tagline:** *"Walk in prepared."*

## 3. Color

Built on a near-neutral slate canvas with a single confident **indigo/violet** primary and semantic accents. Defined as CSS variables (OKLCH-friendly hex shown), exposed to Tailwind as `--color-*` and consumed by shadcn tokens.

### Light theme

| Token | Value | Use |
|---|---|---|
| `--background` | `#FBFBFD` | App canvas |
| `--surface` | `#FFFFFF` | Cards, sheets |
| `--surface-muted` | `#F4F4F7` | Subtle panels, code |
| `--foreground` | `#0B0B12` | Primary text |
| `--muted-foreground` | `#5B5B6B` | Secondary text |
| `--border` | `#E6E6EC` | Hairlines |
| `--primary` | `#5B5BD6` | Primary actions, focus |
| `--primary-foreground` | `#FFFFFF` | Text on primary |
| `--primary-hover` | `#4A4ABF` | Hover |
| `--accent` | `#7C5CFC` | Highlights, gradients |
| `--success` | `#1A9E6C` | Completed, positive scores |
| `--warning` | `#C98A00` | Degraded, near-budget, medium gaps |
| `--danger` | `#D6455D` | Errors, critical gaps |
| `--ring` | `#5B5BD6` | Focus ring |

### Dark theme (default for the app shell)

| Token | Value | Use |
|---|---|---|
| `--background` | `#0A0A0F` | App canvas |
| `--surface` | `#121219` | Cards |
| `--surface-muted` | `#1A1A24` | Panels, code |
| `--foreground` | `#ECECF1` | Primary text |
| `--muted-foreground` | `#9A9AAB` | Secondary text |
| `--border` | `#26262F` | Hairlines |
| `--primary` | `#7C7CF0` | Primary actions |
| `--primary-foreground` | `#0A0A0F` | Text on primary |
| `--accent` | `#9D7BFF` | Highlights |
| `--success` | `#3DD68C` | Positive |
| `--warning` | `#E0A93B` | Caution |
| `--danger` | `#FF6B85` | Errors |

### Semantic usage rules

- **One primary action per screen.** Secondary actions use `ghost`/`outline` variants.
- **Score & gap colors are functional, not decorative:** success/warning/danger map to score bands and gap severity consistently everywhere.
- **Gradients** (accent → primary) are reserved for the marketing surface and the empty-state hero; never behind body text.
- Contrast: body text ≥ 4.5:1, large text ≥ 3:1, UI affordances ≥ 3:1 — verified in CI (axe).

## 4. Typography

- **Display / UI:** `Geist` (or `Inter`) — geometric, neutral, excellent at small sizes.
- **Reading (long analysis prose):** `Geist` at comfortable measure, or a humanist serif option toggle for reports.
- **Mono:** `Geist Mono` / `JetBrains Mono` for code, tokens, JSON.

| Token | Size / line-height | Use |
|---|---|---|
| `text-display` | 48 / 1.05, tracking -0.02em | Marketing hero |
| `text-h1` | 30 / 1.2 | Page titles |
| `text-h2` | 24 / 1.25 | Section titles |
| `text-h3` | 19 / 1.3 | Card titles |
| `text-body` | 15 / 1.6 | Default |
| `text-prose` | 16 / 1.7, max 68ch | Long-form analysis reading |
| `text-sm` | 13 / 1.5 | Secondary, captions |
| `text-mono` | 13 / 1.5 | Code/tokens |

Weights: 400 body, 500 UI labels, 600 headings. Avoid heavier than 600. Numerals tabular for scores/usage.

## 5. Spacing, radius, elevation

- **Spacing scale (4px base):** 0, 1=4, 2=8, 3=12, 4=16, 6=24, 8=32, 12=48, 16=64. Section rhythm uses 24/32/48.
- **Radius:** `--radius` = 12px (cards), 8px (inputs/buttons), 999px (pills/avatars). Soft but not bubbly.
- **Elevation (dark-first):** prefer borders + subtle shadow over heavy drop shadows.
  - `shadow-sm`: 0 1px 2px rgba(0,0,0,.25)
  - `shadow-md`: 0 4px 16px rgba(0,0,0,.30)
  - `shadow-pop`: 0 8px 40px rgba(0,0,0,.45) (dialogs, command palette)
- **Borders** do most of the structural work (1px hairlines in `--border`), echoing Linear/Vercel.

## 6. Layout

- **App shell:** fixed left sidebar (nav: Dashboard, Companies, Resumes, JDs, Preparations, Mock), top bar (search/command palette, usage meter, account). Content max-width 1200px; reading content max 720px.
- **Grid:** 12-col fluid; cards snap to a 4/8/12 rhythm.
- **Command palette (⌘K):** primary navigation accelerator — jump to any analysis, start a new one.
- **Responsive:** sidebar collapses to icons < 1024px, to a sheet < 768px. Mock interview and prep view are mobile-usable.

## 7. Components (shadcn/ui baseline + product specifics)

**Primitives (shadcn):** Button, Input, Textarea, Select, Dialog, Sheet, Tabs, Card, Badge, Tooltip, Toast (Sonner), Skeleton, Progress, Avatar, DropdownMenu, Command. Restyled to the tokens above via `components.json`.

**Product components:**

| Component | Notes |
|---|---|
| `StatusPill` | pending/processing/completed/failed with icon + color; the canonical async indicator |
| `SourceDropzone` | drag/drop + URL + paste; shows accepted types and size limit |
| `AnalysisCard` | list item with title, status, created date, quick actions |
| `SectionBlock` | a company-analysis section: heading, prose, highlight chips |
| `QuestionAccordion` | question → expand to follow-ups + STAR scaffold |
| `StarEditor` | four-field S/T/A/R scaffold with source-experience link |
| `GapList` | skill gaps with severity chips (success/warning/danger) + recommendation |
| `RoadmapTimeline` | milestones with time estimates, vertical timeline |
| `ScoreDial` | mock-interview score as a radial dial, color-banded |
| `ChatTurn` | interviewer/candidate bubbles; streaming caret; per-turn score tag |
| `UsageMeter` | tokens/cost vs plan budget; warns at 80% |

**States are first-class:** every data surface ships `loading` (skeleton), `empty` (illustration + single CTA), `error` (ProblemDetails `code` → friendly message + retry), and `success`.

## 8. Motion (Framer Motion)

Motion is quick, purposeful, and interruptible.

| Pattern | Spec |
|---|---|
| Page/section enter | fade + 8px rise, 180–240ms, ease-out |
| List stagger | 30–40ms per item, cap ~8 |
| Dialog/sheet | scale 0.98→1 + fade, 160ms |
| Status → completed | gentle check pop, single play |
| Chat turn / streaming | token fade-in; typing indicator dots |
| Hover | 120ms color/elevation only |

- **Durations:** micro 120ms, standard 180ms, entrance 240ms. Nothing over ~300ms.
- **`prefers-reduced-motion`:** all non-essential motion disabled; replaced with instant opacity. Gated globally via a `useReducedMotion` wrapper.
- Never animate layout in a way that blocks input or shifts content under the cursor.

## 9. Iconography & illustration

- **Icons:** `lucide-react`, 1.5px stroke, 20px default. Consistent metaphor set (compass, sparkle for AI, target for JD, file for resume, building for company).
- **AI affordance:** a single consistent "sparkle" mark denotes AI-generated content and feedback — applied uniformly so users always know what's model-generated.
- **Illustration:** minimal line + single-accent style for empty states; no stock clip-art.

## 10. Data visualization

- Charts (recharts) inherit theme tokens; max 1 accent + neutrals per chart.
- Scores use the success/warning/danger bands; never rainbow.
- Usage/cost charts are calm, gridlines subtle, labels tabular-num.

## 11. Voice & tone

- **Coachable, specific, encouraging.** "Quantify the result and name the trade-off" — not "Good job!".
- **Honest about uncertainty:** "We couldn't confirm this from the source" beats a confident guess.
- **Plain language**, second person, active voice. Error messages say what happened and what to do next.
- No dark patterns, no fake urgency.

## 12. Accessibility checklist

- [ ] AA contrast (verified by axe in CI)
- [ ] Full keyboard path incl. command palette, dialogs, chat
- [ ] Visible focus ring (`--ring`) on every interactive element
- [ ] Semantic landmarks + headings; ARIA only where needed
- [ ] `prefers-reduced-motion` honored
- [ ] Form fields labeled + errors announced (aria-live)
- [ ] Live regions for async status changes (SSE-driven)
- [ ] Targets ≥ 24×24px (AA 2.2)

## 13. Token → code mapping

`app/globals.css` declares these as CSS variables under `:root` (light) and `.dark` (dark); `tailwind` exposes them as theme colors; shadcn maps its semantic names (`--background`, `--primary`, `--ring`, …) onto them. Changing a value here changes it everywhere. The scaffold in `frontend/app/globals.css` is generated from this document.
