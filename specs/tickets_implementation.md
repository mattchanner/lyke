# Tickets Implementation Plan

---

## TICKET 1 — Improve body measurement & body type guidance during signup

**Current state:** The onboarding page (`features/onboarding/onboarding.page`) is a 4-step wizard (Height, Weight, Body Type, Fit Preference). No guidance, tooltips, or measurement help exists. Body types are rendered as a flat grid loaded from `GET /api/lookup/v1/body-types`.

### Proposed approach

**Phase 1 — Contextual guidance (in-scope)**

1. **"How to measure" expandable panel on Height & Weight steps**
   - Add an `<ion-accordion>` or info button that opens an inline panel with:
     - Simple illustration (SVG/image) showing where to measure
     - 2-3 bullet points of instruction (e.g. "Stand barefoot against a wall")
   - Keep it collapsible so it doesn't clutter the flow for users who don't need it

2. **Body type explainer cards**
   - On the Body Type step (step 3), enhance each grid card with:
     - A silhouette icon per body shape (SVG assets — one per shape)
     - The existing `description` field from the API (already available but not displayed)
   - Add a "Not sure?" link that opens a modal with side-by-side shape comparisons

3. **Fit preference descriptions**
   - Step 4 already shows Fitted/Regular/Relaxed cards. Add a one-line example beneath each (e.g. "Fitted — Clothes follow your body contour, like a tailored shirt")

**Phase 2 — Body type quiz (future, optional)**

4. **Hosted quiz component**
   - Multi-question flow (5-7 questions about shoulder-to-hip ratio, waist definition, overall frame)
   - Output: recommended shape + frame size (maps to the multi-axis model from Ticket 2)
   - Auto-populates selections but remains editable
   - Can be shared on social media as an engagement hook

### Files to modify

| File | Change |
|------|--------|
| `frontend/src/app/features/onboarding/onboarding.page.html` | Add accordion panels, description text, silhouette icons |
| `frontend/src/app/features/onboarding/onboarding.page.ts` | Add toggle signals for help panels |
| `frontend/src/app/features/onboarding/onboarding.page.scss` | Styles for help panels, silhouettes |
| `frontend/src/assets/` | Add SVG silhouette icons for each body shape |

### Backend changes

None for Phase 1. The `BodyTypeResponse` already includes `description` — just needs to be surfaced in the UI.

### Complexity: Low-Medium

---

## TICKET 2 — Rework body type taxonomy to multi-axis model

**Current state:** Single `BodyTypeId` (int FK) on `BodyProfile`. Eight seeded values mixing size (Petite, Plus Size) with shape (Hourglass, Pear, etc.). `FitPreference` is a separate enum (Fitted/Regular/Relaxed). Frontend selects one body type from a grid.

### Proposed approach — Multi-axis: Shape + Frame

Split the current single body type into two independent axes:

- **Body Shape** (silhouette/proportion): Hourglass, Pear, Apple, Rectangle, Inverted Triangle
- **Frame Size** (build/stature): Petite, Average, Tall, Plus

Users select one from each axis. This allows combinations like "Petite + Hourglass" or "Tall + Rectangle".

### Backend changes

**1. New entity: `FrameSize`**

```csharp
// Lyke.Core/Entities/FrameSize.cs
public class FrameSize
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
```

Seeded values:

| ID | Name | Description |
|----|------|-------------|
| 1 | Petite | Shorter stature with a smaller overall frame |
| 2 | Average | Medium height and proportional build |
| 3 | Tall | Taller stature with a longer frame |
| 4 | Plus | Fuller figure across all areas |

**2. Rename existing `BodyType` → keep as `BodyShape`**

Rather than renaming the table (breaking change), keep the `BodyType` table but update its seeded data to pure shapes only:

| ID | Name (updated) | Description |
|----|----------------|-------------|
| 1 | Hourglass | Balanced bust and hips with defined waist |
| 2 | Pear | Hips wider than shoulders |
| 3 | Apple | Fuller midsection with slimmer legs |
| 4 | Rectangle | Balanced proportions, less waist definition |
| 5 | Inverted Triangle | Shoulders wider than hips |

Remove old entries: Petite (1), Slim (2), Athletic (3), Plus Size (8) — these concepts move to FrameSize.

**3. Update `BodyProfile` entity**

