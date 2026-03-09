# Kibbe Style Quiz — Implementation Plan

**Document version:** 1.0
**Date:** 2026-03-09
**Status:** Draft — Ready for implementation

---

## 1. Overview and Strategic Context

### 1.1 Why This Matters

The Kibbe Body Type system is a cult-followed framework with a passionate online community (r/Kibbe has 250k+ members) but **no single authoritative, beautifully designed interactive quiz**. The closest existing tools are utilitarian blog quiz forms. LYKE has the opportunity to become the definitive Kibbe destination — a free, beautiful, emotionally resonant quiz that drives organic user acquisition through social sharing and search.

The growth mechanic is a classic **quiz funnel**: free quiz → teaser result → registration gate → full style profile + feed personalisation. The quiz functions as both a top-of-funnel acquisition tool and a core product feature (personalised feed, creator matching, retailer insights).

### 1.2 The Five Kibbe Families

The Kibbe system organises physical presence into five archetypal families. Each family blends Yin (curved, soft, rounded) and Yang (angular, straight, elongated) energy in different proportions:

| Letter | Family | Yin/Yang Balance | Physical Essence | Celebrity Archetype |
|--------|---------|-----------------|-----------------|-------------------|
| A | **Dramatic** | Pure Yang | Sharp angles, elongated vertical line, chiselled features | Lauren Bacall, Tilda Swinton |
| B | **Natural** | Blended Yang | Blunt angularity, athletic breadth, relaxed and organic | Ingrid Bergman, Jennifer Aniston |
| C | **Classic** | Balanced | Perfect symmetry, even proportions, moderate in all things | Grace Kelly, Kate Middleton |
| D | **Gamine** | Mixed (Yang+Yin) | Compact with high contrast — angular structure softened by rounded details | Audrey Hepburn, Leslie Caron |
| E | **Romantic** | Pure Yin | Soft curves, lush and delicate, petite with full proportions | Sophia Loren (pre-SD), Monroe |

**Mixed types** (second dominant family modifies the first):
- Soft Dramatic (D+E), Flamboyant Natural (B+A), Soft Natural (B+E), Dramatic Classic (C+A), Soft Classic (C+E), Theatrical Romantic (E+A), Flamboyant Gamine (D+A), Soft Gamine (D+E — Gamine dominant)

The quiz produces a **primary family** and optionally a **runner-up family**. The mix is expressed as `Soft [Primary]`, `Flamboyant [Primary]`, `Theatrical [Primary]`, etc., based on the combination.

### 1.3 What Exists Today

| Component | Current State | What Changes |
|-----------|--------------|-------------|
| Body shape quiz | Simple 6-question, 3-answer body shape quiz embedded in onboarding | New Kibbe quiz is a separate, standalone public-facing feature |
| `/api/quiz/v1/calculate` | Returns BodyTypeId, Stature, Build | New endpoint added alongside; existing quiz untouched |
| `QuizPage` | Modal used in onboarding | Kibbe quiz is a separate feature module |
| `BodyProfile` entity | Stores physical measurements | New `StyleProfile` entity added to User |
| `AnalyticsService` | Batch event tracking via AppInsights + backend | Extended with Kibbe-specific events |

---

## 2. UX Design Philosophy

### 2.1 Core Principles (Research-Backed)

**Emotional resonance over clinical accuracy.** The Kibbe system is about "overall impression and physical presence" — not measurements. The quiz copy must feel like a style conversation, not a medical form. Use second-person, identity-affirming language throughout.

**Images before text.** Style quizzes with image-first questions complete at 2–3× the rate of text-only quizzes (Interact.com research). Each option should be illustrated — either through curated photography, fashion illustrations, or stylised silhouette graphics.

**The curiosity gap is your asset.** The landing page and quiz intro should open an emotional loop ("Find out which of the 5 style families is yours") and the full results remain just out of reach until registration. This loop must stay open throughout the quiz.

**Auto-advance on selection.** Once an option is tapped, auto-advance to the next question with a smooth slide transition. This increases pace and completion rate dramatically (Typeform, Prose, Noom all use this pattern). Back navigation must always remain visible and accessible.

**The results reveal moment is the product.** The transition from quiz completion to results is the peak emotional moment. It must feel earned and celebratory — not a dashboard dump. Plan for a 2–3 second "calculating" animation with personalised-feeling copy before the reveal.

**Share card as growth engine.** Every result should feel worth sharing. "I'm a Dramatic" is a statement of identity. The share card design is as important as any other screen — it's the primary acquisition channel.

### 2.2 Quiz Landing Page Copy Framework

**Headline (test variants):**
- "Discover your true style identity in 2 minutes" ← primary
- "What's your Kibbe style type?"
- "Your style has a language. Find out what it's saying."

**Subheadline:**
> "Stylist-developed. Science-informed. Free. The Kibbe Body Type system has helped millions of women dress in alignment with their natural presence — not against it."

**CTA (first-person phrasing outperforms second-person by ~20%):**
- "Find my style type" ← primary
- "Start my style profile"
- "Reveal my type"

**Social proof elements:**
- "Based on David Kibbe's original 1987 system, used by stylists worldwide"
- Style type icons/family names shown as a visual teaser
- "Takes 2 minutes. No measurements required."

**Primer copy (before Q1):**
> "This quiz is about your natural physical presence — the impression you give, not your size or weight. There are no 'better' types. Each family has its own elegance. Answer based on how you look, not how you feel, and choose the description that fits most (not perfectly)."

**Key distinction:** "Yin = curved, soft, rounded. Yang = angular, elongated, straight. Most people are a blend."

### 2.3 Progress and Navigation UX

- **Section-labelled progress bar**: `Bone Structure · Body Flesh · Facial Features` with filled segments, not percentage. Shows meaningful context for each answer.
- **Section start screens**: Brief animated card introducing each section (e.g., "Bone Structure — The architectural foundation of your type").
- **Disabled Next button** until selection made (visible but greyed) — signals action required without hiding navigation.
- **Back always visible** — top-left chevron, consistent position throughout.
- **Question count**: Subtle sub-label "Question 3 of 5 in this section".

### 2.4 Results Reveal Sequence

1. **Calculating screen** (2–3 seconds): Animated with personalised copy — "Analysing your bone structure… Reading your physical essence… Almost there…"
2. **Teaser screen** (pre-auth): Family name revealed with one-line meaning and blurred "full profile" section below
3. **Registration gate**: "Save your style profile" framing, social auth primary
4. **Full reveal** (post-auth): Slide-in animation, personalised greeting, full guide

### 2.5 Family Visual Identity

Each family needs a distinct visual personality applied consistently across all touchpoints (card, share image, profile badge, feed tag):

| Family | Colour | Typographic Mood | Illustration Style |
|--------|--------|-----------------|-------------------|
| Dramatic | Deep charcoal + gold | Geometric serif, high contrast | Angular silhouette, architectural |
| Natural | Warm taupe + sage green | Relaxed humanist sans | Loose sketch, organic |
| Classic | Ivory + burgundy | Proportioned transitional serif | Balanced, refined line art |
| Gamine | Crisp white + cobalt | Tight geometric sans | Playful contrast, compact |
| Romantic | Blush + champagne | Flowing script + rounded serif | Soft curves, delicate |

---

## 3. Feature Architecture

### 3.1 Route Structure

```
/style-quiz                         ← Public landing page (feature shell)
/style-quiz/kibbe                   ← Kibbe quiz entry point (public)
/style-quiz/kibbe/quiz              ← Quiz engine (public, session-keyed)
/style-quiz/kibbe/results           ← Teaser results (public)
/style-quiz/kibbe/full-results      ← Full results (auth required, redirects if unauthed)
/profile/style                      ← Saved style profile (auth required)
```

