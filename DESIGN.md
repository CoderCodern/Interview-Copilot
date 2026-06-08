# DESIGN.md — Interview Copilot AI Design System

> The visual and interaction language for Interview Copilot AI. A **warm, editorial** system inspired by Anthropic / Claude — a parchment canvas, a single terracotta accent, serif headlines, and exclusively warm-toned neutrals. The product should feel like a calm, literate coach, not a cold dashboard. Implemented with **Tailwind 4 + shadcn/ui + Framer Motion** (Doc 06). This file is the single source of truth for tokens; `app/globals.css` mirrors it.

---

## 1. Design principles

1. **Calm confidence.** Interview prep is stressful; the UI should feel like a composed coach, not a noisy dashboard. Generous whitespace, quiet color, one clear action per view.
2. **Content is the interface.** The candidate's resume, the company report, the questions — these are the product. Chrome recedes; content gets contrast and space.
3. **Show the work, earn trust.** Grounded answers cite sources; AI states are explicit (analyzing, completed, low-confidence). Never a silent magic box.
4. **Editorial, human warmth.** Every surface reads like a well-set page: serif headlines for authority, generous line-height for reading, warm paper tones instead of clinical white-on-blue.
5. **Accessible by default.** WCAG 2.2 AA contrast, keyboard-complete, reduced-motion aware, semantic structure.

## 2. Brand

- **Personality:** sharp, supportive, modern. A senior mentor who is precise but warm — thoughtful companion, not a powerful machine.
- **Logomark:** a stylized "compass + chat" mark (guidance + conversation). Wordmark in the **serif** display face, normal weight.
- **Tagline:** *"Walk in prepared."*
- **Atmosphere:** a literary salon reimagined as a product — parchment paper, terracotta ink, organic rather than geometric.

## 3. Color

Built on a warm **parchment** canvas with a single confident **terracotta** primary and warm-toned semantics. Every neutral has a yellow-brown undertone — **no cool blue-grays anywhere**. Defined as CSS variables, exposed to Tailwind as `--color-*` and consumed by shadcn tokens. Token **names are stable**; only values changed when we adopted the warm system, so existing utility classes keep working.

### Light theme (default — parchment)

| Token | Value | Use |
|---|---|---|
| `--background` | `#F5F4ED` | App canvas (Parchment) |
| `--surface` | `#FAF9F5` | Cards, sheets (Ivory) |
| `--surface-muted` | `#E8E6DC` | Subtle panels, buttons (Warm Sand) |
| `--foreground` | `#141413` | Primary text (Near Black) |
| `--muted-foreground` | `#5E5D59` | Secondary text (Olive Gray) |
| `--border` | `#E8E6DC` | Hairlines (Border Warm) |
| `--primary` | `#C96442` | Primary actions (Terracotta) |
| `--primary-foreground` | `#FAF9F5` | Text on primary (Ivory) |
| `--primary-hover` | `#B5563A` | Hover |
| `--accent` | `#D97757` | Highlights, links (Coral) |
| `--success` | `#3F7A4F` | Completed, positive scores |
| `--warning` | `#A8741C` | Degraded, near-budget, medium gaps |
| `--danger` | `#B53333` | Errors, critical gaps (Error Crimson) |
| `--ring` | `#C96442` | Focus ring |

### Dark theme (warm charcoal — optional)

| Token | Value | Use |
|---|---|---|
| `--background` | `#141413` | App canvas (Deep Dark) |
| `--surface` | `#1E1E1C` | Cards |
| `--surface-muted` | `#30302E` | Panels (Dark Surface) |
| `--foreground` | `#F5F4ED` | Primary text |
| `--muted-foreground` | `#B0AEA5` | Secondary text (Warm Silver) |
| `--border` | `#30302E` | Hairlines (Border Dark) |
| `--primary` | `#D97757` | Primary actions (Coral — brighter on dark) |
| `--primary-foreground` | `#141413` | Text on primary |
| `--primary-hover` | `#C96442` | Hover |
| `--accent` | `#D97757` | Highlights |
| `--success` | `#6CBA7F` | Positive |
| `--warning` | `#D8A24A` | Caution |
| `--danger` | `#E0685F` | Errors |

### Semantic usage rules

- **One primary action per screen.** Secondary actions use Warm Sand / `outline` / `ghost` variants.
- **Terracotta is precious.** Reserve `--primary` for the single highest-signal action and brand moments; it's the only chromatic color in the chrome.
- **Score & gap colors are functional, not decorative:** success/warning/danger map to score bands and gap severity consistently everywhere.
- **No cool grays.** If a neutral looks blue, it's wrong — pull it toward warm sand/olive.
- Contrast: body text ≥ 4.5:1, large text ≥ 3:1, UI affordances ≥ 3:1 — verified in CI (axe).

## 4. Typography

**Serif for authority, sans for utility.** Headlines carry a serif at weight 500 (book-title gravitas); all functional UI text uses a neutral sans. Body line-height runs generous (1.6) for a reading experience closer to a book than a dashboard.