```csharp
public class BodyProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public int HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public int BodyTypeId { get; set; }       // Now = body shape
    public int? FrameSizeId { get; set; }      // NEW
    public FitPreference? FitPreference { get; set; }

    public User User { get; set; } = null!;
    public BodyType BodyType { get; set; } = null!;
    public FrameSize? FrameSize { get; set; }  // NEW
}
```

**4. Migration**

- Add `FrameSizes` table
- Add `FrameSizeId` (nullable int FK) to `BodyProfiles`
- Data migration: map existing `BodyTypeId` values to new shape + frame:
  - Old Petite (1) → FrameSize=Petite, BodyType=null (user picks shape later)
  - Old Slim (2) → FrameSize=Average, BodyType=Rectangle
  - Old Athletic (3) → FrameSize=Average, BodyType=Inverted Triangle
  - Old Hourglass (4) → BodyType=Hourglass (1), FrameSize=null
  - Old Pear (5) → BodyType=Pear (2), FrameSize=null
  - Old Apple (6) → BodyType=Apple (3), FrameSize=null
  - Old Rectangle (7) → BodyType=Rectangle (4), FrameSize=null
  - Old Plus Size (8) → FrameSize=Plus, BodyType=null
- Update BodyType seed data to the 5 pure shapes
- Seed FrameSize with 4 values

**5. Update DTOs and endpoints**

- `BodyProfileResponse`: add `frameSizeId`, `frameSizeName`
- `CreateBodyProfileRequest` / `UpdateBodyProfileRequest`: add optional `frameSizeId`
- `AnonymizedBodyProfileResponse`: add `frameSizeName`
- `GET /api/lookup/v1/frame-sizes`: new lookup endpoint
- Feed matching algorithm: update similarity scoring to consider both axes

### Frontend changes

| File | Change |
|------|--------|
| `models/profile/profile.model.ts` | Add `FrameSizeResponse`, update request/response interfaces |
| `models/enums/index.ts` | No change (FrameSize is a lookup, not enum) |
| `features/onboarding/onboarding.page.*` | Split step 3 into two sub-steps: Shape picker then Frame picker |
| `features/profile/body-profile-edit/*` | Add frame size selector |
| `core/services/profile.service.ts` | Add `getFrameSizes()` method |

### Migration safety

- `FrameSizeId` is nullable so existing profiles remain valid
- Users with unmapped shapes (old Petite/Slim/Athletic/Plus Size → null BodyType) should be prompted to update their shape on next profile visit
- Add a `needsBodyTypeUpdate` computed flag to `BodyProfileResponse` when shape or frame is null

### Complexity: High (full-stack, migration, data mapping)

---

## TICKET 3 — Enable multi-select for fit preferences

**Current state:** `FitPreference` is a single enum value (Fitted=0, Regular=1, Relaxed=2) stored on `BodyProfile.FitPreference`. The onboarding UI renders three cards; selecting one deselects the others.

### Proposed approach

Change from a single enum to a collection, allowing users to select 1-3 fit preferences.

### Backend changes

**1. New join table: `BodyProfileFitPreference`**

```csharp
public class BodyProfileFitPreference
{
    public Guid BodyProfileId { get; set; }
    public FitPreference FitPreference { get; set; }

    public BodyProfile BodyProfile { get; set; } = null!;
}
```

**2. Update `BodyProfile` entity**

- Keep `FitPreference?` property temporarily for backward compatibility during migration
- Add `ICollection<BodyProfileFitPreference> FitPreferences`

**3. Migration**

- Create `BodyProfileFitPreferences` table (composite PK: BodyProfileId + FitPreference)
- Migrate existing single values: INSERT into join table from current `FitPreference` column
- Drop `FitPreference` column from `BodyProfiles` after migration verified

**4. Update DTOs**

- `BodyProfileResponse`: change `fitPreference: FitPreference | null` → `fitPreferences: FitPreference[]`
- `CreateBodyProfileRequest` / `UpdateBodyProfileRequest`: change to `fitPreferences: FitPreference[]`
- Feed matching: update similarity scoring — match if ANY of the user's preferences overlap with the post's fit tags

### Frontend changes

| File | Change |
|------|--------|
| `models/profile/profile.model.ts` | Update interfaces to use `fitPreferences: FitPreference[]` |
| `features/onboarding/onboarding.page.html` | Change step 4 cards to toggle selection (not radio) |
| `features/onboarding/onboarding.page.ts` | Use `Set<FitPreference>` signal, toggle on tap |
| `features/profile/body-profile-edit/*` | Same multi-select UI |