### 3.2 Module Structure (Frontend)

```
frontend/src/app/features/
  style-quiz/
    style-quiz.module.ts            ← Lazy-loaded feature module
    style-quiz-routing.module.ts
    landing/
      style-quiz-landing.page.ts   ← Public landing page
      style-quiz-landing.page.html
      style-quiz-landing.page.scss
    quiz/
      kibbe-quiz.page.ts           ← Quiz engine (primer + 3 sections)
      kibbe-quiz.page.html
      kibbe-quiz.page.scss
      kibbe-quiz.data.ts           ← Question definitions (static data)
    results/
      kibbe-teaser.page.ts         ← Pre-auth teaser result
      kibbe-teaser.page.html
      kibbe-teaser.page.scss
      kibbe-full-results.page.ts   ← Post-auth full results
      kibbe-full-results.page.html
      kibbe-full-results.page.scss
    share/
      share-card.component.ts      ← Canvas-based share card generator
      share-card.component.html
      share-card.component.scss
    services/
      kibbe-quiz.service.ts        ← API calls (score, save profile)
      kibbe-session.service.ts     ← Session persistence (localStorage + expiry)
      kibbe-analytics.service.ts   ← Typed analytics event emitter
    models/
      kibbe.models.ts              ← TypeScript interfaces

frontend/src/app/features/profile/
  style/
    style-profile.page.ts          ← Saved profile view (auth)
    style-profile.page.html
    style-profile.page.scss

frontend/src/assets/kibbe/
  families/
    dramatic.svg                   ← Family illustration (SVG, inline-capable)
    natural.svg
    classic.svg
    gamine.svg
    romantic.svg
  share-cards/
    dramatic-card.png              ← Pre-generated share card backgrounds
    natural-card.png
    classic-card.png
    gamine-card.png
    romantic-card.png
```

### 3.3 Backend Module Structure

```
backend/src/Lyke.Api/Endpoints/
  StyleEndpoints.cs               ← New: Kibbe quiz + style profile endpoints

backend/src/Lyke.Application/DTOs/Style/
  KibbeQuizDtos.cs                ← Request/response DTOs
  StyleProfileDtos.cs             ← Style profile persistence DTOs

backend/src/Lyke.Application/Interfaces/
  IKibbeQuizService.cs            ← Service interface

backend/src/Lyke.Application/Services/
  KibbeQuizService.cs             ← Scoring algorithm + profile management

backend/src/Lyke.Core/Entities/
  StyleProfile.cs                 ← New entity: style_profiles table
  KibbeResult.cs                  ← Embedded value object (owned entity)

backend/src/Lyke.Core/Enums/
  KibbeFamily.cs                  ← Enum: Dramatic, Natural, Classic, Gamine, Romantic
  KibbeConfidence.cs              ← Enum: High, Medium, Low

backend/src/Lyke.Infrastructure/Data/Configurations/
  StyleProfileConfiguration.cs   ← EF Core owned entity configuration

tests/Lyke.UnitTests/Services/
  KibbeQuizServiceTests.cs        ← Unit tests for scoring algorithm
```

---

## 4. Backend Implementation

### 4.1 New Enums

```csharp
// backend/src/Lyke.Core/Enums/KibbeFamily.cs
public enum KibbeFamily
{
    Dramatic = 0,   // A
    Natural  = 1,   // B
    Classic  = 2,   // C
    Gamine   = 3,   // D
    Romantic = 4    // E
}

// backend/src/Lyke.Core/Enums/KibbeConfidence.cs
public enum KibbeConfidence { High, Medium, Low }
```

### 4.2 New Entities

```csharp
// backend/src/Lyke.Core/Entities/StyleProfile.cs
public class StyleProfile : BaseEntity
{
    public Guid UserId { get; set; }

    // Computed result from the quiz
    public KibbeFamily? PrimaryFamily { get; set; }
    public KibbeFamily? RunnerUpFamily { get; set; }
    public bool IsMixed { get; set; }
    public KibbeConfidence? Confidence { get; set; }

    // Section dominance (stored as JSON column or separate columns)
    public KibbeFamily? BoneDominance { get; set; }
    public KibbeFamily? FleshDominance { get; set; }
    public KibbeFamily? FaceDominance { get; set; }

    // Raw answer counts (A–E) stored as JSON
    public string? CountsJson { get; set; }        // e.g. {"A":5,"B":3,"C":2,"D":1,"E":1}

    public DateTime? ComputedAt { get; set; }

    // Optional user override
    public KibbeFamily? OverrideFamily { get; set; }
    public bool IsUserOverride { get; set; }
    public DateTime? OverrideSetAt { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
```

**Add to User entity:**
```csharp
public StyleProfile? StyleProfile { get; set; }
```

### 4.3 EF Core Configuration

```csharp
// backend/src/Lyke.Infrastructure/Data/Configurations/StyleProfileConfiguration.cs
public class StyleProfileConfiguration : IEntityTypeConfiguration<StyleProfile>
{
    public void Configure(EntityTypeBuilder<StyleProfile> builder)
    {
        builder.ToTable("style_profiles");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.User)
               .WithOne(x => x.StyleProfile)
               .HasForeignKey<StyleProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.CountsJson).HasColumnType("jsonb");
    }
}
```

**Migration name:** `AddStyleProfile`

```bash
dotnet ef migrations add AddStyleProfile \
  --project backend/src/Lyke.Infrastructure \
  --startup-project backend/src/Lyke.Api
```

### 4.4 DTOs

```csharp
// backend/src/Lyke.Application/DTOs/Style/KibbeQuizDtos.cs

// Input: one answer per question
public record KibbeAnswer(
    string SectionId,     // "bone" | "flesh" | "face"
    string QuestionId,    // "bone_1", "flesh_2", etc.
    string SelectedOption // "A" | "B" | "C" | "D" | "E"
);

public record KibbeScoreRequest(List<KibbeAnswer> Answers);

public record KibbeSectionDominance(
    KibbeFamily Bone,
    KibbeFamily Flesh,
    KibbeFamily Face
);

public record KibbeCounts(int A, int B, int C, int D, int E);

public record KibbeScoreResponse(
    KibbeFamily PrimaryFamily,
    KibbeFamily? RunnerUpFamily,
    KibbeSectionDominance SectionDominance,
    bool IsMixed,
    KibbeConfidence Confidence,
    KibbeCounts Counts
);

// Style Profile DTOs
public record StyleProfileResponse(
    KibbeFamily? PrimaryFamily,
    KibbeFamily? RunnerUpFamily,
    bool IsMixed,
    KibbeConfidence? Confidence,
    KibbeSectionDominance? SectionDominance,
    bool IsUserOverride,
    KibbeFamily? OverrideFamily,
    DateTime? ComputedAt,
    DateTime? LastUpdatedAt
);

public record SaveStyleProfileRequest(KibbeScoreResponse Score);

public record OverrideStyleProfileRequest(KibbeFamily Family);
```

### 4.5 Scoring Service Interface

```csharp
// backend/src/Lyke.Application/Interfaces/IKibbeQuizService.cs
public interface IKibbeQuizService
{
    KibbeScoreResponse Score(KibbeScoreRequest request);
    Task<StyleProfileResponse> SaveAsync(Guid userId, KibbeScoreResponse score, CancellationToken ct);
    Task<StyleProfileResponse?> GetProfileAsync(Guid userId, CancellationToken ct);
    Task<StyleProfileResponse> OverrideAsync(Guid userId, KibbeFamily family, CancellationToken ct);
}
```

### 4.6 Scoring Algorithm (Deterministic v1)

