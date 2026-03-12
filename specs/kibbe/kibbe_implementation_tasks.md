# Kibbe Style Quiz - Implementation Tasks

**Document version:** 1.1
**Created:** 2026-03-11
**Last updated:** 2026-03-12
**Status:** In Progress — Phases 1–6 complete; Phase 7, 8 remaining

---

## Overview

The Kibbe Body Type Quiz is a user acquisition feature that helps users discover their style type through a 14-question quiz across 3 sections (Bone Structure, Body Flesh, Facial Features). The quiz produces one of 5 primary families (Dramatic, Natural, Classic, Gamine, Romantic) with optional mixed-type detection.

**Funnel:** Free quiz → Teaser result → Registration gate → Full style profile

---

## Phase 1: Backend Foundation (Days 1-3)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T1.1 | Add `KibbeFamily` enum to `Lyke.Core/Enums` | [x] | Dramatic=0, Natural=1, Classic=2, Gamine=3, Romantic=4 |
| T1.2 | Add `KibbeConfidence` enum to `Lyke.Core/Enums` | [x] | High, Medium, Low |
| T1.3 | Add `StyleProfile` entity to `Lyke.Core/Entities` | [x] | PrimaryFamily, RunnerUp, SectionDominance, Counts, Override |
| T1.4 | Add `StyleProfile` navigation property to `User` entity | [x] | One-to-one relationship |
| T1.5 | Add `StyleProfileConfiguration` EF Core config | [x] | Table: style_profiles, jsonb for CountsJson |
| T1.6 | Register `StyleProfileConfiguration` in `LykeDbContext` | [x] | Add DbSet and configuration |
| T1.7 | Create EF Core migration: `AddStyleProfile` | [x] | `dotnet ef migrations add AddStyleProfile` |
| T1.8 | Create `KibbeQuizDtos.cs` in `Application/DTOs/Style` | [x] | KibbeAnswer, KibbeScoreRequest, KibbeScoreResponse |
| T1.9 | Create `StyleProfileDtos.cs` in `Application/DTOs/Style` | [x] | StyleProfileResponse, SaveStyleProfileRequest |
| T1.10 | Create `IKibbeQuizService` interface | [x] | Score, SaveAsync, GetProfileAsync, OverrideAsync |
| T1.11 | Implement `KibbeQuizService` scoring algorithm | [x] | Deterministic v1, no DB required for scoring |
| T1.12 | Implement `KibbeQuizService` profile methods | [x] | Save, Get, Override profile |
| T1.13 | Register `KibbeQuizService` in DI container | [x] | Add to DependencyInjection.cs |
| T1.14 | Create Style endpoint handlers | [x] | 5 endpoints in Style/ folder |
| T1.15 | Create `StyleEndpointRoutes.cs` | [x] | Map all style endpoints |
| T1.16 | Register Style endpoints in `Program.cs` | [x] | app.MapStyleEndpoints() |
| T1.17 | Add `KibbeFeedBoost` feature flag | [ ] | Optional, disabled by default |

---

## Phase 2: Backend Tests (Day 3)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T2.1 | Create `KibbeQuizServiceTests.cs` | [x] | 15 test cases covering all scenarios |
| T2.2 | Run `dotnet test` - all tests passing | [x] | 154 unit tests pass |

### Test Cases Required:
1. `AllA_ReturnsDramatic` — 14 answers all "A" → primary=Dramatic, confidence=High, isMixed=false
2. `MixedSections_DetectedAsMixed` — bone=A, flesh=E, face=C → isMixed=true
3. `TwoWayTie_HandledGracefully` — 7 A's and 7 E's → isMixed=true, confidence=Low
4. `NearTie_LowConfidence` — 8 A's, 6 B's → margin=2, confidence=Low, isMixed=true
5. `StrongDominance_HighConfidence` — 10 A's, 4 others → margin≥5, confidence=High
6. `EmptyPayload_ThrowsValidation` — [] answers → ValidationException
7. `InvalidOption_ThrowsValidation` — SelectedOption="F" → ValidationException
8. `SingleAnswer_Scores` — 1 answer → valid result (edge case)
9. `RunnerUp_IsSecondHighestCount` — 8A, 4B, 2C → runnerUp=Natural
10. `SectionDominance_CorrectPerSection` — all bone=A, all flesh=E, all face=C → sectionDominance matches

---

## Phase 3: Frontend Foundation (Days 4-5)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T3.1 | Create `style-quiz` feature folder and module/routing files | [x] | Lazy-loaded routes (standalone components) |
| T3.2 | Create `kibbe.models.ts` with TypeScript interfaces | [x] | All request/response types in `models/style/` |
| T3.3 | Create `kibbe-quiz.data.ts` with 14 questions | [x] | 3 sections (5+5+4), 5 options each |
| T3.4 | Create `KibbeSessionService` | [x] | localStorage + 30min TTL |
| T3.5 | Create `KibbeAnalyticsService` | [x] | Typed event emitter wrapping AnalyticsService |
| T3.6 | Create `KibbeQuizApiService` (API calls) | [x] | HTTP client wrapper for all 5 endpoints |
| T3.7 | Add lazy route to `app.routes.ts` | [x] | /style-quiz → STYLE_QUIZ_ROUTES |
| T3.8 | Add auth guard to full-results route | [x] | authGuard on /style-quiz/results |