### UX details

- Cards get a visible checkmark and border highlight when selected
- At least one selection required to proceed (validation)
- Existing "Relaxed" description updated to clarify "Oversized / loose fits"

### Complexity: Medium

---

## TICKET 4 — Soft nudge for email verification + resend option

**Current state:** Backend sends a verification email on registration (`AuthService.RegisterAsync` generates token and calls `SendEmailVerificationAsync`). A Razor Page at `/account/verify-email` handles the confirmation link. However, the frontend grants full access immediately — `User.IsActive = true` on creation, no checks on `EmailConfirmed`.

### Proposed approach

Soft nudge model: users get full access but see a persistent banner prompting verification, with a resend option.

### Backend changes

**1. Add verification status to profile response**

- `UserProfileResponse`: add `isEmailVerified: bool` (maps to `User.EmailConfirmed` from Identity)

**2. Add resend verification endpoint**

```
POST /api/auth/v1/resend-verification
```

- Requires authentication
- Rate-limited: max 3 requests per hour per user
- Generates a new confirmation token and sends email
- Returns 200 OK with message

**3. Implementation in `AuthService`**

```csharp
public async Task ResendVerificationEmailAsync(Guid userId, CancellationToken ct)
{
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null) throw new NotFoundException("User");
    if (user.EmailConfirmed) throw new ValidationException("Email", "Email is already verified");

    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    await _emailService.SendEmailVerificationAsync(user.Email!, token, ct);
}
```

### Frontend changes

**1. Verification banner component**

Create `shared/components/verification-banner/verification-banner.component.ts`:
- Shown at top of feed, profile, and other main pages when `!isEmailVerified`
- Dismissible per session (stored in a signal, not persisted)
- Contains: warning icon, "Please verify your email" text, "Resend" button
- "Resend" calls `POST /api/auth/v1/resend-verification` and shows success toast
- Cooldown timer (60s) after resend to prevent spam clicks

**2. Profile indicator**

- Show a small "unverified" badge next to email on profile-view page
- Link to resend from profile settings

| File | Change |
|------|--------|
| `shared/components/verification-banner/` | New component (banner + resend logic) |
| `features/feed/feed-home/feed-home.page.html` | Add banner at top of content |
| `features/profile/profile-view/profile-view.page.html` | Add unverified indicator |
| `core/services/auth.service.ts` | Add `resendVerification()` method, expose `isEmailVerified` signal |
| `core/services/profile.service.ts` | Surface `isEmailVerified` from profile response |

### Complexity: Low-Medium

---

## TICKET 5 — Add "Brands" to sidebar navigation

**Current state:** Sidebar drawer menu (`app.component.html`) has: Feed, Explore, Saved, Shop (common section) plus role-specific sections. A `/shop` route exists already. There is no "Brands" page or route.

### Proposed approach

Add a "Brands" directory page and link it from the sidebar.

### Backend changes

**1. Brands listing endpoint** (if not already present)

Check if `GET /api/retailers/v1/` returns a public-facing list of retailers/brands. If not, add:

```
GET /api/brands/v1/
```

- Public or auth-required
- Returns paginated list: `{ id, name, logoUrl, description, productCount }`
- Supports search query param for filtering
- Optionally: `GET /api/brands/v1/{id}` for brand detail page

### Frontend changes

**1. New brands page**

`features/brands/brands.page.ts`:
- Search bar at top
- Scrollable list/grid of brand cards (logo, name, product count)
- Tapping a brand navigates to a filtered shop/feed view (`/feed?retailerId=X` or `/shop?brand=X`)

**2. Routing**

Add to `app.routes.ts`:
```typescript
{ path: 'brands', loadComponent: () => import('./features/brands/brands.page').then(m => m.BrandsPage), canActivate: [authGuard, onboardingGuard] }
```

**3. Sidebar entry**

Add between "Saved" and "Shop" in `app.component.html`:
```html
<ion-item routerLink="/brands" routerLinkActive="selected">
  <ion-icon name="pricetags-outline" slot="start"></ion-icon>
  <ion-label>Brands</ion-label>
</ion-item>
```

