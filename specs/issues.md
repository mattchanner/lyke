-------------------------------------------------------------------------
# TICKET 1
Title: Improve body measurement & body type guidance during signup
Type: Story
Priority: High
Component: Onboarding / UX
Description:
 The measurements and body type step in the signup flow lacks guidance. Users may not know how to determine their body shape or how to measure accurately. This creates friction and risks incorrect data capture.
We should add contextual help (tooltips, diagrams, or a short explainer) and explore implementing a quick hosted quiz (e.g. Kibbe-style) to recommend a body type. The quiz could also double as a social media engagement hook.
Acceptance Criteria:
The body measurement step includes clear “How to measure” guidance.
The body type step includes clear explanations and examples.

If quiz is implemented:
User completes quiz and receives a recommended body type.
Result auto-populates body type field but remains editable.
Guidance is accessible without leaving the signup flow.

-------------------------------------------------------------------------
# TICKET 2
Title: Rework body type taxonomy to allow more accurate representation
Type: Story
Priority: High
Component: Profile / Data Model
Description:
 The current body type list does not accurately reflect real-world combinations (e.g. users can be both petite and hourglass). The taxonomy needs reconsideration.
Options to explore:
Move to a single, consistent framework (e.g. Kibbe body types).


Support multi-attribute selection (e.g. shape + height proportion).


Acceptance Criteria:
Body type data model supports chosen taxonomy (single framework or multi-axis traits).


UI reflects updated structure clearly.


Existing users’ body types are migrated safely (if applicable).


No logical conflicts in selection.

-------------------------------------------------------------------------

# TICKET 3
Title: Enable multi-select for fit preferences
 Type: Bug / Enhancement
 Priority: High
 Component: Onboarding / Profile
Description:
 Fit preferences currently appear to be single-select. Users should be able to select multiple fit options.
Acceptance Criteria:
Fit preference UI supports multiple selections.

Selected states are visually clear.

Saved profile stores multiple values correctly.

Feed/recommendations logic supports multi-value preferences.

-------------------------------------------------------------------------
# TICKET 4
Title: Implement or confirm email verification flow
 Type: Story
 Priority: Medium
 Component: Authentication
Description:
 Signup completed without triggering email verification. Confirm whether this is intentional. If not, implement standard verification.
Acceptance Criteria (if verification required):
Verification email is sent upon registration.


User must verify before full access (or restricted access model defined).


“Resend verification” option available.


Clear UI messaging around verification status.


-------------------------------------------------------------------------
# TICKET 5
Title: Add “Brands” to bottom tab navigation
 Type: Story
 Priority: Medium
 Component: Navigation
Description:
 Add a “Brands” tab to the bottom navigation bar to improve discoverability.
Acceptance Criteria:
“Brands” tab visible in bottom nav.

Tab links to brands directory screen.

Navigation state persists when switching tabs.

No layout regressions on iPhone and Android.

-------------------------------------------------------------------------
# TICKET 6
Title: Add profile photo prompt during onboarding
 Type: Story
 Priority: Medium
 Component: Onboarding / Profile
Description:
 Users are not prompted to upload a profile photo during setup, resulting in default placeholder avatars.
Acceptance Criteria:
User is prompted to upload profile photo during onboarding.


Photo upload supports camera and gallery.


Skip option exists (if required), but user is gently nudged later.


Profile photo updates immediately across app.


-------------------------------------------------------------------------
# TICKET 7
Title: Improve visibility of “Become a Creator” pathway
 Type: Story
 Priority: Medium
 Component: Creator Flow / UX
Description:
 The “Become a Creator” pathway is not sufficiently prominent. Users may not realise this is an option.
Potential longer-term consideration:
 Automatically treat users as creators by default, with a separate “programme” or verified tier (similar to a blue tick) for retailer eligibility.
Acceptance Criteria (Phase 1 – visibility improvement):
“Become a Creator” CTA visible on profile screen.


CTA surfaced in onboarding completion screen or menu.


Analytics event added to track clicks on CTA.


(Product model revision to be discussed separately.)

-------------------------------------------------------------------------
# TICKET 8
Title: Add “Liked Posts” section to user profile
 Type: Story
 Priority: High
 Component: Engagement / Profile
Description:
 Users cannot easily find content they have liked.
Acceptance Criteria:
“Liked Posts” accessible from profile (max 2 taps).


Liked content list loads correctly.


Unliking removes item from list immediately.


Empty state shown if no likes.


-------------------------------------------------------------------------
# TICKET 9
Title: Enable navigation to creator profile from feed posts
 Type: Bug / Missing Feature
 Priority: High
 Component: Feed / Navigation
Description:
 Users cannot tap a creator’s name or avatar in the feed to view their full profile and posts.
Acceptance Criteria:
Tapping username or avatar opens creator profile page.


Creator profile shows grid/list of posts.


Back navigation returns to previous feed scroll position.

-------------------------------------------------------------------------
# TICKET 9.5
Title: Enable users to follow creator profile from feed posts
 Type: Bug / Missing Feature
 Priority: High
 Component: Feed / Navigation
Description:
 Users cannot follow a creator in the feed to add them to creators tab or influence what is shown on feed.
Acceptance Criteria:
Ability to follow a creator from feed post.


Able to review follows and unfollow from list or creator profile.


-------------------------------------------------------------------------
# TICKET 10
Title: Improve profile update responsiveness (optimistic UI update)
 Type: Bug
 Priority: Medium
 Component: Profile
Description:
 After editing profile details, updates take time to reflect on the previous screen, creating the impression that the change failed.
Acceptance Criteria:
Profile updates use optimistic UI update OR display clear loading state.


Returning to profile shows updated data immediately.


If API fails, user receives error message and data reverts appropriately.


-------------------------------------------------------------------------
# TICKET 11
Title: Review and potentially revise UI accent colour (orange)
 Type: Task
 Priority: Low
 Component: Design System
Description:
 Current orange accent colour may not align with desired brand direction. Needs design review.
Acceptance Criteria:
Accent colour token centralised in theme variables.


Colour can be updated without large refactor.


Design review completed and decision documented.