---

## Phase 4: Quiz UI Pages (Days 5-9)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T4.1 | Landing page | [x] | Hero, CTA, family preview chips |
| T4.2 | Primer screen | [x] | Yin/yang explainer, begin CTA |
| T4.3 | Section intro screen | [x] | Section card with question count |
| T4.4 | Question screen | [x] | 5-option cards, auto-advance (400ms delay) |
| T4.5 | Calculating screen | [x] | Spinner overlay, 2.5s min delay |
| T4.6 | Teaser results page | [x] | Family hero, blurred preview, reg CTA |
| T4.7 | Full results page | [x] | Overview + Style Guide tabs |
| T4.8 | Style guide tab content | [x] | Per-family silhouettes/colours/fabrics/avoid/icons |
| T4.9 | Retake quiz flow | [x] | AlertController confirmation on landing if existing data |
| T4.10 | Family override UI | [x] | AlertController radio picker on results + style profile pages |
| T4.11 | Style profile page (`/profile/style`) | [x] | Full saved result view with override + clear |

---

## Phase 5: Share Card (Days 9-10)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T5.1 | Implement `ShareCardService` | [x] | HTML Canvas generator — 1080×1080 PNG with family theming |
| T5.2 | Integrate Web Share API | [x] | File export with navigator.share |
| T5.3 | Implement download fallback | [x] | createObjectURL + anchor click for non-supporting browsers |
| T5.4 | Add share section to results | [x] | Share button + preview image card above action buttons |
| T5.5 | Add share analytics events | [x] | share_click, share_complete via KibbeAnalyticsService |

---

## Phase 6: Auth Integration (Day 10)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T6.1 | Pass `?return=style-quiz/results` to auth | [x] | Teaser page passes param on register/login click |
| T6.2 | Handle return redirect in auth pages | [x] | Login + register read `?return=` and navigate there after success |
| T6.3 | Session expiry message | [x] | Results page shows toast + redirects to /style-quiz if no session |
| T6.4 | Track auth events from quiz | [x] | `loginCompleteFromQuiz` / `registerCompleteFromQuiz` fired when `?return=` present |

---

## Phase 7: Assets and Polish (Days 10-12)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T7.1 | Source 5 family portrait images | [ ] | Unsplash or commission |
| T7.2 | Create 5 family hero images | [ ] | Landscape format |
| T7.3 | Create 3 section icons (SVG) | [ ] | bone, flesh, face |
| T7.4 | Apply family colour system | [ ] | CSS custom properties |
| T7.5 | Add entrance animations | [ ] | Ionic/CSS @keyframes |
| T7.6 | Test on iOS Safari, Android Chrome | [ ] | Responsive + PWA |
| T7.7 | Accessibility audit | [ ] | ARIA labels, focus management |

---

## Phase 8: Feed Personalisation (Days 12-13, Optional)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T8.1 | Add `KibbeFamily` column to Creator | [ ] | Nullable |
| T8.2 | Implement feed score boost | [ ] | Behind feature flag |
| T8.3 | Read StyleProfile in feed pipeline | [ ] | When flag enabled |
| T8.4 | Add kibbe_family to creator profile | [ ] | Self-reported, optional |

---

## API Endpoints Summary

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/style/v1/quiz/kibbe/score` | None | Score answers, return result |
| GET | `/api/style/v1/profile` | Required | Get saved style profile |
| PUT | `/api/style/v1/profile` | Required | Save quiz result to profile |
| PUT | `/api/style/v1/profile/override` | Required | Self-select a different family |
| DELETE | `/api/style/v1/profile/override` | Required | Clear override, revert to computed |

---

## Progress Log

| Date | Tasks Completed | Notes |
|------|-----------------|-------|
| 2026-03-11 | T1.1-T1.16, T2.1-T2.2 | Backend foundation + tests complete - 154 unit tests pass |
| 2026-03-12 | T3.1-T3.8 | Frontend foundation complete — models, data, services, stub pages, routing |
| 2026-03-12 | T4.1-T4.11 | Phase 4 complete — all quiz UI pages, style guide data, auto-advance, retake confirmation, family override, /profile/style page |
| 2026-03-12 | T6.1-T6.4 | Phase 6 complete — return redirect in login/register, expiry toast, quiz analytics events |
| 2026-03-12 | Bug fixes | Calculating page respects light/dark mode; authenticated users bypass teaser and go straight to results; style profile empty state fixed (spinner no longer stuck when no profile exists); ES2018 lib compatibility (replaced flatMap/fromEntries) |
| 2026-03-12 | T5.1-T5.5 | Phase 5 complete — ShareCardService (Canvas 1080×1080 with family themes), Web Share API with file export, download fallback, share preview on results page, analytics events |

---

## References

- `specs/kibbe/kibbe_quiz_implementation_plan.pdf` - Full implementation plan
- `specs/kibbe/An Introduction To The Kibbe Body Types _ the concept wardrobe.pdf` - Kibbe overview
- `specs/kibbe/kibbe_resources.xlsx` - Style recommendations per family