The algorithm is pure computation — no database required for the scoring step. This means:
1. The score endpoint works anonymously (no auth required).
2. The result can be computed client-side as a fallback (port the algorithm to TypeScript).

```csharp
// backend/src/Lyke.Application/Services/KibbeQuizService.cs — scoring method

private static readonly Dictionary<string, KibbeFamily> OptionToFamily = new()
{
    { "A", KibbeFamily.Dramatic },
    { "B", KibbeFamily.Natural  },
    { "C", KibbeFamily.Classic  },
    { "D", KibbeFamily.Gamine   },
    { "E", KibbeFamily.Romantic }
};

// Threshold: if top-2 margin ≤ this, flag as mixed
private const int MixedThreshold = 2;

// Confidence mapping: margin between #1 and #2 counts
// ≥ 5 questions apart → High
// 3–4 questions apart → Medium
// ≤ 2 questions apart → Low
private static KibbeConfidence ToConfidence(int margin) => margin switch
{
    >= 5 => KibbeConfidence.High,
    >= 3 => KibbeConfidence.Medium,
    _    => KibbeConfidence.Low
};

public KibbeScoreResponse Score(KibbeScoreRequest request)
{
    if (request.Answers == null || request.Answers.Count == 0)
        throw new ValidationException("Answers are required");

    // Validate all options are A-E
    var validOptions = new HashSet<string> { "A", "B", "C", "D", "E" };
    foreach (var a in request.Answers)
    {
        if (!validOptions.Contains(a.SelectedOption.ToUpperInvariant()))
            throw new ValidationException($"Invalid option '{a.SelectedOption}' for question '{a.QuestionId}'");
    }

    // Count overall
    var overallCounts = new Dictionary<KibbeFamily, int>
    {
        { KibbeFamily.Dramatic, 0 },
        { KibbeFamily.Natural,  0 },
        { KibbeFamily.Classic,  0 },
        { KibbeFamily.Gamine,   0 },
        { KibbeFamily.Romantic, 0 }
    };

    // Count per section
    var sectionCounts = new Dictionary<string, Dictionary<KibbeFamily, int>>
    {
        { "bone",  new Dictionary<KibbeFamily, int>(overallCounts) },
        { "flesh", new Dictionary<KibbeFamily, int>(overallCounts) },
        { "face",  new Dictionary<KibbeFamily, int>(overallCounts) }
    };

    foreach (var answer in request.Answers)
    {
        var family = OptionToFamily[answer.SelectedOption.ToUpperInvariant()];
        overallCounts[family]++;
        var sectionId = answer.SectionId.ToLowerInvariant();
        if (sectionCounts.TryGetValue(sectionId, out var sectionDict))
            sectionDict[family]++;
    }

    // Overall ranking
    var ranked = overallCounts.OrderByDescending(kv => kv.Value).ToList();
    var primaryFamily = ranked[0].Key;
    var primaryCount  = ranked[0].Value;
    var runnerUpCount = ranked[1].Value;
    var margin = primaryCount - runnerUpCount;

    // Runner-up: may be null if no second vote exists
    KibbeFamily? runnerUpFamily = ranked[1].Value > 0 ? ranked[1].Key : null;

    // Mixed flag
    var isMixed = margin <= MixedThreshold;

    // Section dominance
    static KibbeFamily DominantInSection(Dictionary<KibbeFamily, int> counts) =>
        counts.OrderByDescending(kv => kv.Value).First().Key;

    var boneDominance  = DominantInSection(sectionCounts["bone"]);
    var fleshDominance = DominantInSection(sectionCounts["flesh"]);
    var faceDominance  = DominantInSection(sectionCounts["face"]);

    // Section-level mixed check also contributes to isMixed flag
    if (boneDominance != primaryFamily || fleshDominance != primaryFamily || faceDominance != primaryFamily)
        isMixed = true;

    var confidence = ToConfidence(margin);

    var counts = new KibbeCounts(
        overallCounts[KibbeFamily.Dramatic],
        overallCounts[KibbeFamily.Natural],
        overallCounts[KibbeFamily.Classic],
        overallCounts[KibbeFamily.Gamine],
        overallCounts[KibbeFamily.Romantic]
    );

    return new KibbeScoreResponse(
        primaryFamily,
        runnerUpFamily,
        new KibbeSectionDominance(boneDominance, fleshDominance, faceDominance),
        isMixed,
        confidence,
        counts
    );
}
```

### 4.7 API Endpoints

```csharp
// backend/src/Lyke.Api/Endpoints/StyleEndpoints.cs

public static IEndpointRouteBuilder MapStyleEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/style/v1").WithTags("Style");

    // ── PUBLIC ────────────────────────────────────────────────────────────────
    // Score the quiz (anonymous)
    group.MapPost("/quiz/kibbe/score", ScoreKibbeQuizAsync)
        .WithName("ScoreKibbeQuiz")
        .WithSummary("Score a completed Kibbe quiz")
        .AllowAnonymous()
        .Produces<ApiResponse<KibbeScoreResponse>>(200)
        .Produces<ApiResponse>(400);

    // ── AUTHENTICATED ─────────────────────────────────────────────────────────
    // Get saved style profile
    group.MapGet("/profile", GetStyleProfileAsync)
        .WithName("GetStyleProfile")
        .RequireAuthorization()
        .Produces<ApiResponse<StyleProfileResponse>>(200)
        .Produces<ApiResponse>(404);

    // Save computed result to profile
    group.MapPut("/profile", SaveStyleProfileAsync)
        .WithName("SaveStyleProfile")
        .RequireAuthorization()
        .Produces<ApiResponse<StyleProfileResponse>>(200);

    // Override family (self-selected)
    group.MapPut("/profile/override", OverrideStyleProfileAsync)
        .WithName("OverrideStyleProfile")
        .RequireAuthorization()
        .Produces<ApiResponse<StyleProfileResponse>>(200);

    // Clear override
    group.MapDelete("/profile/override", ClearStyleProfileOverrideAsync)
        .WithName("ClearStyleProfileOverride")
        .RequireAuthorization()
        .Produces<ApiResponse>(200);

    return app;
}
```

**Route summary:**

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/style/v1/quiz/kibbe/score` | None | Score answers, return result |
| `GET`  | `/api/style/v1/profile` | Required | Get saved style profile |
| `PUT`  | `/api/style/v1/profile` | Required | Save quiz result to profile |
| `PUT`  | `/api/style/v1/profile/override` | Required | Self-select a different family |
| `DELETE` | `/api/style/v1/profile/override` | Required | Clear override, revert to computed |

### 4.8 Feed Personalisation Hook (Feature-Flagged)

```csharp
// backend/src/Lyke.Application/Configuration/FeatureFlags.cs
public class FeatureFlags
{
    // ... existing flags ...
    public bool KibbeFeedBoost { get; set; } = false;  // disabled by default
}

// backend/src/Lyke.Application/DTOs/Feed/FeedRequest.cs — add optional field
public record FeedRequest
{
    // ... existing ...
    // Populated from style_profile if KibbeFeedBoost flag is on
    public KibbeFamily? KibbeFamily { get; set; }
}
```

In the feed service, when `KibbeFeedBoost` is enabled and the user has a `StyleProfile.PrimaryFamily`, inject the family as a boost signal. Posts from creators of the same family get a mild score uplift. Implementation details: add a nullable `KibbeFamily` column to the `Creator` entity; matching family = +0.15 boost to feed score. Gate entirely behind the feature flag.

### 4.9 Unit Tests

```
tests/Lyke.UnitTests/Services/KibbeQuizServiceTests.cs

