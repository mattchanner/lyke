# Kibbe Style Quiz - Implementation Tasks

**Document version:** 1.0
**Created:** 2026-03-11
**Status:** In Progress

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
| T3.1 | Create `style-quiz` feature folder and module/routing files | [ ] | Lazy-loaded module |
| T3.2 | Create `kibbe.models.ts` with TypeScript interfaces | [ ] | All request/response types |
| T3.3 | Create `kibbe-quiz.data.ts` with 14 questions | [ ] | 3 sections, 5 options each |
| T3.4 | Create `KibbeSessionService` | [ ] | localStorage + 30min TTL |
| T3.5 | Create `KibbeAnalyticsService` | [ ] | Typed event emitter |
| T3.6 | Create `KibbeQuizService` (API calls) | [ ] | HTTP client wrapper |
| T3.7 | Add lazy route to `app.routes.ts` | [ ] | /style-quiz → module |
| T3.8 | Add auth guard to full-results route | [ ] | Redirect if unauthed |

---

## Phase 4: Quiz UI Pages (Days 5-9)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T4.1 | Landing page | [ ] | Hero, CTA, family preview chips |
| T4.2 | Primer screen | [ ] | Yin/yang explainer, begin CTA |
| T4.3 | Section intro screen | [ ] | Animated card per section |
| T4.4 | Question screen | [ ] | 5-option cards, auto-advance |
| T4.5 | Calculating screen | [ ] | Animated gradient, 2.5s delay |
| T4.6 | Teaser results page | [ ] | Family hero, blurred section, reg CTA |
| T4.7 | Full results page | [ ] | Animated reveal, style guide |
| T4.8 | Style guide tab content | [ ] | Per-family recommendations |
| T4.9 | Retake quiz flow | [ ] | Confirmation, re-run, overwrite |
| T4.10 | Family override UI | [ ] | Self-select different family |
| T4.11 | Style profile page (`/profile/style`) | [ ] | Saved result view |

---

## Phase 5: Share Card (Days 9-10)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T5.1 | Implement `ShareCardComponent` | [ ] | HTML Canvas generator |
| T5.2 | Integrate Web Share API | [ ] | File export for sharing |
| T5.3 | Implement download fallback | [ ] | For non-supporting browsers |
| T5.4 | Add share section to results | [ ] | Button + preview |
| T5.5 | Add share analytics events | [ ] | share_click, share_complete |

---

## Phase 6: Auth Integration (Day 10)

| Task | Description | Status | Notes |
|------|-------------|--------|-------|
| T6.1 | Pass `?return=kibbe-results` to auth | [ ] | From teaser page |
| T6.2 | Handle return redirect in AuthService | [ ] | Navigate to full-results |
| T6.3 | Session expiry message | [ ] | Friendly retake prompt |
| T6.4 | Track auth events from quiz | [ ] | register/login_complete_from_quiz |

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

---

## References

- `specs/kibbe/kibbe_quiz_implementation_plan.pdf` - Full implementation plan
- `specs/kibbe/An Introduction To The Kibbe Body Types _ the concept wardrobe.pdf` - Kibbe overview
- `specs/kibbe/kibbe_resources.xlsx` - Style recommendations per family
