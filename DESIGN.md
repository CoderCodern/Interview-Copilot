# DESIGN.md — Interview Copilot AI Design System

> The visual and interaction language for Interview Copilot AI. A **monochrome, editorial** template inspired by the "Mainframe" creative-agency hero: a white canvas, black ink, **Helvetica Now Display** type, pill buttons, hairline borders, and a single full-screen mouse-scrub video reserved for the marketing landing. Stark, fast, confident. Implemented with **Tailwind 4 + shadcn/ui + Framer Motion** (Doc 06). This file is the single source of truth for tokens; `app/globals.css` mirrors it.

---

## 1. Design principles

1. **Monochrome confidence.** Black on white, almost no color. Hierarchy comes from type scale, weight, and whitespace — not hue. Color appears only as functional status.
2. **Content is the interface.** The candidate's resume, the company report, the questions — these are the product. Chrome is a thin hairline; content gets the space.
3. **Show the work, earn trust.** Grounded answers cite sources; AI states are explicit (analyzing, completed, low-confidence). Never a silent magic box.
4. **Editorial type, generous space.** Helvetica Now at large sizes with tight tracking sets the tone; pill buttons and thin rules are the only ornament.
5. **Accessible by default.** WCAG 2.2 AA contrast (trivial in monochrome), keyboard-complete, reduced-motion aware, semantic structure.

## 2. Brand

- **Personality:** sharp, modern, quietly premium — a studio, not a dashboard.
- **Logomark:** wordmark **"Interview Copilot®"** in the heading face + a decorative asterisk **✳︎**. Tight tracking, black.
- **Tagline:** *"Walk in prepared."*
- **Signature surface:** the landing hero — a full-screen video scrubbed by horizontal mouse movement, black copy, a typewriter line, and pill CTAs. Inner app pages drop the video for a clean white canvas.

## 3. Color

Two values do almost everything: **black** ink on a **white** canvas, with `black/10` hairline borders. Semantic colors are the only chromatic exception and are used strictly for status. Token **names are stable** (so existing utilities keep working); only values changed.

### Light theme (default)

| Token | Value | Use |
|---|---|---|
| `--background` | `#FFFFFF` | App canvas |
| `--surface` | `#FFFFFF` | Cards, sheets (separated by hairlines, not fills) |
| `--surface-muted` | `#F4F4F2` | Subtle panels, hover, code |
| `--foreground` | `#000000` | Primary text |
| `--muted-foreground` | `#6B6B6B` | Secondary text |
| `--border` | `rgba(0,0,0,0.10)` | Hairlines |
| `--primary` | `#000000` | Primary actions (black pill / button) |
| `--primary-foreground` | `#FFFFFF` | Text on primary |
| `--primary-hover` | `#1A1A1A` | Hover |
| `--accent` | `#000000` | Emphasis (monochrome) |
| `--success` | `#1F7A4D` | Completed, positive scores |
| `--warning` | `#8A6500` | Degraded, near-budget, medium gaps |
| `--danger` | `#B00020` | Errors, critical gaps |
| `--ring` | `#000000` | Focus ring |

### Dark theme (inverted — optional)

| Token | Value | Use |
|---|---|---|
| `--background` | `#000000` | App canvas |
| `--surface` | `#0A0A0A` | Cards |
| `--surface-muted` | `#161616` | Panels, code |
| `--foreground` | `#FFFFFF` | Primary text |
| `--muted-foreground` | `#A1A1A1` | Secondary text |
| `--border` | `rgba(255,255,255,0.14)` | Hairlines |
| `--primary` | `#FFFFFF` | Primary actions (white pill) |
| `--primary-foreground` | `#000000` | Text on primary |
| `--primary-hover` | `#E5E5E5` | Hover |
| `--accent` | `#FFFFFF` | Emphasis |
| `--success` | `#4ADE80` | Positive |
| `--warning` | `#E0B23B` | Caution |
| `--danger` | `#FF5470` | Errors |

### Semantic usage rules

- **Black and white first.** Reach for hue only for status (`success`/`warning`/`danger`); everything else is ink, paper, and hairline.
- **Primary action = solid black** (inverts to solid white on dark); secondary = white pill with hairline border that inverts to black on hover.
- **One primary action per screen.** Pills are the system's only button shape.
- Contrast: black-on-white is ~21:1; verify only the muted/semantic pairings in CI (axe).

## 4. Typography