Test cases required:
1. AllA_ReturnsDramatic — 12 answers all "A" → primary=Dramatic, confidence=High, isMixed=false
2. MixedSections_DetectedAsMixed — bone=A, flesh=E, face=C → isMixed=true, primary determined by count
3. TwoWayTie_HandledGracefully — 6 A's and 6 E's → isMixed=true, confidence=Low, both appear in top two
4. NearTie_LowConfidence — 7 A's, 5 B's → margin=2, confidence=Low, isMixed=true
5. StrongDominance_HighConfidence — 10 A's, 2 others → margin≥5 from runner-up, confidence=High
6. EmptyPayload_ThrowsValidation — [] answers → ValidationException
7. InvalidOption_ThrowsValidation — SelectedOption="F" → ValidationException
8. SingleAnswer_Scores — 1 answer → valid result (edge case)
9. RunnerUp_IsSecondHighestCount — 7A, 5B, 2C → runnerUp=Natural
10. SectionDominance_CorrectPerSection — all bone=A, all flesh=E, all face=C → sectionDominance matches
```

---

## 5. Frontend Implementation

### 5.1 TypeScript Models

```typescript
// frontend/src/app/features/style-quiz/models/kibbe.models.ts

export type KibbeFamily = 'Dramatic' | 'Natural' | 'Classic' | 'Gamine' | 'Romantic';
export type KibbeConfidence = 'High' | 'Medium' | 'Low';
export type KibbeSection = 'bone' | 'flesh' | 'face';
export type KibbeOption = 'A' | 'B' | 'C' | 'D' | 'E';

export interface KibbeAnswer {
  sectionId: KibbeSection;
  questionId: string;
  selectedOption: KibbeOption;
}

export interface KibbeScoreRequest {
  answers: KibbeAnswer[];
}

export interface KibbeSectionDominance {
  bone: KibbeFamily;
  flesh: KibbeFamily;
  face: KibbeFamily;
}

export interface KibbeCounts {
  A: number; B: number; C: number; D: number; E: number;
}

export interface KibbeScoreResponse {
  primaryFamily: KibbeFamily;
  runnerUpFamily?: KibbeFamily;
  sectionDominance: KibbeSectionDominance;
  isMixed: boolean;
  confidence: KibbeConfidence;
  counts: KibbeCounts;
}

export interface StyleProfileResponse {
  primaryFamily?: KibbeFamily;
  runnerUpFamily?: KibbeFamily;
  isMixed: boolean;
  confidence?: KibbeConfidence;
  sectionDominance?: KibbeSectionDominance;
  isUserOverride: boolean;
  overrideFamily?: KibbeFamily;
  computedAt?: string;
  lastUpdatedAt?: string;
}

// Quiz data model
export interface KibbeQuestion {
  id: string;
  prompt: string;
  subPrompt?: string;
  options: KibbeQuestionOption[];
}

export interface KibbeQuestionOption {
  letter: KibbeOption;
  label: string;
  description?: string;
  imageSrc?: string;   // optional illustration path
}

export interface KibbeSection {
  id: KibbeSection;
  title: string;
  subtitle: string;
  iconSrc: string;
  questions: KibbeQuestion[];
}
```

### 5.2 Quiz Question Data

```typescript
// frontend/src/app/features/style-quiz/quiz/kibbe-quiz.data.ts
// Based on David Kibbe's original system, adapted for UI clarity

export const KIBBE_SECTIONS: KibbeSection[] = [
  {
    id: 'bone',
    title: 'Bone Structure',
    subtitle: 'The architectural foundation of your style type',
    iconSrc: 'assets/kibbe/icons/bone.svg',
    questions: [
      {
        id: 'bone_1',
        prompt: 'How would you describe your overall bone structure?',
        subPrompt: 'Think about sharpness vs softness of angles',
        options: [
          { letter: 'A', label: 'Sharp and angular', description: 'Prominent, chiselled angles — edges that catch the light' },
          { letter: 'B', label: 'Large and blunt', description: 'Broad and wide with softer angular edges — substantial but not sharp' },
          { letter: 'C', label: 'Moderate and symmetrical', description: 'Even, regular proportions — neither prominently sharp nor blunt' },
          { letter: 'D', label: 'Small and angular', description: 'Delicate but sharp — fine-boned with defined edges' },
          { letter: 'E', label: 'Small and rounded', description: 'Delicate and soft — no prominent angles anywhere' }
        ]
      },
      {
        id: 'bone_2',
        prompt: 'How would you describe your shoulders?',
        options: [
          { letter: 'A', label: 'Square or narrow with sharp edges' },
          { letter: 'B', label: 'Broad and wide — square with blunt edges' },
          { letter: 'C', label: 'Moderate width — slightly tapered or straight' },
          { letter: 'D', label: 'Narrow and slightly angular, or tapered' },
          { letter: 'E', label: 'Sloped and rounded — soft and narrow' }
        ]
      },
      {
        id: 'bone_3',
        prompt: 'How would you describe your hands and feet?',
        options: [
          { letter: 'A', label: 'Long and narrow, or long and broad' },
          { letter: 'B', label: 'Large — broad or long with a sturdy look' },
          { letter: 'C', label: 'Moderate — neither particularly large nor small' },
          { letter: 'D', label: 'Small and narrow, slightly angular' },
          { letter: 'E', label: 'Small and rounded — delicate and soft' }
        ]
      },
      {
        id: 'bone_4',
        prompt: 'What is your overall vertical line?',
        subPrompt: 'Your sense of height and elongation relative to your actual height',
        options: [
          { letter: 'A', label: 'Elongated — I appear tall regardless of actual height' },
          { letter: 'B', label: 'Broad — I appear wide and substantial' },
          { letter: 'C', label: 'Moderate — I appear neither very elongated nor broad' },
          { letter: 'D', label: 'Compact — I appear small and contained' },
          { letter: 'E', label: 'Petite — I appear small and delicate' }
        ]
      },
      {
        id: 'bone_5',
        prompt: 'How would you describe your jawline?',
        options: [
          { letter: 'A', label: 'Narrow and very sharp — strongly defined' },
          { letter: 'B', label: 'Wide and blunt — strong but with soft edges' },
          { letter: 'C', label: 'Moderate — symmetrical and balanced' },
          { letter: 'D', label: 'Narrow and sharp, or pointy — defined but small' },
          { letter: 'E', label: 'Rounded — soft, with little definition' }
        ]
      }
    ]
  },
  {
    id: 'flesh',
    title: 'Body Flesh',
    subtitle: 'How muscle and soft tissue express themselves on your frame',
    iconSrc: 'assets/kibbe/icons/flesh.svg',
    questions: [
      {
        id: 'flesh_1',
        prompt: 'How would you describe your overall body flesh?',
        options: [
          { letter: 'A', label: 'Taut and sinewy', description: 'Lean muscular tissue, little soft flesh — lithe or wiry' },
          { letter: 'B', label: 'Straight and muscular', description: 'Athletic build, moderate muscle, tends to remain straight not curvy' },
          { letter: 'C', label: 'Evenly distributed', description: 'Proportionate bust, waist, hips — slightly lithe and controlled' },
          { letter: 'D', label: 'Small and fine', description: 'Delicate and light — minimal flesh, stays lean' },
          { letter: 'E', label: 'Soft and rounded', description: 'Lush and fleshy — curves are the defining feature' }
        ]
      },
      {
        id: 'flesh_2',
        prompt: 'How would you describe your bust?',
        options: [
          { letter: 'A', label: 'Small or flat — not a defining feature' },
          { letter: 'B', label: 'Moderate — present but not prominent' },
          { letter: 'C', label: 'Moderate and proportionate to hips' },
          { letter: 'D', label: 'Small — may be flat or very slight' },
          { letter: 'E', label: 'Full and rounded — a prominent feature' }
        ]
      },
      {
        id: 'flesh_3',
        prompt: 'How is your waistline naturally defined?',
        options: [
          { letter: 'A', label: 'Minimal — my torso is relatively straight' },
          { letter: 'B', label: 'Slightly defined but stays straight overall' },
          { letter: 'C', label: 'Moderately defined — proportionate to bust and hips' },
          { letter: 'D', label: 'Small — defined but compact' },
          { letter: 'E', label: 'Strongly defined — clearly visible hourglass curve' }
        ]
      },
      {
        id: 'flesh_4',
        prompt: 'How would you describe your hips?',
        options: [
          { letter: 'A', label: 'Narrow — not a prominent feature' },
          { letter: 'B', label: 'Straight and even with shoulders' },
          { letter: 'C', label: 'Moderate and proportionate' },
          { letter: 'D', label: 'Small and narrow or slightly rounded' },
          { letter: 'E', label: 'Full and rounded — a prominent feature' }
        ]
      },
      {
        id: 'flesh_5',
        prompt: 'When you gain weight, where does it primarily accumulate?',
        options: [
          { letter: 'A', label: 'Upper thighs and hips — body stays relatively lean above' },
          { letter: 'B', label: 'Becomes more square/stocky — body remains fairly straight' },
          { letter: 'C', label: 'Evenly distributed throughout the body' },
          { letter: 'D', label: 'Stays proportional — remains small overall' },
          { letter: 'E', label: 'The fleshiest parts (bust, hips, waist) become more pronounced' }
        ]
      }
    ]
  },
  {
    id: 'face',
    title: 'Facial Features',
    subtitle: 'The overall impression of your face, not individual features',
    iconSrc: 'assets/kibbe/icons/face.svg',
    questions: [
      {
        id: 'face_1',
        prompt: 'How would you describe the overall impression of your facial features?',
        options: [
          { letter: 'A', label: 'Sleek and angular', description: 'Sharp, sculpted, chiselled — geometric precision' },
          { letter: 'B', label: 'Broad and blunt', description: 'Open and strong — irregular or bold in an uncontrived way' },
          { letter: 'C', label: 'Symmetrical and balanced', description: 'Even, regular, classically proportioned' },
          { letter: 'D', label: 'Small and sharp, or angular-cute', description: 'High-contrast — angular features in a compact face' },
          { letter: 'E', label: 'Soft and rounded', description: 'Lush and curved — fullness in all the features' }
        ]
      },
      {
        id: 'face_2',
        prompt: 'How would you describe your eyes?',
        options: [
          { letter: 'A', label: 'Narrow, sloe or almond-shaped — elongated' },
          { letter: 'B', label: 'Broad or straight — can be large and open or straight and strong' },
          { letter: 'C', label: 'Moderate — evenly sized and symmetrical' },
          { letter: 'D', label: 'Large relative to face, or sharp and defined' },
          { letter: 'E', label: 'Large, round and soft — a defining feature' }
        ]
      },
      {
        id: 'face_3',
        prompt: 'How would you describe your lips?',
        options: [
          { letter: 'A', label: 'Narrow and thin, or firmly set' },
          { letter: 'B', label: 'Slightly irregular or wide — natural, uncontrived' },
          { letter: 'C', label: 'Moderate and symmetrically shaped' },
          { letter: 'D', label: 'Small, possibly defined or slightly quirky' },
          { letter: 'E', label: 'Full, rounded and lush' }
        ]
      },
      {
        id: 'face_4',
        prompt: 'How would you describe your cheeks and nose?',
        options: [
          { letter: 'A', label: 'Taut cheeks, prominent or refined nose — angular' },
          { letter: 'B', label: 'Broad nose, fleshy or flat cheeks — open and strong' },
          { letter: 'C', label: 'Symmetrical nose, moderately fleshed cheeks' },
          { letter: 'D', label: 'Small nose, flat or lightly defined cheeks' },
          { letter: 'E', label: 'Small, soft nose — fleshy, full cheeks' }
        ]
      }
    ]
  }
];
```

### 5.3 Session Service

```typescript
// frontend/src/app/features/style-quiz/services/kibbe-session.service.ts
// Persists quiz state across auth boundary; max 30 min TTL