| File | Change |
|------|--------|
| `frontend/src/app/app.component.html` | Add "Brands" menu item |
| `frontend/src/app/app.routes.ts` | Add `/brands` route |
| `frontend/src/app/features/brands/` | New page: brands list with search |
| Backend endpoints (if needed) | Public brands listing endpoint |

### Complexity: Low-Medium

---

## TICKET 6 — Add profile photo prompt during onboarding

**Current state:** The onboarding flow is 4 steps (Height → Weight → Body Type → Fit Preference). No profile photo prompt. `User.ProfileImageUrl` exists on the entity (added 2026-02-20). A `PUT /api/profile/v1/image` endpoint likely exists or needs creation.

### Proposed approach

Add a step 0 (before measurements) or step 5 (after fit preference) prompting for a profile photo.

Recommended: **Step 5 (final step)** — after the core body profile is saved, prompt for a photo. This keeps the critical data collection first and ends on a lighter, optional note.

### Backend changes

If not already present, ensure:
```
POST /api/profile/v1/image
```
- Accepts multipart/form-data with image file
- Uploads to blob storage (Azure Blob / S3)
- Updates `User.ProfileImageUrl`
- Returns the new URL

### Frontend changes

**1. Add step 5 to onboarding wizard**

- Camera-style circle placeholder with "+" icon
- Two action buttons: "Take Photo" (camera) and "Choose from Gallery" (photo library)
- Uses Capacitor Camera plugin (`@capacitor/camera`) for native access
- Preview shows the selected image in the circle
- "Skip for now" link at the bottom
- On upload success, proceed to feed

**2. Nudge for skippers**

- If user skips, set a flag in local storage
- On 3rd app open (or after 1 week), show a subtle prompt on the profile page: "Add a profile photo to personalize your account"

| File | Change |
|------|--------|
| `features/onboarding/onboarding.page.html` | Add step 5 photo upload UI |
| `features/onboarding/onboarding.page.ts` | Add photo step logic, Camera plugin integration |
| `core/services/profile.service.ts` | Add `uploadProfileImage()` method |
| `features/profile/profile-view/profile-view.page.html` | Add nudge banner for missing photo |

### Complexity: Medium (camera plugin integration, upload handling)

---

## TICKET 7 — Improve "Become a Creator" pathway visibility

**Current state:** A "Become a Creator" link exists in the profile-view page as a list item (icon: `star-outline`, routes to `/creator/register`). Only shown if user is not already Creator/Retailer/Admin. No analytics tracking. Not surfaced during onboarding.

### Proposed approach

**Phase 1 — Improve visibility**

1. **Onboarding completion screen**
   - After the final onboarding step, show a "Get Started" screen with two paths:
     - "Start Browsing" → `/feed`
     - "Become a Creator" → `/creator/register` (with brief value prop: "Share outfits, earn commissions")

2. **Feed prompt card**
   - Show a dismissible card in the feed (after the 3rd post) on first 3 feed visits:
     - "Love sharing outfits? Become a Creator and earn from your style."
     - CTA button → `/creator/register`
   - Dismissed state stored in local storage

3. **Profile CTA enhancement**
   - Make the existing profile list item more prominent: add a highlight background color (accent/tertiary) and a brief subtitle ("Share outfits & earn")

4. **Analytics events**
   - Track `creator_cta_clicked` with source property: `onboarding | feed_card | profile`
   - Track `creator_cta_dismissed` for the feed card

| File | Change |
|------|--------|
| `features/onboarding/onboarding.page.*` | Add completion screen with creator CTA |
| `features/feed/feed-home/feed-home.page.*` | Add dismissible creator promo card |
| `features/profile/profile-view/profile-view.page.*` | Enhance CTA styling and subtitle |
| `core/services/analytics.service.ts` | Add creator CTA event tracking |

### Complexity: Low

---

## TICKET 8 — Add "Liked Posts" section to user profile

**Current state:** Likes are stored in the `Engagements` table with `Type = Like`. A `GET /api/posts/v1/saved` endpoint exists for saved posts. No equivalent endpoint for liked posts. The profile-view page has no liked posts section.

### Proposed approach

Mirror the saved-posts pattern for liked posts.

### Backend changes

**1. New endpoint**

```
GET /api/posts/v1/liked?page=1&pageSize=20
```

