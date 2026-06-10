# DESIGN.md — Interview Copilot AI Design System

> The visual and interaction language for Interview Copilot AI. **"Folio"** — a warm, editorial system that feels like *a premium interview preparation notebook designed for professionals*: a paper canvas, warm neutral surfaces, **Newsreader** serif headings over **Inter** UI text, a single bronze accent, layered warm shadows, and hairline borders. Calm, trustworthy, built for long study sessions. Implemented with **Tailwind 4 + shadcn/ui + Framer Motion** (Doc 06). This file is the single source of truth for tokens; `app/globals.css` mirrors it.

---

## 1. Design principles

1. **Warm paper, one accent.** A warm-neutral canvas with a single bronze accent (`#8C7B68`). Never blue, purple, or neon; no glassmorphism; no loud gradients. Hierarchy comes from surface tiers, type contrast, and the accent used sparingly.
2. **Content is the interface.** The candidate's resume, the company report, the questions — these are the product. Chrome stays quiet; content gets the space.
3. **Show the work, earn trust.** Grounded answers cite sources; AI states are explicit (analyzing, completed, low-confidence). Never a silent magic box.
4. **Editorial type, generous space.** Serif headings (Newsreader) set the notebook tone; Inter keeps UI crisp. Progress and completion are always visible.
5. **Calm over clever.** Optimize for readability, trust, and long sessions — not for developer-tool density or SaaS-template flash.
6. **Accessible by default.** WCAG 2.2 AA contrast, keyboard-complete, reduced-motion aware, semantic structure.

## 2. Brand

- **Personality:** warm, considered, quietly premium — a beautifully bound notebook, not a dashboard.
- **Logomark:** an italic-serif **"ic"** monogram on a bronze gradient tile + wordmark **"Interview Copilot"** in the heading serif.
- **Tagline:** *"Walk in prepared."*
- **Signature surface:** the landing hero — a full-screen video scrubbed by horizontal mouse movement, typewriter line, pill CTAs. Inner app pages use the warm paper canvas with elevated panels.

## 3. Color

A warm-neutral palette with **four surface tiers** (background → surface → surface-raised → hover) and a single bronze accent. Semantic colors are used strictly for status. Token **names are stable** (so existing utilities keep working).

### Light theme (default)

| Token | Value | Use |
|---|---|---|
| `--background` | `#FAF9F6` | App canvas (warm paper) |
| `--surface` | `#FDFCF9` | Cards, panels |
| `--surface-raised` | `#FFFFFE` | Hovered cards, dropdowns, dialogs |
| `--hover` / `--surface-muted` | `#F6F2ED` | Hovered rows, pressed states |
| `--foreground` | `#171717` | Primary text |
| `--muted-foreground` | `#6F6A64` | Secondary text |
| `--faint-foreground` | `#99928A` | Tertiary text, captions |
| `--border` | `#E6E0D8` | Hairlines |
| `--border-strong` | `#D8D0C4` | Hovered-card borders |
| `--accent` | `#8C7B68` | The accent — links, active nav, progress, icon chips |
| `--accent-deep` | `#6E5F4F` | Gradient partner, emphasis |
| `--accent-soft` / `--accent-softer` / `--accent-ring` | 10% / 6% / 28% accent | Tints for chips, fills, rings |
| `--primary` | `#8C7B68` | Primary actions (bronze button) |
| `--primary-foreground` | `#FBFAF7` | Text on primary |
| `--track` | `#ECE7DF` | Progress meter track |
| `--success` | `#2A7A52` | Completed, positive scores |
| `--warning` | `#8A6018` | Degraded, near-budget, medium gaps |
| `--danger` | `#C0283C` | Errors, critical gaps |
| `--ring` | `#8C7B68` | Focus ring |

### Dark theme (.dark — "notebook on a wooden desk at night")

| Token | Value | Use |
|---|---|---|
| `--background` | `#3D3732` | App canvas |
| `--surface` | `#4A443E` | Cards |
| `--surface-raised` | `#514B44` | Hovered cards, overlays |
| `--hover` / `--surface-muted` | `#565049` | Hovered rows |
| `--foreground` | `#F5F1EB` | Primary text |
| `--muted-foreground` | `#C4BCB2` | Secondary text |
| `--border` | `#655E57` | Hairlines |
| `--border-strong` | `#756D64` | Hovered-card borders |
| `--accent` | `#A38D75` | Accent (lighter for dark bg) |
| `--accent-deep` | `#BCA68C` | Emphasis |
| `--primary` | `#A38D75` | Primary actions |
| `--primary-foreground` | `#2D2822` | Text on primary |
| `--track` | `#36312C` | Meter track |
| `--success` | `#52C98B` | Positive |
| `--warning` | `#E0B23B` | Caution |
| `--danger` | `#F76E82` | Errors |

