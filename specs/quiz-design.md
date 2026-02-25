# Body Type Quiz Design Specification

## Overview

The LYKE Body Type Quiz is a 6-question assessment that determines a user's body shape, stature, and build. The quiz uses a weighted scoring algorithm to classify users into one of five body shapes, combined with stature and build modifiers. Results are used to match users with creators who have similar body profiles.

### Design Goals

1. **Accessibility** — Anonymous access allows social media sharing and viral engagement
2. **Accuracy** — Weighted scoring matrices provide nuanced classification
3. **Editability** — Results auto-populate but users can override selections
4. **Privacy** — No personal measurements required; uses relative proportions only

---

## Body Shape Classifications

The quiz determines one of five body shapes based on proportional characteristics:

| ID | Shape | Description | Key Characteristics |
|----|-------|-------------|---------------------|
| 1 | **Hourglass** | Balanced bust and hips with defined waist | Shoulders and hips approximately equal width; clearly defined waistline |
| 2 | **Pear** | Hips wider than shoulders | Weight carried in lower body; narrower shoulders; defined waist |
| 3 | **Apple** | Fuller midsection with slimmer legs | Weight carried in midsection; less waist definition; slimmer lower body |
| 4 | **Rectangle** | Balanced proportions, less waist definition | Shoulders, waist, and hips similar width; athletic or straight silhouette |
| 5 | **Inverted Triangle** | Shoulders wider than hips | Broader shoulders/bust; narrower hips; weight in upper body |

---

## Stature and Build Modifiers

In addition to body shape, the quiz captures two modifiers:

### Stature (Question 5)

| Value | Enum | Description |
|-------|------|-------------|
| Petite | `Stature.Petite` (1) | Shorter stature, typically under 5'3" / 160cm |
| Average | `Stature.Average` (2) | Medium height range |
| Tall | `Stature.Tall` (3) | Taller stature, typically over 5'7" / 170cm |

### Build (Question 6)

| Value | Enum | Description |
|-------|------|-------------|
| Standard | `Build.Standard` (1) | Standard clothing size range |
| Plus | `Build.Plus` (2) | Plus-size clothing range |

---

## Quiz Questions

### Question 1: Shoulder-Hip Comparison

**Prompt:** "When you look in the mirror, how do your shoulders compare to your hips?"

| Index | Answer | Indicates |
|-------|--------|-----------|
| 0 | My shoulders are noticeably wider than my hips | Inverted Triangle, Apple, Rectangle |
| 1 | My shoulders and hips are about the same width | Hourglass, Rectangle |
| 2 | My hips are noticeably wider than my shoulders | Pear |

---

### Question 2: Waist Definition

**Prompt:** "How would you describe your natural waistline?"

| Index | Answer | Indicates |
|-------|--------|-----------|
| 0 | Very defined — my waist is noticeably narrower than my bust and hips | Hourglass, Pear |
| 1 | Moderately defined — there's some curve but not dramatic | Neutral (all shapes) |
| 2 | Minimal definition — my waist, bust, and hips are similar measurements | Rectangle, Apple |

---

### Question 3: Weight Distribution

**Prompt:** "When you gain weight, where does it tend to go first?"

| Index | Answer | Indicates |
|-------|--------|-----------|
| 0 | Upper body (shoulders, arms, bust area) | Inverted Triangle |
| 1 | Midsection (stomach, waist, back) | Apple |
| 2 | Lower body (hips, thighs, bottom) | Pear |
| 3 | Evenly distributed throughout my body | Hourglass, Rectangle |

---

### Question 4: Clothing Fit Challenges

**Prompt:** "When shopping for clothes, where do you most often need adjustments?"

| Index | Answer | Indicates |
|-------|--------|-----------|
| 0 | Shoulders/bust area is often too tight | Inverted Triangle |
| 1 | Waist/midsection needs more room | Apple |
| 2 | Hips/thighs are snug while waist gaps | Pear, Hourglass |
| 3 | Clothes fit similarly throughout — alterations are consistent | Rectangle |

---

### Question 5: Stature

**Prompt:** "How would you describe your height?"