const SESSION_KEY = 'kibbe_quiz_session';
const SESSION_TTL_MS = 30 * 60 * 1000; // 30 minutes

interface KibbeSession {
  sessionId: string;
  answers: KibbeAnswer[];
  scoreResponse?: KibbeScoreResponse;
  expiresAt: number; // epoch ms
}

@Injectable({ providedIn: 'root' })
export class KibbeSessionService {

  save(answers: KibbeAnswer[], score?: KibbeScoreResponse): string {
    const sessionId = crypto.randomUUID();
    const session: KibbeSession = {
      sessionId,
      answers,
      scoreResponse: score,
      expiresAt: Date.now() + SESSION_TTL_MS
    };
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    return sessionId;
  }

  load(): KibbeSession | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    const session: KibbeSession = JSON.parse(raw);
    if (Date.now() > session.expiresAt) {
      this.clear();
      return null;
    }
    return session;
  }

  clear(): void {
    localStorage.removeItem(SESSION_KEY);
  }

  isExpired(): boolean {
    return this.load() === null && !!localStorage.getItem(SESSION_KEY);
  }
}
```

### 5.4 Analytics Service (Kibbe-Typed)

```typescript
// frontend/src/app/features/style-quiz/services/kibbe-analytics.service.ts
// Thin wrapper over the existing AnalyticsService with typed Kibbe events

export type KibbeEventName =
  | 'quiz_start'
  | 'primer_view'
  | 'primer_continue'
  | 'section_start'
  | 'section_complete'
  | 'quiz_complete'
  | 'teaser_view'
  | 'teaser_register_click'
  | 'register_complete_from_quiz'
  | 'login_complete_from_quiz'
  | 'full_results_view'
  | 'style_profile_saved'
  | 'style_profile_override'
  | 'retake_started'
  | 'retake_confirmed'
  | 'share_click'
  | 'share_complete';

@Injectable({ providedIn: 'root' })
export class KibbeAnalyticsService {
  private readonly analytics = inject(AnalyticsService);
  private readonly sessionId = crypto.randomUUID();

  track(
    event: KibbeEventName,
    extra?: Partial<{
      sectionId: string;
      family: string;
      confidence: string;
      isMixed: string;
      timeSpentMs: string;
    }>
  ): void {
    this.analytics.track(`kibbe.${event}`, {
      session_id: this.sessionId,
      ...extra
    });
  }
}
```

### 5.5 Share Card Component

```typescript
// frontend/src/app/features/style-quiz/share/share-card.component.ts
// Uses HTML Canvas to generate a shareable image dynamically

@Component({
  selector: 'app-share-card',
  template: `<canvas #canvas [width]="1080" [height]="1080" style="display:none"></canvas>`,
  standalone: true
})
export class ShareCardComponent {
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLCanvasElement>;
  @Input() family!: KibbeFamily;
  @Input() isMixed = false;
  @Input() runnerUpFamily?: KibbeFamily;

  async generateAndShare(): Promise<void> {
    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d')!;

    // Draw background gradient (family-specific)
    const gradient = this.getFamilyGradient(ctx);
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, 1080, 1080);

    // Draw family illustration (preloaded SVG as image)
    const img = await this.loadImage(`assets/kibbe/families/${this.family.toLowerCase()}.png`);
    ctx.drawImage(img, 140, 140, 800, 600);

    // Draw text overlay
    ctx.fillStyle = 'rgba(0,0,0,0.5)';
    ctx.fillRect(0, 750, 1080, 330);

