---
name: LYKE
description: Confidence through relevance — the well-lit fitting room.
colors:
  midnight-pine: "#0E3A3B"
  quiet-slate: "#5F7394"
  aged-mustard: "#D1A72E"
  soft-stone: "#F4F2EE"
  warm-ash: "#E3E0DA"
  charcoal-ink: "#2A2D32"
  muted-pewter: "#6B6F76"
  parchment: "#FFFFFF"
  signal-success: "#5B8C5A"
  signal-warning: "#E8930C"
  signal-danger: "#C94C4C"
typography:
  display:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "2rem"
    fontWeight: 600
    lineHeight: 1.15
    letterSpacing: "-0.01em"
  headline:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 500
    lineHeight: 1.25
    letterSpacing: "-0.01em"
  title:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 500
    lineHeight: 1.3
    letterSpacing: "-0.01em"
  title-prominent:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.15rem"
    fontWeight: 700
    lineHeight: 1.3
    letterSpacing: "-0.01em"
  body:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
    letterSpacing: "normal"
  body-small:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.9rem"
    fontWeight: 500
    lineHeight: 1.45
    letterSpacing: "-0.01em"
  caption:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.8rem"
    fontWeight: 400
    lineHeight: 1.4
    letterSpacing: "normal"
  label:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 600
    lineHeight: 1.2
    letterSpacing: "0.02em"
  stat:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.4rem"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "-0.01em"
rounded:
  sm: "8px"
  md: "10px"
  lg: "12px"
  xl: "14px"
  "2xl": "18px"
  pill: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "14px"
  lg: "16px"
  xl: "24px"
  "2xl": "32px"
components:
  button-primary:
    backgroundColor: "{colors.midnight-pine}"
    textColor: "{colors.parchment}"
    typography: "{typography.body}"
    rounded: "{rounded.xl}"
    padding: "12px 24px"
  button-primary-hover:
    backgroundColor: "#0C3334"
    textColor: "{colors.parchment}"
    typography: "{typography.body}"
    rounded: "{rounded.xl}"
    padding: "12px 24px"
  button-secondary:
    backgroundColor: "transparent"
    textColor: "{colors.midnight-pine}"
    typography: "{typography.body}"
    rounded: "{rounded.xl}"
    padding: "12px 24px"
  button-accent:
    backgroundColor: "{colors.aged-mustard}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.body}"
    rounded: "{rounded.pill}"
    padding: "12px 24px"
  card:
    backgroundColor: "{colors.parchment}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.body}"
    rounded: "{rounded.2xl}"
    padding: "14px"
  chip:
    backgroundColor: "{colors.warm-ash}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.label}"
    rounded: "{rounded.pill}"
    padding: "4px 10px"
  chip-primary:
    backgroundColor: "{colors.midnight-pine}"
    textColor: "{colors.parchment}"
    typography: "{typography.label}"
    rounded: "{rounded.pill}"
    padding: "4px 10px"
  input-field:
    backgroundColor: "{colors.warm-ash}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.body}"
    rounded: "{rounded.lg}"
    padding: "12px 14px"
  selectable-card:
    backgroundColor: "{colors.parchment}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.body-small}"
    rounded: "{rounded.xl}"
    padding: "20px"
  selectable-card-selected:
    backgroundColor: "rgba(14, 58, 59, 0.08)"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.body-small}"
    rounded: "{rounded.xl}"
    padding: "20px"
  toolbar:
    backgroundColor: "{colors.parchment}"
    textColor: "{colors.charcoal-ink}"
    typography: "{typography.title-prominent}"
    rounded: "0"
    padding: "0 16px"
  badge:
    backgroundColor: "{colors.charcoal-ink}"
    textColor: "{colors.parchment}"
    typography: "{typography.label}"
    rounded: "{rounded.sm}"
    padding: "4px 10px"
---

# Design System: LYKE

## 1. Overview

**Creative North Star: "The Well-Lit Fitting Room"**

LYKE is a fitting room with honest light. The shopper walks in with a real question — *will this fit a body like mine?* — and the room is calm enough to think in. Surfaces are soft and matte. Type is clear and unhurried. Imagery is real people under daylight, not magazine retouching. Nothing on the wall is shouting.

The system rejects, by name, the visual languages of Instagram, TikTok, fast fashion, influencer gloss, and tech-startup neon. It also rejects the white-and-teal SaaS reflex that's the default failure mode for "calm and minimal" — calm is not the same as bland, and restraint is not the same as emptiness. Every surface earns its quiet through deliberate weight, not through removal.