Implementation in `FeedService` / `FeedEndpoints`:
```csharp
// Query engagements where Type == Like for current user, join with Posts
var likedPostIds = dbContext.Engagements
    .Where(e => e.UserId == userId && e.Type == EngagementType.Like)
    .OrderByDescending(e => e.CreatedAt)
    .Select(e => e.PostId);

// Then load full post data with the same projection as feed posts
```

Returns `PagedResponse<FeedPostResponse>` — same shape as the feed and saved endpoints.

### Frontend changes

**1. New liked-posts page**

`features/feed/liked/liked.page.ts`:
- Mirrors `saved.page.ts` structure
- Header: "Liked Posts"
- Grid of `<app-post-card>` components
- Empty state: "No liked posts yet" + "Posts you like will appear here."
- Infinite scroll for pagination
- Unliking a post removes it from the list immediately (via `PostEngagementService` sync)

**2. Routing**

Add to feed routes:
```typescript
{ path: 'liked', loadComponent: () => import('./liked/liked.page').then(m => m.LikedPage) }
```

**3. Profile entry point**

Add to profile-view page list, below "Body Profile":
```html
<ion-item routerLink="/feed/liked" detail>
  <ion-icon name="heart-outline" slot="start"></ion-icon>
  <ion-label>Liked Posts</ion-label>
</ion-item>
```

**4. Sidebar entry (optional)**

Add below "Saved" in the sidebar menu for quick access.

| File | Change |
|------|--------|
| Backend: `FeedEndpoints.cs` | Add `GET /api/posts/v1/liked` |
| Backend: `FeedService.cs` | Add `GetLikedPostsAsync()` method |
| `features/feed/liked/` | New page (mirrors saved page) |
| `app.routes.ts` | Add `/feed/liked` route |
| `features/profile/profile-view/profile-view.page.html` | Add "Liked Posts" list item |
| `app.component.html` | Optionally add to sidebar |

### Complexity: Low-Medium

---

## TICKET 9 — Enable navigation to creator profile from feed posts

**Current state:** The post-card component (`shared/components/post-card/`) displays creator name and avatar, but the creator row has `(click)="$event.stopPropagation()"` which explicitly prevents navigation. There is no dedicated public creator profile page — only the authenticated creator's own dashboard.

### Proposed approach

**1. Public creator profile page**

Create a new page that any user can view:

`features/profile/creator-profile/creator-profile.page.ts`:
- Header: creator display name
- Profile section: avatar, display name, verification badge, bio, social links
- Anonymized body profile (height range, body type — not exact measurements)
- Post grid: all published posts by this creator
- "Filter feed by this creator" shortcut

**2. Backend endpoint**

If not already present:
```
GET /api/creators/v1/{id}/public-profile
```

Returns:
```json
{
  "id": "guid",
  "displayName": "string",
  "bio": "string",
  "profileImageUrl": "string",
  "isVerified": true,
  "socialLinks": {},
  "bodyProfile": { "heightRange": "160-170cm", "bodyTypeName": "Hourglass" },
  "postCount": 42,
  "followerCount": 0
}
```

And ensure:
```
GET /api/feed/v1/?creatorId={id}
```
Already exists — the feed-home page supports `?creatorId` filtering.

**3. Make creator row tappable in post-card**

- Remove `(click)="$event.stopPropagation()"` from the creator info row
- Add `(click)="navigateToCreator($event)"` which:
  - Calls `$event.stopPropagation()` (still prevent card navigation)
  - Navigates to `/profile/creator/{creatorId}`

**4. Route**

```typescript
{ path: 'creator/:id', loadComponent: () => import('./creator-profile/creator-profile.page').then(m => m.CreatorProfilePage) }
```

Under `/profile/creator/:id` (public, auth-guarded but not role-guarded).

**5. Scroll position preservation**

Ionic's `ion-router-outlet` preserves scroll position by default when navigating forward and back. Verify this works correctly; if not, use `ViewWillEnter` lifecycle to restore position from a service.

| File | Change |
|------|--------|
| `shared/components/post-card/post-card.component.html` | Make creator row tappable |
| `shared/components/post-card/post-card.component.ts` | Add `navigateToCreator()` with router |
| `features/profile/creator-profile/` | New public creator profile page |
| `app.routes.ts` | Add route |
| Backend: new or existing endpoint | Public creator profile data |

### Complexity: Medium

---

## TICKET 9.5 — Enable users to follow creators

**Current state:** No follow functionality exists anywhere — no database tables, no backend endpoints, no frontend implementation.