    ctx.fillStyle = '#FFFFFF';
    ctx.font = 'bold 80px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText(`My Style Type`, 540, 820);

    ctx.font = 'bold 120px Georgia, serif';
    ctx.fillText(this.family, 540, 940);

    // LYKE branding
    ctx.font = '36px Arial, sans-serif';
    ctx.fillStyle = 'rgba(255,255,255,0.7)';
    ctx.fillText('Find your type at lyke.app/style-quiz', 540, 1020);

    // Share via Web Share API or copy link
    canvas.toBlob(async (blob) => {
      if (!blob) return;
      const file = new File([blob], `my-kibbe-type-${this.family.toLowerCase()}.png`, { type: 'image/png' });

      if (navigator.canShare?.({ files: [file] })) {
        await navigator.share({ files: [file], title: `I'm a ${this.family} — LYKE Style Quiz`, url: 'https://lyke.app/style-quiz' });
      } else {
        // Fallback: trigger download
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = file.name;
        a.click();
      }
    });
  }

  private getFamilyGradient(ctx: CanvasRenderingContext2D): CanvasGradient {
    const gradients: Record<KibbeFamily, [string, string]> = {
      Dramatic: ['#1a1a2e', '#c9a96e'],
      Natural:  ['#2d4a22', '#a8b89a'],
      Classic:  ['#3d1c1c', '#c8a47b'],
      Gamine:   ['#0a1628', '#4a90d9'],
      Romantic: ['#3d1a2e', '#e8a4c0']
    };
    const [from, to] = gradients[this.family];
    const g = ctx.createLinearGradient(0, 0, 0, 1080);
    g.addColorStop(0, from);
    g.addColorStop(1, to);
    return g;
  }

  private loadImage(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = src;
    });
  }
}
```

### 5.6 Key Page Pseudocode

#### Landing Page
```
KibbeQuizLandingPage:
  - Hero section: full-bleed background (editorial collage of all 5 families)
  - Headline: "Discover your true style identity"
  - Sub: "Based on David Kibbe's body type system, used by stylists worldwide"
  - Family type preview: 5 animated chips (Dramatic / Natural / Classic / Gamine / Romantic)
  - CTA button: "Find my style type" → navigate to /style-quiz/kibbe/quiz
  - Below fold: "What is the Kibbe system?" accordion
  - Track: quiz_start when CTA tapped
```

#### Quiz Engine (KibbeQuizPage)
```
State:
  currentPhase: 'primer' | 'section_intro' | 'question' | 'calculating'
  currentSectionIndex: 0-2
  currentQuestionIndex: 0-4
  answers: Map<questionId, KibbeOption>

Primer screen:
  - Display yin/yang explainer text
  - "This is about overall impression, not size or weight"
  - "Begin" button → currentPhase = 'section_intro', sectionIndex = 0
  - Track: primer_view, primer_continue

Section intro screen:
  - Show section title (e.g. "Bone Structure")
  - Animated icon + brief description
  - "Continue" → currentPhase = 'question', questionIndex = 0
  - Track: section_start

Question screen:
  - IonProgressBar showing overall progress (answered / total questions)
  - Section label (section name + "Question N of M in this section")
  - Question prompt
  - 5 option cards (A-E), each with letter badge + label + optional image
  - Selecting an option auto-advances after 300ms (smooth slide transition)
  - Back button always visible
  - Track answer selection in state

On completing last question of a section:
  - If more sections: show next section_intro
  - If last section: navigate to 'calculating' phase

Calculating screen (2.5 seconds):
  - Full-screen animated gradient matching likely family colours
  - Rotating copy: "Analysing your bone structure..." → "Reading your physical essence..." → "Almost there..."
  - After 2.5s, call POST /api/style/v1/quiz/kibbe/score
  - On success: save session, navigate to /style-quiz/kibbe/results
  - Track: quiz_complete
```

#### Teaser Results (KibbeTeaserPage)
```
Layout:
  - Hero: full-bleed family illustration, family colour palette
  - "You're likely in the {primaryFamily} family"
  - One-line family meaning (e.g., "Dramatic: Bold vertical presence with sculptural elegance")
  - Confidence chip if isMixed: "You have a {runnerUp} undercurrent"
  - Blurred section below: "Your full style guide" (CSS blur filter over placeholder cards)
  - CTA card: "Create a free account to unlock your complete style profile"
    - Social auth buttons (Google, Apple) primary
    - "Or continue with email" secondary
    - "Already have an account? Sign in"
  - On CTA tap: save session to localStorage, navigate to /auth/register?return=kibbe-results
  - Track: teaser_view, teaser_register_click
```

#### Full Results (KibbeFullResultsPage)
```
On init:
  - If not authenticated: redirect to teaser
  - Load session from KibbeSessionService
    - If expired: show "Your session expired" with "Retake quiz" CTA
    - If valid: call PUT /api/style/v1/profile with session score
    - On save: clear session, show full results
  - Also load GET /api/style/v1/profile for returning users

Layout:
  - Animated reveal: slide-in from below over 600ms
  - "Welcome, {firstName}. Here's your style profile."
  - Primary family hero card (large illustration, family name, 3-line essence)
  - If isMixed: "Your type: {mixedLabel}" (e.g. "Soft Dramatic")
  - Section breakdown:
    - Bone Structure: {boneDominance family name}
    - Body Flesh: {fleshDominance family name}
    - Facial Features: {faceDominance family name}
  - Style guide tabs: Shape / Fabrics / Colours / Accessories / Hair & Makeup
    - Each tab: 3-4 bullet guidance points from the Kibbe style data
  - Celebrity examples: "People who share your type include..."
  - Share card section: [Share card preview] [Share my type] button
  - Retake / Override section at bottom
  - Track: full_results_view, style_profile_saved
```

---

## 6. Quiz Content: Family Descriptions and Copy

These are the copy blocks needed for the full results pages and teaser screens. Drawn from the reference data.

### Dramatic (A)
**One-liner:** Bold, sculptural vertical presence — architecture made flesh.
**Essence:** Sharp angles and chiselled features define your look. You have an extreme elongated vertical that needs to be honoured — anything that interrupts this line diminishes your impact.
**Key style words:** Geometric, elongated, tailored, matte, high-contrast, minimal.
**Avoid:** Anything overly fussy, rounded, small-print, or that shortens the vertical line.
**Celebrities:** Lauren Bacall, Tilda Swinton, Cate Blanchett.

### Natural (B)
**One-liner:** Effortless, earthy vitality — the impression of authentic, uncontrived beauty.
**Essence:** Slightly angular bones with a relaxed, athletic body. Your look is best served by clothing that moves with you — never stiff, never overly structured, always free.
**Key style words:** Relaxed, textured, unconstructed, layered, organic, earthy.
**Avoid:** Overly formal tailoring, delicate fabrics, ornate detail, anything that looks "done."
**Celebrities:** Ingrid Bergman, Julia Roberts, Jennifer Aniston.

### Classic (C)
**One-liner:** Polished symmetry — timeless elegance through perfect proportion.
**Essence:** Your balanced proportions are your greatest asset. You need clothing that echoes your own natural harmony — never extreme in any direction, always refined and controlled.
**Key style words:** Proportioned, symmetrical, tailored, refined, understated, moderate.
**Avoid:** Extremes of any kind — overly bold, overly soft, too much detail, too little. The middle path is yours.
**Celebrities:** Grace Kelly, Kate Middleton, Carolyn Bessette-Kennedy.

### Gamine (D)
**One-liner:** Compact, high-contrast dynamism — the most visually striking of all types.
**Essence:** Your small frame contains enormous presence. The combination of angular bone structure with rounded features creates natural visual contrast that comes alive in bold, mixed looks.
**Key style words:** Geometric, contrasting, compact, playful, sharp, eclectic.
**Avoid:** Elongated vertical lines (they overwhelm your frame), bland monotone looks, overly soft or overly angular choices alone — you thrive on the mix.
**Celebrities:** Audrey Hepburn, Leslie Caron, Mia Farrow.

### Romantic (E)
**One-liner:** Lush, soft curves and delicate femininity — the embodiment of Yin energy.
**Essence:** Softness is your strength. Your curves and delicacy need to be dressed with equally soft, draped, and flowing clothing. Structure and angularity work against your natural elegance.
**Key style words:** Draped, soft, rounded, plush, feminine, delicate.
**Avoid:** Stiff fabrics, sharp geometric shapes, oversized silhouettes, or anything that overwhelms your delicate frame.
**Celebrities:** Marilyn Monroe, Elizabeth Taylor, Salma Hayek.

### Mixed Type Labels
When `isMixed = true`, the displayed type label combines both families:

| Primary | Runner-Up | Display Label |
|---------|-----------|--------------|
| Dramatic | Romantic | Soft Dramatic |
| Natural | Dramatic | Flamboyant Natural |
| Natural | Romantic | Soft Natural |
| Classic | Dramatic | Dramatic Classic |
| Classic | Romantic | Soft Classic |
| Romantic | Dramatic | Theatrical Romantic |
| Gamine | Dramatic | Flamboyant Gamine |
| Gamine | Romantic | Soft Gamine |
| Any other mix | Any | "{Primary} with {Runner-Up} notes" |

---

## 7. Analytics Event Schema

All events are emitted through `KibbeAnalyticsService.track()` which wraps the existing `AnalyticsService`. Events appear in AppInsights as `kibbe.{event_name}`.

```
Standard properties on every Kibbe event:
  session_id: string     — UUID generated at quiz start, persists through auth
  user_id: string?       — populated if authenticated, else omitted
  timestamp: ISO string  — from AppInsights