- **Display / headings:** `Source Serif 4` (substitute for Anthropic Serif; fallback `Georgia, serif`) — weight **500**, the ceiling. No bold serifs.
- **Body / UI:** `Geist` (substitute for Anthropic Sans; fallback `system-ui`) — neutral, excellent at small sizes.
- **Mono:** `Geist Mono` for code, tokens, JSON.

| Token | Font | Size / line-height | Weight | Use |
|---|---|---|---|---|
| `text-display` | Serif | 64 / 1.10, normal tracking | 500 | Marketing hero |
| `text-h1` | Serif | 36 / 1.15 | 500 | Page titles |
| `text-h2` | Serif | 28 / 1.20 | 500 | Section titles |
| `text-h3` | Serif | 21 / 1.20 | 500 | Card titles, feature names |
| `text-body` | Sans | 16 / 1.6 | 400 | Default |
| `text-prose` | Sans/Serif | 17 / 1.6, max 68ch | 400 | Long-form analysis reading |
| `text-sm` | Sans | 14 / 1.5 | 400 | Secondary, captions |
| `text-label` | Sans | 12 / 1.4, +0.12px | 500 | Badges, small labels |
| `text-mono` | Mono | 14 / 1.6 | 400 | Code/tokens |

Weights: serif headings fixed at **500**; sans body 400, UI labels 500. **Never** bold (700+) on serif. Numerals tabular for scores/usage.

## 5. Spacing, radius, elevation

- **Spacing scale (4px base):** 0, 1=4, 2=8, 3=12, 4=16, 6=24, 8=32, 12=48, 16=64. Section rhythm uses 24/32/48; major marketing sections breathe at 80–120.
- **Radius (soft, approachable):** `--radius` = 12px (inputs, primary buttons, cards). Generous: 16px featured cards, 24–32px hero containers / embedded media, 8px small secondary buttons, 999px pills/avatars. **Never** sharp corners under 6px.
- **Elevation — ring shadows, not drop shadows.** Depth comes from warm-toned ring halos and background luminance shifts (parchment → ivory → sand → charcoal), not heavy shadows.
  - `ring` (Level 2): `0 0 0 1px` in warm gray (`#D1CFC5`) — interactive cards/buttons.
  - `whisper` (Level 3): `0 4px 24px rgba(0,0,0,0.05)` — elevated feature cards, screenshots.
  - `inset` (Level 4): `inset 0 0 0 1px rgba(0,0,0,0.15)` — active/pressed.
- **Borders** do most structural work: `1px` hairlines in `--border` (warm cream/sand). Light/dark section alternation creates the biggest depth moments.

## 6. Layout

- **App shell:** fixed left sidebar (nav: Dashboard, Companies, Resumes, JDs, Preparations, Mock), top bar (search/command palette, usage meter, account). Content max-width 1200px; reading content max 720px.
- **Grid:** 12-col fluid; cards snap to a 4/8/12 rhythm.
- **Editorial pacing:** sections breathe like a magazine spread; alternate parchment and near-black "chapters" for emphasis on marketing surfaces.
- **Command palette (⌘K):** primary navigation accelerator — jump to any analysis, start a new one.
- **Responsive:** sidebar collapses to icons < 1024px, to a sheet < 768px. Mock interview and prep view are mobile-usable.

## 7. Components (shadcn/ui baseline + product specifics)

**Primitives (shadcn):** Button, Input, Textarea, Select, Dialog, Sheet, Tabs, Card, Badge, Tooltip, Toast (Sonner), Skeleton, Progress, Avatar, DropdownMenu, Command. Restyled to the tokens above via `components.json`.

**Button variants:** `primary` (Terracotta, ivory text, 12px radius) · `secondary` (Warm Sand, charcoal text, ring shadow, 8px) · `ghost` (transparent, hover to sand) · `dark` (Near Black, on light emphasis). Inputs focus with a blue ring (`#3898EC`) — the single intentional cool accent, used purely for accessibility.

**Product components:**

| Component | Notes |
|---|---|
| `StatusPill` | pending/processing/completed/failed with icon + color; the canonical async indicator |
| `SourceDropzone` | drag/drop + URL + paste; shows accepted types and size limit |
| `AnalysisCard` | list item with title, status, created date, quick actions |
| `SectionBlock` | a company-analysis section: serif heading, prose, highlight chips |
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
- **Illustration:** organic, hand-drawn-feeling line work in terracotta, near-black, and muted green for empty states and concept art — never geometric/tech clip-art or stock.

## 10. Data visualization

- Charts (recharts) inherit theme tokens; max 1 accent (terracotta/coral) + warm neutrals per chart.
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

`app/globals.css` declares these as CSS variables under `:root` (light/parchment, the default) and `.dark` (warm charcoal); `tailwind` exposes them as theme colors via `@theme inline`; shadcn maps its semantic names (`--background`, `--primary`, `--ring`, …) onto them. Changing a value here changes it everywhere. The serif display face is wired as `--font-serif`. The scaffold in `frontend/app/globals.css` is generated from this document.

---

*Adapted from the Claude (Anthropic) design language in [VoltAgent/awesome-design-md](https://github.com/VoltAgent/awesome-design-md), tuned for Interview Copilot AI. See ADR `0003-warm-editorial-design-system.md`.*