### Semantic usage rules

- **Paper first, bronze second.** The accent appears in small, meaningful doses: active nav, primary buttons, progress fills, icon chips, completion marks. Never large accent fills.
- **Forbidden:** blue accents, purple accents, neon, glassmorphism, rainbow charts, excessive gradients (the only gradient is the subtle accent→accent-deep on primary buttons and meters).
- **Primary action = bronze gradient button**, one per screen; secondary = bordered ghost that fills with `--hover`.
- Contrast: verify muted/semantic pairings in CI (axe).

## 4. Typography

A two-face editorial pairing loaded via `next/font` (self-hosted, no FOUT): **Newsreader** (serif) for headings, display numbers, and the logomark — exposed as `--font-heading`; **Inter** for body and UI — `--font-body`. Geist Mono for code.

- **Headings / logo / big numerals:** `var(--font-heading)` (Newsreader), weight 500, tracking -0.015em. Italic for editorial accents.
- **Body / UI:** `var(--font-body)` (Inter), weight 400–550.
- **Mono:** `Geist Mono` for code, tokens, JSON.

| Token | Size / line-height | Use |
|---|---|---|
| `text-display` | clamp(40px, 8vw, 84px) / 1.0 | Hero / landing |
| `text-h1` | clamp(28px, 4.5vw, 38px) / 1.15 | Page titles (serif) |
| `text-h2` | 25 / 1.25 | Section titles (serif) |
| `text-h3` | 19 / 1.2 | Card titles (serif) |
| `text-body` | 14–15 / 1.5 | Default UI |
| `text-eyebrow` | 11.5 / 1, +0.1em, uppercase, accent | Section eyebrows |
| `text-sm` | 13.5 / 1.5 | Secondary, captions |
| `text-mono` | 14 / 1.5 | Code/tokens |

Weights: serif headings 500–600, body 400, UI labels 500–550. Numerals tabular for scores/usage; stat numbers render in the serif at 26px+.

## 5. Spacing, radius, elevation

- **Spacing scale (4px base):** 0, 1=4, 2=8, 3=12, 4=16, 6=24, 8=32, 12=48, 16=64. Sections breathe at 48–96. Card grids use 16px gaps (never `gap-px` fused grids — cards are discrete panels).
- **Radius:** cards & panels `14px` (`--radius`); buttons & inputs `9–10px`; chips/badges `9999px`.
- **Elevation — warm layered shadows + hairlines.** Every panel = `--surface` fill + 1px `--border` + `--shadow-sm` + `--inset-line` (hairline inner top highlight). Hover raises to `--surface-raised`, `--border-strong`, `--shadow-md`, and a `-3px` translateY. Shadows are brown-tinted (`--shadow-color: 56,46,36`), never gray-blue. Overlays (dropdown, dialog, palette) use `--shadow-md`/`--shadow-lg`.
- **Texture:** a near-invisible SVG grain on `body::before` keeps the paper canvas from feeling flat.
- **Interaction states:** hover = lift + raise + reveal affordance ("Open →" slides in); active = settle back (`-1px`); focus = 2px `--accent-ring` outline.
- **Progress is first-class:** `.meter` bars (accent gradient fill on `--track`), radial score dials, and completion checkmarks appear wherever work has a state.

## 6. Layout

- **Global top navbar (fixed, z-10):** logo + ✳︎ left; grouped nav center — **Dashboard**, **Prepare** (dropdown: Companies, Resumes, Job Descriptions, Preparations), **Mock Interview**; CTA right (Get started / Account). Animated hamburger → full-screen `bg-white/95 backdrop-blur` overlay on mobile. Two variants: **overlay** (transparent, over the landing video) and **solid** (white, hairline bottom border, for app pages).
- **Landing:** full `h-screen` video hero (the only video surface).
- **App pages:** white canvas under the fixed navbar; content max-width 1100–1200px, reading content max 720px, centered, generous top padding.
- **Responsive:** desktop links collapse into the mobile overlay below `md`. Mock interview and prep view are mobile-usable.

## 7. Components (shadcn/ui baseline + product specifics)

**Buttons.** Variants: `primary` (bronze gradient `accent→accent-deep`, `--primary-foreground` text, `shadow-sm`, hover lifts 1px + `shadow-md`) · `ghost` (transparent, `--border-strong` border, hover fills `--hover`) · radius `9–10px`, `text-[13.5–14px]` medium. Inputs: `--surface`, hairline border, accent focus ring, 10px radius. Status chips stay `rounded-full`.