Event-specific properties:

quiz_start           → (no extra props)
primer_view          → (no extra props)
primer_continue      → time_spent_ms: string
section_start        → section_id: "bone"|"flesh"|"face"
section_complete     → section_id, time_spent_ms
quiz_complete        → family: primary family name, is_mixed: "true"|"false"
teaser_view          → family, confidence
teaser_register_click → family
register_complete_from_quiz → (no extra, user_id now populated)
login_complete_from_quiz    → (no extra, user_id now populated)
full_results_view    → family, is_mixed, confidence
style_profile_saved  → family, is_mixed, confidence
style_profile_override → family (new override family)
retake_started       → (no extra)
retake_confirmed     → (no extra)
share_click          → family
share_complete       → family
```

---

## 8. Visual Asset Requirements

### 8.1 Required Assets

```
frontend/src/assets/kibbe/
  families/
    dramatic.png          — Editorial illustration/photo, portrait 2:3
    natural.png           — Editorial illustration/photo, portrait 2:3
    classic.png           — Editorial illustration/photo, portrait 2:3
    gamine.png            — Editorial illustration/photo, portrait 2:3
    romantic.png          — Editorial illustration/photo, portrait 2:3
    dramatic-hero.jpg     — Full-bleed hero, landscape 16:9
    natural-hero.jpg
    classic-hero.jpg
    gamine-hero.jpg
    romantic-hero.jpg
  icons/
    bone.svg              — Section icon: skeleton/bone motif
    flesh.svg             — Section icon: body form motif
    face.svg              — Section icon: face/features motif
  share-cards/
    dramatic-bg.jpg       — Background for canvas share card
    natural-bg.jpg
    classic-bg.jpg
    gamine-bg.jpg
    romantic-bg.jpg
  yin-yang.svg            — For primer screen: yin/yang concept graphic
```

### 8.2 Free/Open Image Sources

**Photography (free commercial use):**
- **Unsplash** (unsplash.com) — Highest editorial quality. Search:
  - Dramatic: `"editorial fashion portrait" "avant-garde"`
  - Natural: `"effortless bohemian style" "outdoor casual fashion"`
  - Classic: `"timeless elegant fashion" "minimalist portrait"`
  - Gamine: `"pixie crop fashion" "androgynous portrait"`
  - Romantic: `"soft feminine fashion" "floral dress portrait"`

**Illustration (free/low-cost):**
- **Humaaans** (humaaans.com) — Mix-and-match human figures, free for commercial use, adjustable body types. Ideal for type cards.
- **Blush Design** (blush.design) — "Comfy" and "Street Style" packs. Some free, most ~$9/month. CC licensed.
- **unDraw** (undraw.co) — Abstract SVG illustrations, fully free. Good for section icons and empty states.

**Production recommendation:** Commission a set of 5 fashion illustrations representing each family archetype (abstract, not photographic). Budget £600–£1,500 for a set from a freelancer via Dribbble or Behance. Illustration is preferred over photography because:
1. Avoids body representation sensitivity issues
2. Can abstract the "essence" of each type without being literal
3. Works better as a consistent visual language across the app

### 8.3 Colour System by Family

Use CSS custom properties scoped per family:

```scss
// Dramatic
--family-primary: #1a1a1a;
--family-accent: #c9a96e;
--family-bg: #f5f0e8;
--family-text: #1a1a1a;

// Natural
--family-primary: #2d4a22;
--family-accent: #a8b89a;
--family-bg: #f4f1ec;
--family-text: #2d3b1f;

// Classic
--family-primary: #5c2c2c;
--family-accent: #c8a47b;
--family-bg: #faf8f5;
--family-text: #3d2222;

// Gamine
--family-primary: #0a1628;
--family-accent: #4a90d9;
--family-bg: #f0f4fa;
--family-text: #0a1628;