The system is split across two tonal registers that share the same vocabulary. **Content surfaces** (feed, post detail, creator profiles, my-posts) follow Everlane: image-forward, editorial restraint, typography that lets photography speak. **Data surfaces** (retailer insights, body-profile editing, the style quiz, charts) follow Apple Health: ranges and bands as native units, generous breathing room around numbers, type-led hierarchy with no decorative chrome. The same color palette and the same component primitives serve both.

**Key Characteristics:**
- Tonal layering (Soft Stone → Warm Ash → Parchment) does most of the depth work.
- Components are tactile and confident — soft corners, decisive states, never half-greyed.
- Calm pacing in motion (state transitions only, never theatrical).
- Bands and ranges, not exact measurements, in every body-profile surface.
- Real, naturally-lit photography. Filters and gradients are forbidden on people.

## 2. Colors: The Daylight Stone Palette

A muted, warm-leaning palette anchored on a dark teal that reads as *trust* without becoming corporate-blue. Hex values are the source of truth in code; OKLCH is cited alongside as the canonical reference for any future tint, ramp, or palette extension.

### Primary
- **Midnight Pine** (`#0E3A3B`, ≈`oklch(31% 0.040 195)`): Primary buttons, navigation toolbars, active states, key headings (H1), focus rings. The brand's center of gravity. Carries trust and credibility without the corporate-finance heaviness of navy.

### Secondary
- **Quiet Slate** (`#5F7394`, ≈`oklch(50% 0.050 255)`): Secondary buttons, supporting iconography, neutral chart series. Used when Midnight Pine would be too declarative — a quieter voice on the same chord.

### Tertiary (Accent)
- **Aged Mustard** (`#D1A72E`, ≈`oklch(75% 0.135 90)`): Reserved for primary CTA emphasis, notifications, and key highlights only. Never body copy. Never decorative. It earns its weight through scarcity.

### Neutral
- **Soft Stone** (`#F4F2EE`, ≈`oklch(96% 0.005 80)`): Page backgrounds. Warm-tinted off-white that reads as paper rather than screen-white.
- **Warm Ash** (`#E3E0DA`, ≈`oklch(89% 0.008 80)`): Card and surface backgrounds inside lists, input field fills, divider tints. The middle layer of the tonal stack.
- **Parchment** (`#FFFFFF`, ≈`oklch(100% 0 80)`): Toolbar, modal, and floating-card surfaces. Pure white is allowed only in places that need to read as *lifted above the page*.
- **Charcoal Ink** (`#2A2D32`, ≈`oklch(28% 0.008 260)`): Primary body text. Slate-leaning to keep it from feeling like printed black.
- **Muted Pewter** (`#6B6F76`, ≈`oklch(53% 0.005 260)`): Secondary text, captions, body-info under post cards, chart labels.

### Signal (use sparingly)
- **Signal Success** (`#5B8C5A`, ≈`oklch(58% 0.085 145)`): Successful state confirmations, similarity-match badges. Warm-leaning green to avoid the medical-app reflex.
- **Signal Warning** (`#E8930C`, ≈`oklch(73% 0.160 65)`): Verification pending, low-confidence matches. Distinct enough from Mustard to avoid signal collision.
- **Signal Danger** (`#C94C4C`, ≈`oklch(60% 0.160 25)`): Destructive actions, errors. Muted red, not fire-engine.

### Named Rules

**The Mustard-Is-Action Rule.** Aged Mustard appears only on elements that exist to be acted on, or that confirm an action just happened. Decorative use is prohibited. If Mustard appears on more than one element per screen, the second one is wrong.

**The Tonal-Stack Rule.** Page → Card → Surface depth is conveyed by stepping through Soft Stone → Warm Ash → Parchment, in that order. A Parchment card directly on Soft Stone (skipping Warm Ash) is acceptable when the card is genuinely floating; a Warm Ash card directly on Soft Stone is acceptable inside dense lists. Other combinations are not.

**The No-Body-Filter Rule.** Color treatments — gradients, vignettes, tints, color-grading — are never applied to imagery containing people. The platform's value collapses if a creator's photo starts looking like an Instagram filter preset. This rule is non-negotiable and supersedes any other styling guidance.

## 3. Typography

**Display Font:** Inter (with `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto` as system fallbacks)
**Body Font:** Inter
**Label/Mono Font:** Inter (the system is monolithic by font family; weight and size carry all hierarchy)