### Proposed approach

Full-stack implementation of a follow system.

### Backend changes

**1. New entity: `UserFollow`**

```csharp
public class UserFollow
{
    public Guid Id { get; set; }
    public Guid FollowerUserId { get; set; }   // The user doing the following
    public Guid FollowedUserId { get; set; }   // The creator being followed
    public DateTime CreatedAt { get; set; }

    public User Follower { get; set; } = null!;
    public User Followed { get; set; } = null!;
}
```

**EF Configuration:**
- Unique constraint on (FollowerUserId, FollowedUserId)
- Indexes on both columns for efficient lookups
- Cascade delete from User

**2. New endpoints**

```
POST   /api/users/v1/{id}/follow          -- Follow a user
DELETE /api/users/v1/{id}/follow          -- Unfollow a user
GET    /api/users/v1/following             -- List users I follow (paginated)
GET    /api/users/v1/{id}/followers        -- List followers of a user (paginated)
GET    /api/users/v1/{id}/follow-status    -- Check if I follow this user
```

**3. New service: `FollowService`**

- `FollowAsync(followerId, followedId)` — create follow, prevent self-follow
- `UnfollowAsync(followerId, followedId)` — remove follow
- `GetFollowingAsync(userId, page, pageSize)` — paginated list
- `GetFollowersAsync(userId, page, pageSize)` — paginated list
- `IsFollowingAsync(followerId, followedId)` — boolean check

**4. Feed integration**

- Update `FeedService` to boost/include posts from followed creators when sort = "For You"
- Add optional `followedOnly=true` query param to feed endpoint for a "Following" feed tab

**5. Update `FeedPostResponse`**

Add `isFollowing: bool` to indicate follow status on each post in the feed.

### Frontend changes

**1. Follow button on post-card**

- Add a follow/unfollow button next to the creator name in the post-card component
- Optimistic toggle with revert on error
- Emits `followed` event

**2. Follow button on creator profile page** (from Ticket 9)

- Prominent follow/unfollow button below creator name

**3. Following list page**

`features/profile/following/following.page.ts`:
- List of followed creators with avatars, names, unfollow buttons
- Accessible from profile-view page
- Empty state: "You're not following anyone yet"

**4. Feed "Following" tab**

- Add a "Following" chip/tab alongside "For You", "Recent", "Popular" on the feed-home page
- Filters to `followedOnly=true`

**5. Profile entry**

Add to profile-view:
```html
<ion-item routerLink="/profile/following" detail>
  <ion-icon name="people-outline" slot="start"></ion-icon>
  <ion-label>Following</ion-label>
</ion-item>
```

| File | Change |
|------|--------|
| Backend: `Lyke.Core/Entities/UserFollow.cs` | New entity |
| Backend: `UserFollowConfiguration.cs` | EF config |
| Backend: new migration | Add `UserFollows` table |
| Backend: `FollowService.cs` | New service |
| Backend: `FollowEndpoints.cs` or `UserEndpoints.cs` | New endpoints |
| Backend: `FeedService.cs` | Boost followed creators in relevance sort |
| `shared/components/post-card/*` | Add follow button |
| `features/profile/creator-profile/*` | Add follow button |
| `features/profile/following/` | New following list page |
| `features/feed/feed-home/*` | Add "Following" sort chip |
| `features/profile/profile-view/*` | Add "Following" list item |
| `core/services/follow.service.ts` | New frontend service |
| `app.routes.ts` | Add routes |

### Complexity: High (full-stack, new data model, feed algorithm changes)

---

## TICKET 10 — Improve profile update responsiveness (optimistic UI)

**Current state:** Profile edit page (`features/profile/profile-edit/`) and body profile edit page submit updates via PUT requests. On success, they navigate back. The profile-view page then re-fetches data from the API, causing a visible delay where old data appears momentarily.

### Proposed approach

Implement optimistic UI updates using a shared profile state signal.

**1. Create `ProfileStateService`**

```typescript
@Injectable({ providedIn: 'root' })
export class ProfileStateService {
  private _profile = signal<UserProfileResponse | null>(null);
  private _bodyProfile = signal<BodyProfileResponse | null>(null);

  readonly profile = this._profile.asReadonly();
  readonly bodyProfile = this._bodyProfile.asReadonly();

  // Called after edit page saves successfully
  updateProfile(partial: Partial<UserProfileResponse>) { ... }
  updateBodyProfile(partial: Partial<BodyProfileResponse>) { ... }

  // Called on initial load / refresh
  setProfile(profile: UserProfileResponse) { ... }
  setBodyProfile(body: BodyProfileResponse) { ... }
}
```