The whole site uses **Helvetica Now Display** — `…-Medium` for headings/logo, `…W01-Rg` for body and UI — loaded via stylesheet links in the root layout, exposed as `--font-heading` / `--font-body`. Big sizes, tight tracking, generous leading on body.

- **Headings / logo:** `var(--font-heading)` (Helvetica Now Display Medium), weight 500, tracking tight.
- **Body / UI:** `var(--font-body)` (Helvetica Now Display Regular), weight 400.
- **Mono:** `Geist Mono` for code, tokens, JSON.

| Token | Size / line-height | Use |
|---|---|---|
| `text-display` | clamp(40px, 8vw, 84px) / 1.0, tracking -0.02em | Hero / landing |
| `text-h1` | clamp(30px, 5vw, 44px) / 1.05 | Page titles |
| `text-h2` | 28 / 1.15 | Section titles |
| `text-h3` | 20 / 1.2 | Card titles |
| `text-body` | clamp(15px, 2vw, 17px) / 1.5 | Default |
| `text-lead` | clamp(18px, 4vw, 26px) / 1.35 | Hero subcopy, typewriter line |
| `text-sm` | 14 / 1.5 | Secondary, captions |
| `text-pill` | 13–15 / 1 | Pill button labels |
| `text-mono` | 14 / 1.5 | Code/tokens |

Weights: headings 500, body 400, UI labels 400–500. Avoid heavier than 500. Numerals tabular for scores/usage.

## 5. Spacing, radius, elevation

- **Spacing scale (4px base):** 0, 1=4, 2=8, 3=12, 4=16, 6=24, 8=32, 12=48, 16=64. Sections breathe at 48–96.
- **Radius:** pills/avatars `9999px` (the dominant shape); cards & inputs `8px`; the canvas stays crisp. `--radius` = 0.5rem.
- **Elevation — hairlines, not shadows.** Depth is a `1px` `black/10` rule and whitespace. The system is flat by intent; reserve any shadow for true overlays (command palette, dialog) and keep it soft.
- **Borders** do the structural work everywhere: `border border-black/10` on cards, inputs, pills, and the navbar's bottom edge.

## 6. Layout

- **Global top navbar (fixed, z-10):** logo + ✳︎ left; grouped nav center — **Dashboard**, **Prepare** (dropdown: Companies, Resumes, Job Descriptions, Preparations), **Mock Interview**; CTA right (Get started / Account). Animated hamburger → full-screen `bg-white/95 backdrop-blur` overlay on mobile. Two variants: **overlay** (transparent, over the landing video) and **solid** (white, hairline bottom border, for app pages).
- **Landing:** full `h-screen` video hero (the only video surface).
- **App pages:** white canvas under the fixed navbar; content max-width 1100–1200px, reading content max 720px, centered, generous top padding.
- **Responsive:** desktop links collapse into the mobile overlay below `md`. Mock interview and prep view are mobile-usable.

## 7. Components (shadcn/ui baseline + product specifics)

**Buttons are pills.** Variants: `default` (white, `border-black/10`, hover → black/white) · `primary` (solid black, white text) · `outline` (transparent, current border) · all `rounded-full`, `text-[13px] sm:text-[15px]`, `px-4 sm:px-5 py-[0.3em]`, `transition-colors duration-200`. Inputs: white, hairline border, black focus ring, 8px radius.

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

- **Icons:** `lucide-react`, 1.5px stroke, 20px default, black. Restrained metaphor set (file, building, target, sparkle for AI, messages for mock).
- **AI affordance:** a single consistent "sparkle" mark denotes AI-generated content and feedback.
- **Illustration:** none by default — type and whitespace carry the page. The asterisk ✳︎ is the one decorative mark.

## 10. Data visualization

- Charts (recharts) are monochrome: black series on white with hairline gridlines; a single status color only where it encodes meaning.
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

`app/globals.css` declares these as CSS variables under `:root` (light, default) and `.dark` (inverted); `tailwind` exposes them via `@theme inline`; shadcn maps its semantic names onto them. `--font-heading` / `--font-body` are the Helvetica Now faces (loaded via root-layout stylesheet links); `--font-mono` is Geist Mono. Changing a value here changes it everywhere.

---

*Monochrome editorial template adapted from the "Mainframe" hero brief, tuned for Interview Copilot AI. Supersedes the warm Claude/Anthropic system. See ADR `0004-monochrome-mainframe-template.md` (supersedes `0003`).*