**Character:** A single confident sans-serif, used across the entire system. Inter's slightly-wider counters keep small sizes legible on phone screens; its tight tracking at display sizes keeps headings unhurried without feeling soft. The system explicitly rejects "fashion editorial" typography (no script faces, no thin display serifs, no all-caps decorative use).

### Hierarchy

- **Display** (600, `2rem` / 32px, `1.15` line-height, `-0.01em` tracking): Page hero headings (H1). Onboarding step titles, quiz landing, profile names. Always Midnight Pine on light surfaces.
- **Headline** (500, `1.5rem` / 24px, `1.25`, `-0.01em`): Section heads (H2). Charcoal Ink, never Midnight Pine — keeps Pine reserved for the page-defining heading.
- **Title** (500, `1.25rem` / 20px, `1.3`, `-0.01em`): Card heads, sub-section heads (H3).
- **Title-Prominent** (700, `1.15rem` / 18.4px, `1.3`, `-0.01em`): Toolbar titles only. The bolder weight at a smaller size compensates for the limited toolbar height.
- **Body** (400, `1rem` / 16px, `1.5`): Default running text. Cap line length at 65–75ch on tablet/desktop layouts.
- **Body-Small** (500, `0.9rem` / 14.4px, `1.45`, `-0.01em`): Post titles in feed cards, secondary content blocks, dense inline copy. The slight tracking keeps it from going limp at the smaller size.
- **Caption** (400, `0.8rem` / 12.8px, `1.4`): Supporting metadata under post cards (height/size band, fit note), chart axis labels. Always Muted Pewter.
- **Label** (600, `0.75rem` / 12px, `1.2`, `0.02em`): Badges, tag chips, status pills. The wider tracking earns the smaller size.
- **Stat** (700, `1.4rem` / 22.4px, `1.2`, `-0.01em`): Insight metrics, retailer dashboard numbers, earnings figures. Always Midnight Pine. Tabular figures (`font-variant-numeric: tabular-nums`) when stacked.

### Named Rules

**The One-Pine-Heading Rule.** Midnight Pine on type is reserved for one heading per screen — the H1 (Display) — and for stat values. Section heads (H2) and below are Charcoal Ink. This keeps the eye's path through the page unambiguous.

**The Tracking-Compensates-Size Rule.** Tighten tracking as size grows (`-0.01em` from `1.15rem` upward); widen it as size shrinks (`0.02em` at the Label step). Body and Caption stay at `normal`. A flat `letter-spacing: 0` system feels mechanical; a single varied scale feels considered.

## 4. Elevation

The system is **flat at rest, soft elevation on state**. Surfaces do not carry shadow as a default. Depth is conveyed by the tonal stack (Soft Stone → Warm Ash → Parchment) and by hairline borders at very low opacity. Shadow appears only as a *response* — to hover, to active drag, to a modal entering — and never as decoration.

This decision matters: it means a shadow has communicative weight. When the user sees one, something has *happened*. Defaulting cards to a faint ambient shadow would burn that signal for nothing.

### Shadow Vocabulary

- **`ambient-low`** (`box-shadow: 0 4px 12px rgba(14, 58, 59, 0.08)`): Hover lift on interactive cards, follow buttons, selectable cards in onboarding. Tinted toward Midnight Pine rather than pure black, so the shadow inherits the brand hue.
- **`ambient-mid`** (`box-shadow: 0 8px 24px rgba(14, 58, 59, 0.12)`): Modals, dropdowns, action sheets, quiz transitions during entry.
- **`ambient-high`** (`box-shadow: 0 16px 40px rgba(14, 58, 59, 0.16)`): Reserved. Use only for full-screen sheets, image-cropper overlays, or genuinely-lifted creator-tool surfaces.
- **`hairline`** (`border: 0.5px solid rgba(42, 45, 50, 0.08)`): The everyday separator. Used on toolbar bottom edges, list dividers, card outlines when a card needs definition without weight. Not a shadow, but it lives in the same vocabulary.

### Named Rules

**The Flat-By-Default Rule.** Surfaces are flat at rest. If you find yourself adding `box-shadow` to a static element, you're solving a hierarchy problem with the wrong tool. Use the tonal stack and a hairline first.

**The Pine-Tinted-Shadow Rule.** All shadows carry a hint of Midnight Pine (`rgba(14, 58, 59, ...)`) rather than pure black. Pure-black shadows on warm neutrals look blue-ish and "off"; brand-tinted shadows feel like part of the system.