| Index | Answer | Maps To |
|-------|--------|---------|
| 0 | Petite (under 5'3" / 160cm) | `Stature.Petite` |
| 1 | Average (5'3" - 5'7" / 160-170cm) | `Stature.Average` |
| 2 | Tall (over 5'7" / 170cm) | `Stature.Tall` |

---

### Question 6: Build

**Prompt:** "Which best describes your typical clothing size range?"

| Index | Answer | Maps To |
|-------|--------|---------|
| 0 | Standard sizes (XS-XL, 0-14) | `Build.Standard` |
| 1 | Plus sizes (1X+, 16+) | `Build.Plus` |

---

## Scoring Algorithm

### Scoring Matrices

Questions 1-4 use weighted scoring matrices. Each answer awards points to multiple body shapes simultaneously. The shape with the highest total score wins.

**Matrix Format:** `[Hourglass, Pear, Apple, Rectangle, Inverted Triangle]`

#### Q1: Shoulder-Hip Comparison

```
Answer 0 (Shoulders wider):    [0, 0, 1, 1, 3]
Answer 1 (Same width):         [2, 1, 1, 2, 0]
Answer 2 (Hips wider):         [0, 3, 0, 1, 0]
```

#### Q2: Waist Definition

```
Answer 0 (Very defined):       [3, 2, 0, 0, 1]
Answer 1 (Moderate):           [1, 1, 1, 1, 1]
Answer 2 (Minimal):            [0, 0, 2, 3, 1]
```

#### Q3: Weight Distribution

```
Answer 0 (Upper body):         [0, 0, 1, 0, 3]
Answer 1 (Midsection):         [0, 0, 3, 1, 0]
Answer 2 (Lower body):         [1, 3, 0, 0, 0]
Answer 3 (Evenly):             [2, 1, 1, 2, 1]
```

#### Q4: Clothing Fit Challenges

```
Answer 0 (Tight shoulders):    [0, 0, 0, 0, 2]
Answer 1 (Tight middle):       [0, 0, 2, 0, 0]
Answer 2 (Tight hips):         [1, 2, 0, 0, 0]
Answer 3 (Same everywhere):    [0, 0, 0, 2, 0]
```

### Score Calculation

1. Initialize score array: `[0, 0, 0, 0, 0]` for each body shape
2. For each answer to Q1-Q4, add the corresponding row from the scoring matrix
3. The body shape with the highest total score is selected

**Example:**
```
User answers: Q1=1, Q2=0, Q3=3, Q4=2

Q1 (Same width):     [2, 1, 1, 2, 0]
Q2 (Very defined):   [3, 2, 0, 0, 1]
Q3 (Evenly):         [2, 1, 1, 2, 1]
Q4 (Tight hips):     [1, 2, 0, 0, 0]
─────────────────────────────────────
Total:               [8, 6, 2, 4, 2]

Winner: Hourglass (score: 8)
```

### Maximum Possible Scores

| Shape | Max Score | Achieved By |
|-------|-----------|-------------|
| Hourglass | 8 | Q1=1, Q2=0, Q3=3, Q4=2 |
| Pear | 10 | Q1=2, Q2=0, Q3=2, Q4=2 |
| Apple | 8 | Q1=0, Q2=2, Q3=1, Q4=1 |
| Rectangle | 8 | Q1=1, Q2=2, Q3=3, Q4=3 |
| Inverted Triangle | 9 | Q1=0, Q2=2, Q3=0, Q4=0 |

---

## Tie-Breaker Logic

When two or more body shapes have equal highest scores, the tie-breaker priority determines the winner:

**Priority Order (highest to lowest):**
1. Hourglass
2. Pear
3. Rectangle
4. Apple
5. Inverted Triangle

**Rationale:** Hourglass and Pear are the most common body shapes in the general population, so ties are resolved toward these more prevalent classifications. This reduces edge cases where users might feel misclassified.

---

## Result Label Formatting

The result label combines stature, build, and body shape into a human-readable string.

### Formatting Rules

1. **Stature** — Include only if NOT "Average" (Average is the default/baseline)
2. **Build** — Include only if NOT "Standard" (Standard is the default/baseline)
3. **Shape** — Always included

### Examples

| Stature | Build | Shape | Result Label |
|---------|-------|-------|--------------|
| Petite | Plus | Hourglass | "Petite Plus Hourglass" |
| Tall | Standard | Pear | "Tall Pear" |
| Average | Plus | Rectangle | "Plus Rectangle" |
| Petite | Standard | Apple | "Petite Apple" |
| Average | Standard | Inverted Triangle | "Inverted Triangle" |

---

## API Specification

### Endpoint

```
POST /api/quiz/v1/calculate
```

**Authentication:** None required (anonymous access for shareability)

### Request

```json
{
  "answers": [
    { "questionId": 1, "answerIndex": 1 },
    { "questionId": 2, "answerIndex": 0 },
    { "questionId": 3, "answerIndex": 3 },
    { "questionId": 4, "answerIndex": 2 },
    { "questionId": 5, "answerIndex": 0 },
    { "questionId": 6, "answerIndex": 1 }
  ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `answers` | array | Yes | List of quiz answers |
| `answers[].questionId` | int | Yes | Question ID (1-6) |
| `answers[].answerIndex` | int | Yes | Zero-based answer index |

### Response

```json
{
  "success": true,
  "data": {
    "bodyTypeId": 1,
    "bodyTypeName": "Hourglass",
    "stature": "Petite",
    "build": "Plus",
    "resultLabel": "Petite Plus Hourglass",
    "resultDescription": "We'll show you content from creators with similar proportions."
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `bodyTypeId` | int | Database ID of the body type (1-5) |
| `bodyTypeName` | string | Human-readable body shape name |
| `stature` | string | Stature classification (Petite, Average, Tall) |
| `build` | string | Build classification (Standard, Plus) |
| `resultLabel` | string | Formatted display label |
| `resultDescription` | string | Explanation text for the user |

### Validation Rules

- `answers` array must not be empty
- `answers` array cannot exceed 10 items
- `questionId` must be between 1 and 10
- `answerIndex` must be >= 0 and <= 10

### Error Response

```json
{
  "success": false,
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Quiz answers are required"
  }
}
```

---

## Default Behaviors

| Scenario | Behavior |
|----------|----------|
| Q5 (Stature) not answered | Defaults to `Average` |
| Q6 (Build) not answered | Defaults to `Standard` |
| Invalid question ID | Answer ignored, no error |
| Invalid answer index | Answer ignored, no error |
| All Q1-Q4 skipped | Returns shape with highest tie-breaker priority (Hourglass) |
| Empty answers array | Returns defaults: Hourglass, Average, Standard |

---

## Database Integration

### Body Type Mapping

| Shape Name | Database ID |
|------------|-------------|
| Hourglass | 1 |
| Pear | 2 |
| Apple | 3 |
| Rectangle | 4 |
| Inverted Triangle | 5 |

### Body Profile Fields

Quiz results can be saved to the user's `BodyProfile` entity:

```csharp
public class BodyProfile
{
    public int BodyTypeId { get; set; }      // From quiz result
    public Stature? Stature { get; set; }    // From Q5
    public Build? Build { get; set; }        // From Q6
    // ... other fields
}
```

---

## Frontend Integration Notes

### Suggested UI Flow

1. **Introduction screen** — Explain what the quiz determines and how results are used
2. **Questions 1-4** — Body shape assessment (progress indicator: 1/6 to 4/6)
3. **Question 5** — Stature selection (progress: 5/6)
4. **Question 6** — Build selection (progress: 6/6)
5. **Results screen** — Display result label with body shape illustration
6. **Confirmation** — Allow user to accept or manually adjust selections

### Social Sharing

The anonymous endpoint enables sharing quiz results on social media:

- Generate shareable image/card with result label and silhouette
- Deep link back to LYKE app/website
- Track viral coefficient through referral codes (future enhancement)

### Accessibility Considerations

- All questions should have clear, jargon-free language
- Include visual aids (silhouette illustrations) alongside text options
- Support screen readers with proper ARIA labels
- Allow keyboard navigation through answer options

---

## Future Enhancements

1. **Confidence scoring** — Show users how strongly they match their result vs. alternatives
2. **Secondary type** — Display "You're also close to Pear" when scores are near-tied
3. **Retake flow** — Allow users to retake quiz and compare results
4. **A/B testing** — Test alternative question wordings for accuracy improvement
5. **Machine learning** — Train model on user feedback to improve scoring weights