**Product components:**

| Component | Notes |
|---|---|
| `SiteNav` | shared fixed top navbar (overlay/solid variants) with the Prepare dropdown + mobile overlay |
| `StatusPill` | pending/processing/completed/failed — bordered mono pill with a status-colored dot |
| `SourceDropzone` | drag/drop + URL + paste; hairline dashed border; accepted types + size limit |
| `AnalysisCard` | hairline card: title, status, created date, quick actions; hover darkens border |
| `SectionBlock` | a company-analysis section: heading, prose, highlight chips |
| `QuestionAccordion` | question → expand to follow-ups + STAR scaffold |
| `StarEditor` | four-field S/T/A/R scaffold with source-experience link |
| `GapList` | skill gaps with severity dots (success/warning/danger) + recommendation |
| `RoadmapTimeline` | milestones with time estimates, vertical timeline |
| `ScoreDial` | mock-interview score as a radial dial, mono with one status band |
| `ChatTurn` | interviewer/candidate bubbles; streaming caret; per-turn score tag |
| `UsageMeter` | tokens/cost vs plan budget; warns at 80% |

**States are first-class:** every data surface ships `loading` (skeleton), `empty` (single CTA), `error` (ProblemDetails `code` → friendly message + retry), and `success`.

## 8. Motion (Framer Motion)

Motion is quick, purposeful, and interruptible.

| Pattern | Spec |
|---|---|
| Page/section enter | fade + 8px rise, 180–240ms, ease-out |
| List stagger | 30–40ms per item, cap ~8 |
| Typewriter | 38ms/char, 600ms start delay, blinking caret until done (landing) |
| Pill reveal | opacity + 8px translate, 0.4s, 400ms after load |
| Hamburger → X | bars rotate ±45° / fade, 300ms |
| Hover | 120–200ms color only (pill inversion) |

- **Durations:** micro 120ms, standard 180–200ms, entrance 240ms. Nothing over ~400ms.
- **`prefers-reduced-motion`:** all non-essential motion disabled; replaced with instant opacity.
- Never animate layout in a way that blocks input or shifts content under the cursor.

## 9. Iconography & illustration

- **Icons:** `lucide-react`, 1.8px stroke, 17–20px, accent-colored inside soft accent chips (`bg-accent-soft` + `--accent-ring` border, 10px radius). Restrained metaphor set (file, building, target, sparkle for AI, messages for mock).
- **AI affordance:** a single consistent "sparkle" mark denotes AI-generated content and feedback.
- **Illustration:** none by default — type, whitespace, and the paper grain carry the page. The italic-serif monogram is the one decorative mark.

## 10. Data visualization

- Charts (recharts) are warm-monochrome: accent series on the paper canvas with hairline gridlines; a single status color only where it encodes meaning.
- Scores use the success/warning/danger bands; never rainbow.
- Usage/cost labels tabular-num.

## 11. Voice & tone

- **Coachable, specific, encouraging.** "Quantify the result and name the trade-off" — not "Good job!".
- **Honest about uncertainty:** "We couldn't confirm this from the source" beats a confident guess.
- **Plain language**, second person, active voice. Error messages say what happened and what to do next.
- No dark patterns, no fake urgency.

## 12. Accessibility checklist

- [ ] AA contrast (trivial in monochrome; verify muted/semantic pairs via axe in CI)
- [ ] Full keyboard path incl. nav dropdown, mobile overlay, dialogs, chat
- [ ] Visible focus ring (`--ring`) on every interactive element
- [ ] Semantic landmarks + headings; ARIA only where needed
- [ ] `prefers-reduced-motion` honored
- [ ] Form fields labeled + errors announced (aria-live)
- [ ] Live regions for async status changes (SSE-driven)
- [ ] Targets ≥ 24×24px (AA 2.2)

## 13. Token → code mapping

`app/globals.css` declares these as CSS variables under `:root` (light, default) and `.dark`; `tailwind` exposes them via `@theme inline`; shadcn maps its semantic names onto them. `--font-heading` (Newsreader) / `--font-body` (Inter) load via `next/font` in the root layout; `--font-mono` is Geist Mono. Reusable primitives live beside the tokens: `.panel`, `.panel-interactive`, `.meter`. Changing a value here changes it everywhere.

---

*"Folio" warm-editorial system — a premium interview preparation notebook. Supersedes the monochrome "Mainframe" template (ADR `0004`).*