## 5. Components

Component philosophy: **tactile and confident.** Soft corners (12-18px), decisive states (no half-greyed disabled treatments — full opacity at 50% for disabled, never 80%), and a clear pressable response on every interactive element. Calm in pacing, confident in affordance — confidence in the *thing*, not in the *theatre*.

### Buttons

- **Shape:** Rounded corners (`14px` for standard buttons; `pill` / `24px` for accent CTAs and quiz primary actions; `20px` for compact follow-buttons inside post cards). Never sharp, never fully sharp-pill in the same context.
- **Primary:** Midnight Pine background, Parchment text, `12px 24px` padding, weight `600`, letter-spacing `0.01em`, no text-transform. On hover: shifts to `#0C3334` (Pine shade) and lifts with `ambient-low`.
- **Secondary:** Transparent background, Midnight Pine text, 1px Midnight Pine border, otherwise identical padding and type. On hover: 8% Pine fill (`rgba(14, 58, 59, 0.08)`).
- **Accent (CTA):** Aged Mustard background, Charcoal Ink text, pill shape (`24px` radius), same padding. Reserved per the Mustard-Is-Action Rule. On hover: shifts to `#B89328` (Mustard shade) and lifts with `ambient-low`.
- **Ghost / Text:** No background, no border, Midnight Pine text. Used inside dense surfaces (feed promo bars, table action menus) where a bordered button would compete with content.
- **Disabled:** Same colors as the default, opacity `0.5`. Never grey. Never reduced contrast on the type alone.

### Chips

- **Default:** Warm Ash background, Charcoal Ink text, pill shape (`20px` radius), `4px 10px` padding, Label typography. Used for product tags inside post cards, sort filters, body-band displays.
- **Primary (active filter):** Midnight Pine background, Parchment text. Used to indicate a filter is currently applied. The state is unambiguous; no half-state.
- **Promo / accent context:** Background tinted with Mustard at 10% (`rgba(209, 167, 46, 0.1)`), with a 20%-opacity Mustard bottom border. Used only on the creator-promo strip. Not a general pattern.

### Cards

- **Corner Style:** `18px` radius (`2xl`). Slightly heavier than the button radius — the deliberate size difference reads as "card" without needing a heavier border or shadow.
- **Background:** Parchment when floating on Soft Stone (feed home, profile views). Warm Ash when nested inside another container (rare; usually a code smell — avoid nested cards).
- **Shadow Strategy:** Flat at rest. `ambient-low` only on hover for interactive cards (post cards, selectable cards in onboarding). Static informational cards stay flat permanently.
- **Border:** None by default. Hairline (`0.5px solid rgba(42,45,50,0.08)`) only when a card sits on a same-tone background and needs definition.
- **Internal Padding:** `14px` for content cards (post cards, retailer stat cards). `20px` for selectable cards in onboarding and quiz flows where the card is the primary affordance.
- **Avatar overlap pattern:** Post cards use a signature pattern where the creator avatar is positioned to overlap the bottom of the media image (`bottom: -26px`, `4px` Parchment border). The card's `overflow: visible` allows this; the inner media `.media-clip` handles its own image rounding. Preserve this pattern when adding new content-card variants.

### Inputs / Fields

- **Style:** Warm Ash background fill, no border, `12px` radius, `12px 14px` padding. The fill carries the field; no outline needed at rest.
- **Focus:** 2px Midnight Pine ring at 100% opacity (not a glow — a definite ring), positioned outside the field with `outline-offset: 2px`. Pressable, decisive — the ring confirms focus the moment it arrives.
- **Error:** Signal Danger ring replacing the focus ring. Error message in Signal Danger, Caption typography, below the field.
- **Disabled:** Field opacity `0.5`. Never reduce text contrast alone.

### Navigation (Toolbar / Tab Bar)

- **Toolbar:** Parchment background, Charcoal Ink title in Title-Prominent type, `0.5px` bottom hairline at 8% Charcoal. No shadow. Stays flat through scroll — depth comes from the hairline and the tonal step from page (Soft Stone) to toolbar (Parchment).
- **Tab Bar (mobile):** Parchment background, top hairline (same hairline token), icons in Muted Pewter at rest, Midnight Pine when active with a small Mustard dot indicator if there's an unread notification badge. No background fill on the active tab.
- **Segment buttons (in-page):** `10px` radius, Midnight Pine indicator + `--color-checked` on iOS uses Parchment text inside the filled indicator pill. On Android, Charcoal Ink text remains, only the underline indicator is Pine.