**2. Update edit pages**

- On successful PUT, immediately update `ProfileStateService` with the new values before navigating back
- On failure, show error toast (data was never updated locally)

**3. Update profile-view page**

- Read from `ProfileStateService` signals instead of making fresh API calls on every `ionViewWillEnter`
- Only fetch from API if state is null (first load) or on pull-to-refresh

**4. Optional: loading indicator**

- While the PUT is in-flight, show a brief spinner/skeleton on the field being updated
- Resolves to the new value on success

| File | Change |
|------|--------|
| `core/services/profile-state.service.ts` | New shared state service |
| `features/profile/profile-view/profile-view.page.ts` | Read from state service |
| `features/profile/profile-edit/profile-edit.page.ts` | Update state on save |
| `features/profile/body-profile-edit/body-profile-edit.page.ts` | Update state on save |

### Complexity: Low

---

## TICKET 11 — Review and centralize accent colour

**Current state:** The accent was previously orange and has already been changed to mustard (#D1A72E). Theme variables are centralized in `frontend/src/theme/variables.scss` with a custom `.ion-color-accent` class in `global.scss`. The primary color is midnight green (#0E3A3B).

### Proposed approach

Since the color has already been changed, this ticket is largely about verification and documentation.

**1. Audit**

- Grep the codebase for any hardcoded orange hex values (`#FF`, `#E8`, `#F5` prefixed warm tones) that may have been missed during the change
- Verify all accent usages go through the CSS variable `--ion-color-accent` or `var(--ion-color-tertiary)`, not hardcoded values

**2. Centralization check**

Confirm these are defined in `variables.scss`:
- `--ion-color-tertiary` = `#D1A72E` (the mustard accent)
- `--ion-color-tertiary-rgb`
- `--ion-color-tertiary-contrast`
- `--ion-color-tertiary-shade`
- `--ion-color-tertiary-tint`

**3. Document the design decision**

Add a comment block in `variables.scss`:
```scss
// Brand Accent: Mustard (#D1A72E)
// Decision: Changed from orange (2026-02) to better align with brand identity.
// Use --ion-color-tertiary or .ion-color-accent for all accent-colored elements.
```

**4. Refactor any stragglers**

If any component SCSS files contain hardcoded color values instead of CSS variables, replace them with variable references.

| File | Change |
|------|--------|
| `frontend/src/theme/variables.scss` | Add documentation comment, verify all tokens |
| Various component `.scss` files | Replace hardcoded colors with variables (if found) |

### Complexity: Low

---

## Implementation Priority & Sequencing

### Phase 1 — Foundation (do first, other tickets depend on these)
| Ticket | Title | Complexity | Reason |
|--------|-------|-----------|--------|
| **2** | Body type taxonomy rework | High | Ticket 1 guidance depends on final taxonomy |
| **3** | Multi-select fit preferences | Medium | Pairs with Ticket 2 migration |

### Phase 2 — Core engagement features
| Ticket | Title | Complexity | Reason |
|--------|-------|-----------|--------|
| **9** | Creator profile navigation | Medium | Prerequisite for Ticket 9.5 |
| **9.5** | Follow creators | High | Depends on Ticket 9 (creator profile page) |
| **8** | Liked posts | Low-Medium | Independent, quick win |

### Phase 3 — Onboarding & UX polish
| Ticket | Title | Complexity | Reason |
|--------|-------|-----------|--------|
| **1** | Measurement guidance | Low-Medium | Depends on Ticket 2 (final body types) |
| **6** | Profile photo prompt | Medium | Independent onboarding improvement |
| **7** | Creator pathway visibility | Low | Independent UX improvement |
| **4** | Email verification nudge | Low-Medium | Independent auth improvement |

### Phase 4 — Polish
| Ticket | Title | Complexity | Reason |
|--------|-------|-----------|--------|
| **10** | Optimistic profile updates | Low | Independent quality-of-life fix |
| **5** | Brands in sidebar | Low-Medium | Independent navigation addition |
| **11** | Accent colour audit | Low | Verification task, mostly done |