// Romantic
--family-primary: #3d1a2e;
--family-accent: #e8a4c0;
--family-bg: #fdf5f8;
--family-text: #3d1a2e;
```

---

## 9. Task Breakdown

### Phase 1: Backend Foundation (Days 1–3)

- [ ] **T1.1** Add `KibbeFamily` and `KibbeConfidence` enums to `Lyke.Core/Enums`
- [ ] **T1.2** Add `StyleProfile` entity to `Lyke.Core/Entities`
- [ ] **T1.3** Add `StyleProfile` navigation property to `User` entity
- [ ] **T1.4** Add `StyleProfileConfiguration` EF Core config to `Lyke.Infrastructure`
- [ ] **T1.5** Register `StyleProfileConfiguration` in `AppDbContext`
- [ ] **T1.6** Create EF Core migration: `AddStyleProfile`
- [ ] **T1.7** Create `KibbeQuizDtos.cs` and `StyleProfileDtos.cs` in Application DTOs
- [ ] **T1.8** Create `IKibbeQuizService` interface
- [ ] **T1.9** Implement `KibbeQuizService` scoring algorithm
- [ ] **T1.10** Implement `KibbeQuizService` save/get/override profile methods
- [ ] **T1.11** Register `KibbeQuizService` in DI container
- [ ] **T1.12** Implement `StyleEndpoints.cs` (5 endpoints)
- [ ] **T1.13** Map `StyleEndpoints` in `Program.cs`
- [ ] **T1.14** Add `FeatureFlags.KibbeFeedBoost` configuration

### Phase 2: Backend Tests (Day 3)

- [ ] **T2.1** Create `KibbeQuizServiceTests.cs` with all 10 test cases
- [ ] **T2.2** Run `dotnet test` — all tests passing

### Phase 3: Frontend Foundation (Days 4–5)

- [ ] **T3.1** Create `style-quiz` feature folder and module/routing files
- [ ] **T3.2** Create `kibbe.models.ts` with all TypeScript interfaces
- [ ] **T3.3** Create `kibbe-quiz.data.ts` with all 14 questions across 3 sections
- [ ] **T3.4** Create `KibbeSessionService` with localStorage persistence + TTL
- [ ] **T3.5** Create `KibbeAnalyticsService` wrapping `AnalyticsService`
- [ ] **T3.6** Create `KibbeQuizService` (API calls)
- [ ] **T3.7** Add lazy route to main `app.routes.ts`: `/style-quiz` → style-quiz module
- [ ] **T3.8** Add auth guard to `/style-quiz/kibbe/full-results` route

### Phase 4: Quiz UI Pages (Days 5–9)

- [ ] **T4.1** **Landing page** — hero, CTA, family type preview chips, yin/yang explainer
- [ ] **T4.2** **Primer screen** — yin/yang concept card, begin CTA, analytics events
- [ ] **T4.3** **Section intro screen** — animated card with section title, icon, description
- [ ] **T4.4** **Question screen** — 5-option cards (A-E), progress bar, back/next, auto-advance
- [ ] **T4.5** **Calculating screen** — animated gradient, rotating copy, 2.5s minimum
- [ ] **T4.6** **Teaser results page** — family hero, one-liner, blurred section, registration CTA
- [ ] **T4.7** **Full results page** — animated reveal, family breakdown, style guide tabs, celebrity examples
- [ ] **T4.8** **Style guide tab content** — per-family content for Shape, Fabrics, Colours, Accessories, Hair & Makeup
- [ ] **T4.9** **Retake quiz flow** — confirmation modal, re-run quiz, overwrite profile
- [ ] **T4.10** **Family override UI** — family selector, self-selected label on profile
- [ ] **T4.11** **Style profile page** (`/profile/style`) — saved result view, retake/override CTAs

### Phase 5: Share Card (Day 9–10)

- [ ] **T5.1** Implement `ShareCardComponent` using HTML Canvas
- [ ] **T5.2** Integrate Web Share API with file export
- [ ] **T5.3** Implement download fallback for non-supporting browsers
- [ ] **T5.4** Add share section to full results page
- [ ] **T5.5** Add analytics: `share_click`, `share_complete`

### Phase 6: Auth Integration (Day 10)

- [ ] **T6.1** Pass `?return=kibbe-results` query param to register/login from teaser
- [ ] **T6.2** In `AuthService.onLoginSuccess()`: check for `return=kibbe-results` → navigate to full-results
- [ ] **T6.3** Implement session expiry message on full-results page (friendly retake prompt)
- [ ] **T6.4** Track `register_complete_from_quiz` and `login_complete_from_quiz` in auth flow

### Phase 7: Assets and Polish (Days 10–12)

- [ ] **T7.1** Source and optimise 5 family portrait images (Unsplash minimum, commission ideal)
- [ ] **T7.2** Create or source 5 family hero images (landscape)
- [ ] **T7.3** Create 3 section icons (bone, flesh, face) — SVG
- [ ] **T7.4** Apply family colour system (CSS custom properties) to all results screens
- [ ] **T7.5** Add entrance animations (Ionic animations or CSS `@keyframes`)
- [ ] **T7.6** Test on iOS Safari, Android Chrome, and PWA (responsive)
- [ ] **T7.7** Accessibility: ARIA labels on option cards, focus management between questions

### Phase 8: Feed Personalisation Hook (Day 12–13, Optional)

- [ ] **T8.1** Add `KibbeFamily` nullable column to `Creator` entity
- [ ] **T8.2** Implement feed score boost in `FeedService` behind `KibbeFeedBoost` feature flag
- [ ] **T8.3** Read `StyleProfile.PrimaryFamily` in feed request pipeline when flag is on
- [ ] **T8.4** Add `kibbe_family` to creator profile edit flow (optional, self-reported)

---

## 10. API Contract Quick Reference

### POST `/api/style/v1/quiz/kibbe/score`
```json
Request:
{
  "answers": [
    { "sectionId": "bone", "questionId": "bone_1", "selectedOption": "A" },
    { "sectionId": "bone", "questionId": "bone_2", "selectedOption": "A" },
    ...
  ]
}

Response 200:
{
  "success": true,
  "data": {
    "primaryFamily": "Dramatic",
    "runnerUpFamily": "Natural",
    "sectionDominance": {
      "bone": "Dramatic",
      "flesh": "Dramatic",
      "face": "Natural"
    },
    "isMixed": true,
    "confidence": "Medium",
    "counts": { "A": 9, "B": 5, "C": 0, "D": 0, "E": 0 }
  }
}

Response 400 (validation):
{
  "success": false,
  "error": { "code": "INVALID_REQUEST", "message": "Answers are required" }
}
```

### PUT `/api/style/v1/profile`
```json
Request:
{
  "score": {
    "primaryFamily": "Dramatic",
    "runnerUpFamily": "Natural",
    "sectionDominance": { "bone": "Dramatic", "flesh": "Dramatic", "face": "Natural" },
    "isMixed": true,
    "confidence": "Medium",
    "counts": { "A": 9, "B": 5, "C": 0, "D": 0, "E": 0 }
  }
}

Response 200:
{
  "success": true,
  "data": {
    "primaryFamily": "Dramatic",
    "runnerUpFamily": "Natural",
    "isMixed": true,
    "confidence": "Medium",
    "sectionDominance": { "bone": "Dramatic", "flesh": "Dramatic", "face": "Natural" },
    "isUserOverride": false,
    "overrideFamily": null,
    "computedAt": "2026-03-09T12:00:00Z",
    "lastUpdatedAt": "2026-03-09T12:00:00Z"
  }
}
```

### PUT `/api/style/v1/profile/override`
```json
Request: { "family": "Classic" }
Response 200: StyleProfileResponse with isUserOverride: true, overrideFamily: "Classic"
```

---

## 11. Migration Notes

The `AddStyleProfile` migration adds a single new table `style_profiles` with a 1:1 relationship to `users`. It is **non-breaking** — the column is nullable on the user side, so all existing users have `StyleProfile = null` until they complete the quiz.

**Steps:**
1. Run `dotnet ef migrations add AddStyleProfile --project backend/src/Lyke.Infrastructure --startup-project backend/src/Lyke.Api`
2. Review the generated migration SQL
3. `dotnet ef database update` on dev/staging before deploying the API endpoints
4. Auto-migrations on startup will handle production (already configured)

---

## 12. References and Background Sources

- **Kibbe, David** — *Metamorphosis: Discover Your Image Identity and Dazzle As Only You Can* (1987). Original 13-type system; the community has since consolidated to 5 families.
- **The Concept Wardrobe** — "An Introduction to the Kibbe Body Types" (PDF in this folder). Good explainer of the 5-family framework.
- **Kibbe Resources Spreadsheet** (Excel in this folder) — Physical profile descriptions per subtype, style recommendations per family for: shape, line, fabric, detail, separates, jackets, skirts, pants, blouses, dresses, colour, prints, accessories, shoes, bags, belts, hats, hosiery, jewelry, evening wear, hair, makeup.
- **Reddit r/Kibbe** — Community discussion, celebrity analyses, visual guides: `reddit.com/r/Kibbe`
- **Style Syntax** (`stylesyntax.com`) — Most rigorous academic Kibbe resource
- **Yin/Yang elements source** — `reddit.com/r/Kibbe/comments/gpdjkb/yin_and_yang_representative_clothing_styles_and/`
- **UX research sources**: NNGroup quiz patterns, CXL quiz completion research, Typeform benchmark data, Interact.com quiz UX guide, Prose/Noom/Function of Beauty funnel analyses

---

*End of implementation plan. See `kibbe_quiz.md` for the original feature specification.*