### Selectable Cards (Onboarding / Quiz)

A signature LYKE pattern. Used in onboarding for body-type, frame-size, and fit-preference selection, and in the style quiz for question answers.

- **Style:** Parchment background, 2px Warm Ash border, `14px` radius, `20px` padding. Body-Small typography for the option name, Caption for the description.
- **Selected:** 2px Midnight Pine border, 8% Pine background tint (`rgba(14, 58, 59, 0.08)`). A small Pine checkmark in the top-right corner. The state is fully committed — never a half-tint.
- **Hover (desktop / tablet):** `ambient-low` lift. On touch devices, the active state replaces hover entirely.
- **Transition:** `all 0.2s ease` on the border and background. Don't animate `padding` or `border-radius` (CSS layout properties).

### Body-Band Display

The single most brand-defining component. Renders a user's or creator's body profile as a *band*, never as exact stats. Used under every post card, on creator profiles, and in retailer insight rows.

- **Style:** Caption typography in Muted Pewter. Format: *"5'4"–5'6" · medium frame · fitted preference"* — three facets joined by middle-dots.
- **Variant for retailer insights:** Same format, but counts ("3,412 in band") rendered in Stat type alongside, never with a percentage that could feel surveillance-y.
- **Forbidden:** Never display exact height ("5'5\""), exact weight, or any single number that could identify an individual user. The band is the visual unit, even when the underlying data is finer.

## 6. Do's and Don'ts

### Do:

- **Do** use the tonal stack (Soft Stone → Warm Ash → Parchment) for depth before reaching for a shadow.
- **Do** keep Aged Mustard reserved for action and confirmation. Decorative Mustard is wrong.
- **Do** show body-profile data as bands, never exact stats. *"5'4"–5'6", fitted preference"* — never *"5'5", 132lb"*.
- **Do** use Midnight Pine on H1 and Stat values only. Section heads (H2) and below stay Charcoal Ink.
- **Do** tint shadows toward Midnight Pine (`rgba(14, 58, 59, ...)`) rather than pure black.
- **Do** lift selectable cards and post cards on hover with `ambient-low`. Static informational cards stay flat.
- **Do** honor `prefers-reduced-motion`. The `fadeSlideUp` and `fadeIn` keyframes must degrade to instant or fade-only when the system requests it.
- **Do** pair color with shape, position, label, or pattern in any chart or status indicator. Color-blind safety is non-negotiable on retailer data viz.
- **Do** photograph people under natural light, with the diversity of bodies the platform actually serves. Real bodies, no smoothing.
- **Do** use tabular figures (`font-variant-numeric: tabular-nums`) on Stat type when numbers stack vertically.

### Don't:

- **Don't** apply gradients, vignettes, tints, or color-grading to imagery containing people. Ever. *(The No-Body-Filter Rule.)*
- **Don't** use `border-left` or `border-right` greater than 1px as a colored accent stripe on cards, list items, or callouts.
- **Don't** use gradient text (`background-clip: text` + gradient). Use solid color; emphasize via weight or size.
- **Don't** default to glassmorphism or blur backdrops. Rare and purposeful, or nothing.
- **Don't** use the hero-metric template (big number, small label, supporting stats, gradient accent). It's the SaaS cliché LYKE explicitly rejects.
- **Don't** build a feed that looks like Instagram. No double-tap-to-like animations, no story rings, no engagement-bait pacing.
- **Don't** build retailer surfaces that feel like surveillance dashboards. Aggregated bands only. Read more like a research report than a CCTV monitor.
- **Don't** reach for a modal as the first thought. Exhaust inline and progressive alternatives first.
- **Don't** use `#000` or `#fff` directly. Charcoal Ink and Parchment are the project's near-black and near-white.
- **Don't** use urgency timers, "X people viewing this", or any fast-fashion engagement-bait pattern. Reassurance over excitement.
- **Don't** display exact body measurements anywhere user-facing. Bands, always.
- **Don't** animate CSS layout properties (`padding`, `margin`, `width`, `height`, `border-radius`). Animate `transform` and `opacity` only.
- **Don't** use bouncy or elastic easing curves. Stick to ease-out (default `ease`) at 0.2-0.45s durations.
- **Don't** reach for a colored side-stripe to indicate state (info / warning / error). Use a full background tint, a leading icon, or a decisive border ring.
