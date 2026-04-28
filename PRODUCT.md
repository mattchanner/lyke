# Product

## Register

product

## Users

LYKE serves four distinct user groups, each with different surfaces and motivations:

- **Shoppers (primary).** Adults browsing fashion to buy. Their context is everyday: commute, evening, sofa, fitting room. Their job is *decision confidence* — "will this actually fit and look right on a body like mine?" — not entertainment. They open the app when they have a real purchase in mind, or to gather signal before one. Multi-size ordering and returns are the friction LYKE removes.
- **Creators.** Community contributors who post outfits with body-profile context, garment sizes worn, and fit notes. Some are casual; some monetize via affiliate links and sponsored placements. Their surface is upload-heavy: post creation, tagging, performance metrics, earnings.
- **Retailers (B2B).** Brands consuming aggregated, anonymized insight: engagement by body-profile band, fit feedback trends, return-risk indicators. They never see individual users. Their surface is dashboard-style: charts, exports, campaign management.
- **Admins.** Internal moderation and operations. Content review queues, user/account management, tag correction, verification review.

The shopper experience is the heart of the product; creator, retailer, and admin surfaces support and feed it.

## Product Purpose

LYKE helps shoppers see fashion content worn by people with genuinely similar body profiles — height, weight, body type, fit preference — so they can buy with confidence and stop ordering three sizes "just in case." It exists because the dominant fashion-content surfaces (Instagram, TikTok, retailer PDPs) optimize for aspiration or scale, not relevance to the person actually shopping.

Success looks like:
- Shoppers report higher purchase confidence and lower return rates after using LYKE.
- Creators earn from authenticity (real bodies, real fit notes) rather than reach.
- Retailers gain insight into fit-driven demand they could not derive from PDP analytics alone.
- The platform never trades on individual user data; insight is always aggregated and banded.

## Brand Personality

**Grounded. Inclusive. Intelligent. Calm.**

Voice is clear and reassuring. No fashion snobbery. No "perfect body" language. No exaggerated claims. The app speaks like a thoughtful friend who happens to know what fits.

- Say "See how this fits on people like you", not "Find your dream body look".
- Say "Fit notes from someone your height", not "Flawless style".
- Say "We don't have enough matches yet — try a wider band", not "No results".

Emotional goals: confidence, trust, calm. The app should feel *more considered than TikTok, less sterile than a retailer PDP, more human than a virtual try-on tool*.

## Anti-references

What LYKE explicitly should NOT look or feel like:

- **Instagram / TikTok.** Engagement-bait pacing, infinite-scroll dopamine, aspirational influencer aesthetic, perfect-body filters.
- **Fast-fashion retail (Shein, Fashion Nova).** Visual chaos, neon CTAs, urgency timers, "perfect look" hype.
- **Influencer-gloss tooling.** Heavy retouching, studio lighting, bodies smoothed into uniformity.
- **Tech-startup neon.** Saturated gradients, glassmorphism, gradient text, dark-mode-because-cool, hero-metric template.
- **Generic minimalism.** White-and-teal SaaS reflex, identical card grids, decorative whitespace without rhythm.

If the interface ever drifts toward any of these, it has gone wrong. Calm is not the same as bland; restraint is not the same as emptiness.

## Positive references

Two anchors, one per surface family, to keep "calm and grounded" from collapsing into generic minimalism:

- **Apple Health — for data surfaces** (retailer insights, body-profile editing, the style quiz results, charts and trend views). Restrained data viz. Ranges and bands as native visual units. Privacy-forward language. Generous breathing room around numbers. Type-led hierarchy, not chrome-led.
- **Everlane — for content surfaces** (feed, creator profiles, post detail, my-posts). Editorial restraint. Real people, naturally lit. Transparency as an aesthetic. Image-forward but quiet. Typography that lets the imagery speak.

These are calibration points, not templates. The goal is to rhyme with their *feel*, not copy their components.

## Design Principles

These guide judgment calls when the brief is ambiguous:

1. **Relevance over reach.** Match by body profile and fit, not popularity or trend-hype. Surfaces that surface "trending" or "most-liked" without a fit-relevance reason are wrong by default.
2. **Ranges, not measurements.** A user's body data is private; a band ("5'4–5'6, fitted preference") is the public unit. This must show up visually — never display exact stats in shopper-facing or creator-facing surfaces, even when the data is technically available.
3. **Real over retouched.** Imagery is authentic, diverse, natural-lit. Visual treatments (filters, gradients, vignettes, smoothing) must not glamorize or smooth bodies. The platform's value collapses if it starts looking like an influencer feed.
4. **Reassurance over excitement.** Calm pacing, low-stim layouts. The job is decision confidence, not dopamine. Animations are functional, never theatrical. No urgency timers, no "X people viewing this", no engagement-bait notifications.
5. **Insight, not surveillance.** Retailer-facing surfaces only ever show aggregated bands. Never let a retailer feel like they are watching a user. Insight surfaces should feel like reading a research report, not a CCTV monitor.

## Accessibility & Inclusion

Accessibility is non-negotiable. The brand's inclusivity claim is hollow without it.

- **Conformance level.** WCAG 2.1 **AA across the entire app**, with **AAA on critical flows**: onboarding, body-profile editing, the style quiz. AAA means 7:1 text contrast and stricter target sizes on these flows.
- **Reduced motion.** Honor `prefers-reduced-motion` system-wide. Feed scroll effects, quiz transitions, post-card interactions, and any decorative motion must degrade gracefully to instant or fade-only.
- **Color-blind safe data viz.** Retailer insights, charts (Chart.js), and any color-coded body-profile band visualizations must be legible without relying on hue alone — pair color with shape, position, label, or pattern.
- **Inclusivity beyond compliance.** Body taxonomy, fit-preference vocabulary, and example imagery must represent the actual diversity of bodies the platform serves. Anti-bias review is part of design QA, not a separate workstream.
