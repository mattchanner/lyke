# LYKE Mobile Application - Implementation Plan

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend API | C# ASP.NET Core 10.0 |
| ORM | Entity Framework Core 9.0 |
| Database | PostgreSQL 16 |
| Frontend Framework | Ionic 7 + Angular 17 |
| UI Components | Angular Material 17 |
| State Management | NgRx Signals |
| Authentication | ASP.NET Core Identity + JWT |
| File Storage | Azure Blob Storage / AWS S3 |
| Search | PostgreSQL Full-Text Search (MVP) |
| Caching | Redis |
| Analytics | Application Insights / Custom Events |
| Native Features | Capacitor 5 |
| Testing | Jest + Cypress |

---

## Current Progress Summary

**Last Updated:** 2026-02-05

### Backend Status

| Component | Status |
|-----------|--------|
| Backend Foundation | ✅ Complete |
| Database Schema & Migrations | ✅ Complete |
| Entity Configurations | ✅ Complete (15 entities) |
| Repository Pattern | ✅ Complete |
| Exception Handling | ✅ Complete |
| JWT Infrastructure | ✅ Complete |
| Auth Service & Endpoints | ✅ Complete (7 endpoints) |
| Profile Service & Endpoints | ✅ Complete (8 endpoints) |
| Feed Service & Endpoints | ✅ Complete (8 endpoints) |
| Commerce Service & Endpoints | ✅ Complete (6 endpoints) |
| Creator Service & Endpoints | ✅ Complete (15 endpoints) |
| Media Upload Service | ✅ Complete (4 endpoints) |
| Creator Verification Workflow | ✅ Complete (5 endpoints) |
| Admin Service & Endpoints | ✅ Complete (11 endpoints) |
| FluentValidation | ✅ Complete (20 validators) |
| Seed Data (Lookups) | ✅ Complete (8 body types, 12 fit tags) |
| Role-Based Authorization | ✅ Complete (5 policies) |
| Unit Tests | ✅ Complete (5 service test suites) |
| Integration Tests | ✅ Complete (7 endpoint test suites) |

### Frontend Status

| Component | Status | Tasks |
|-----------|--------|-------|
| Project Setup & Configuration | ❌ Not Started | 42 |
| Shared Components Library | ❌ Not Started | 65 |
| Auth Module | ❌ Not Started | 48 |
| Profile & Onboarding Module | ❌ Not Started | 45 |
| Feed & Discovery Module | ❌ Not Started | 40 |
| Commerce Module | ❌ Not Started | 28 |
| Creator Module | ❌ Not Started | 95 |
| Settings & Privacy Module | ❌ Not Started | 52 |
| Frontend Testing | ❌ Not Started | 42 |

**Backend Progress: ~68% complete (93/136 tasks)**
**Frontend Progress: 0% complete (0/504 tasks)**
**Overall Project Progress: ~15% (97/650 tasks)**

---

## Phase 1: Project Foundation & Infrastructure

### 1.1 Backend Project Setup
- [x] Create ASP.NET Core Web API solution structure
  - `Lyke.Api` - Web API project
  - `Lyke.Core` - Domain entities and interfaces
  - `Lyke.Infrastructure` - EF Core, repositories, external services
  - `Lyke.Application` - Business logic, DTOs, services
  - `Lyke.UnitTests` - Unit test project
  - `Lyke.IntegrationTests` - Integration test project
  - `Lyke.AppHost` - .NET Aspire host project
  - `Lyke.ServiceDefaults` - Aspire service defaults
- [x] Configure PostgreSQL connection and EF Core
- [x] Set up dependency injection container
- [x] Configure logging (Serilog)
- [x] Set up environment-based configuration (appsettings.json)
- [x] Add health check endpoints
- [x] Configure CORS for mobile app
- [x] Configure .NET Aspire for cloud-native development

### 1.2 Frontend Project Setup

#### 1.2.1 Project Initialization
- [ ] Initialize Ionic Angular project (`ionic start lyke-app blank --type=angular --capacitor`)
- [ ] Configure Angular 17 with standalone components
- [ ] Install and configure Angular Material 17
  - [ ] Add `@angular/material` and `@angular/cdk` packages
  - [ ] Create custom Material theme (LYKE brand colors)
  - [ ] Configure Material typography
  - [ ] Set up dark mode theme variant
- [ ] Install additional dependencies
  - [ ] `@ngrx/signals` for state management
  - [ ] `@capacitor/camera`, `@capacitor/filesystem`, `@capacitor/share`
  - [ ] `@capacitor/preferences` for local storage
  - [ ] `swiper` for media carousels
  - [ ] `chart.js` with `ng2-charts` for analytics
  - [ ] `date-fns` for date manipulation

#### 1.2.2 Project Structure
- [ ] Configure project folder structure:
  ```
  /src/app
    /core
      /services          - API services, auth, storage
      /guards            - Route guards (auth, role-based)
      /interceptors      - HTTP interceptors
      /state             - NgRx signal stores
      /utils             - Helper functions
    /shared
      /components        - Reusable UI components
      /directives        - Custom directives
      /pipes             - Custom pipes
      /layouts           - Page layouts
      /validators        - Form validators
    /features
      /auth              - Login, register, password reset
      /onboarding        - Body profile setup wizard
      /feed              - Main feed, explore, search
      /post              - Post detail, engagement
      /profile           - User profile, settings
      /creator           - Creator dashboard, post creation
      /commerce          - Product views, shopping
      /settings          - App settings, privacy
    /models              - TypeScript interfaces/types
  ```

#### 1.2.3 Configuration
- [ ] Set up environment configuration
  - [ ] `environment.ts` (development)
  - [ ] `environment.staging.ts` (staging)
  - [ ] `environment.prod.ts` (production)
  - [ ] Configure API base URLs per environment
  - [ ] Configure feature flags
- [ ] Configure Angular routing
  - [ ] Set up lazy loading for feature modules
  - [ ] Configure route animations
  - [ ] Set up route guards
- [ ] Configure app initialization
  - [ ] Create APP_INITIALIZER for auth state restoration
  - [ ] Configure error handling strategy
  - [ ] Set up global loading state

#### 1.2.4 Capacitor Native Configuration
- [ ] Configure Capacitor for iOS
  - [ ] Set up Xcode project settings
  - [ ] Configure app icons and splash screens
  - [ ] Set up camera and photo library permissions
  - [ ] Configure deep linking (Universal Links)
- [ ] Configure Capacitor for Android
  - [ ] Set up Android Studio project settings
  - [ ] Configure app icons and splash screens
  - [ ] Set up camera and storage permissions
  - [ ] Configure deep linking (App Links)
- [ ] Configure native plugins
  - [ ] Camera plugin for media capture
  - [ ] Share plugin for social sharing
  - [ ] Push notifications (Firebase/APNs)
  - [ ] Haptics for tactile feedback
  - [ ] Status bar customization
  - [ ] Keyboard handling

### 1.3 CI/CD Pipeline
- [ ] Set up GitHub Actions / Azure DevOps pipeline
- [ ] Configure build stages (build, test, deploy)
- [ ] Set up database migrations automation
- [ ] Configure environment deployments

---

## Phase 2: Database Design & Entity Framework Models

### 2.1 Core Entities

```
Users
├── Id (GUID)
├── Email
├── PasswordHash
├── UserType (Shopper, Creator, Retailer, Admin)
├── CreatedAt
├── UpdatedAt
└── IsActive

BodyProfiles
├── Id (GUID)
├── UserId (FK)
├── HeightCm (int)
├── WeightKg (decimal)
├── BodyTypeId (FK)
├── FitPreference (enum: Fitted, Regular, Relaxed)
├── CreatedAt
└── UpdatedAt

BodyTypes (lookup)
├── Id
├── Name
├── Description
└── DisplayOrder

Creators
├── Id (GUID)
├── UserId (FK)
├── DisplayName
├── Bio
├── IsVerified
├── SocialLinks (JSON)
├── PayoutDetails (encrypted)
└── CreatedAt

Retailers
├── Id (GUID)
├── Name
├── LogoUrl
├── WebsiteUrl
├── AffiliateConfig (JSON)
├── IsActive
└── CreatedAt

Products
├── Id (GUID)
├── RetailerId (FK)
├── ExternalSku
├── Name
├── Description
├── Category
├── SubCategory
├── ImageUrls (JSON array)
├── ProductUrl
├── Price
├── Currency
├── IsActive
├── LastSyncedAt
└── CreatedAt

Posts (Outfit Content)
├── Id (GUID)
├── CreatorId (FK)
├── Title
├── Description
├── MediaType (Image, Video)
├── MediaUrls (JSON array)
├── Status (Draft, PendingReview, Published, Rejected)
├── ModerationNotes
├── PublishedAt
├── CreatedAt
└── UpdatedAt

PostProducts (junction)
├── Id
├── PostId (FK)
├── ProductId (FK)
├── SizeWorn
├── FitNotes
├── FitRating (enum: TooSmall, SlightlySmall, TrueToSize, SlightlyLarge, TooLarge)
└── StylingNotes

PostFitTags (junction)
├── PostProductId (FK)
├── FitTagId (FK)

FitTags (lookup)
├── Id
├── Name (e.g., "Tight on hips", "Long in arms")
├── Category
└── IsActive

Engagements
├── Id (GUID)
├── UserId (FK)
├── PostId (FK)
├── Type (View, Like, Save, Share)
├── CreatedAt

ClickEvents
├── Id (GUID)
├── UserId (FK, nullable)
├── PostId (FK)
├── PostProductId (FK)
├── SessionId
├── CreatedAt
├── ConvertedAt (nullable)
└── AttributionData (JSON)

SponsoredPlacements
├── Id (GUID)
├── RetailerId (FK)
├── ProductId (FK, nullable)
├── BudgetAmount
├── SpentAmount
├── TargetBodyTypes (JSON array)
├── TargetCategories (JSON array)
├── StartDate
├── EndDate
├── IsActive
└── CreatedAt

CreatorEarnings
├── Id (GUID)
├── CreatorId (FK)
├── ClickEventId (FK)
├── EarningType (Affiliate, Sponsored)
├── Amount
├── Currency
├── Status (Pending, Confirmed, Paid)
├── PaidAt
└── CreatedAt
```

### 2.2 Database Tasks
- [x] Create EF Core DbContext with all entity configurations
- [x] Configure entity relationships and constraints
- [x] Set up soft delete for applicable entities
- [x] Create initial migration
- [x] Implement audit fields (CreatedAt, UpdatedAt) via SaveChanges override
- [x] Create database indexes for performance
  - `IX_Posts_CreatorId_Status_PublishedAt`
  - `IX_BodyProfiles_BodyTypeId_HeightCm_WeightKg`
  - `IX_Products_RetailerId_Category_IsActive`
  - `IX_ClickEvents_PostId_CreatedAt`
- [ ] Set up PostgreSQL full-text search indexes
- [x] Create seed data for lookup tables (BodyTypes, FitTags)

---

## Phase 3: Authentication & Authorization

### 3.1 Backend Auth Implementation
- [x] Configure ASP.NET Core Identity with PostgreSQL
- [x] Implement JWT token generation and validation
- [x] Create refresh token mechanism with rotation
- [x] Create AuthService with full authentication logic
- [x] Create FluentValidation validators for all auth requests
- [x] Implement role-based authorization (Shopper, Creator, Retailer, Admin)
- [x] Add policy-based authorization for granular permissions (CreatorOnly, AdminOnly, RetailerOnly, CreatorOrAdmin, RetailerOrAdmin)
- [ ] Implement account lockout and security features

### 3.2 API Endpoints - Authentication (Minimal APIs)
- [x] POST /api/auth/register - Register new user
- [x] POST /api/auth/login - Login with email/password
- [x] POST /api/auth/refresh - Refresh access token
- [x] POST /api/auth/logout - Invalidate refresh token
- [x] POST /api/auth/forgot-password - Request password reset
- [x] POST /api/auth/reset-password - Reset password with token
- [x] DELETE /api/auth/account - Delete account (GDPR)
- [ ] POST /api/auth/social/google - Google OAuth login
- [ ] POST /api/auth/social/apple - Apple Sign-In

### 3.3 Frontend Auth Implementation

#### 3.3.1 Core Auth Services
- [ ] Create AuthService
  - [ ] Login method with email/password
  - [ ] Register method with validation
  - [ ] Logout with token cleanup
  - [ ] Token refresh logic with retry
  - [ ] Password reset request/confirm
  - [ ] Account deletion request
  - [ ] Auth state observable (isAuthenticated$, currentUser$)
- [ ] Create TokenService
  - [ ] Secure token storage (Capacitor Preferences with encryption)
  - [ ] Token retrieval and validation
  - [ ] Token expiry checking
  - [ ] Refresh token rotation handling
- [ ] Create AuthStateStore (NgRx Signals)
  - [ ] User state (user, isAuthenticated, isLoading, error)
  - [ ] Computed signals (isCreator, isAdmin, hasBodyProfile)
  - [ ] Actions (login, logout, refreshToken, updateUser)

#### 3.3.2 HTTP Infrastructure
- [ ] Create AuthInterceptor
  - [ ] Inject JWT token into request headers
  - [ ] Skip auth for public endpoints
  - [ ] Handle 401 responses with token refresh
  - [ ] Queue requests during token refresh
  - [ ] Redirect to login on refresh failure
- [ ] Create ErrorInterceptor
  - [ ] Global error handling
  - [ ] Error message extraction
  - [ ] Network error detection
  - [ ] Retry logic for transient failures
- [ ] Create LoadingInterceptor
  - [ ] Track pending requests
  - [ ] Expose loading state

#### 3.3.3 Route Guards
- [ ] Create AuthGuard
  - [ ] Protect authenticated routes
  - [ ] Redirect to login if unauthenticated
  - [ ] Store intended destination for post-login redirect
- [ ] Create GuestGuard
  - [ ] Prevent authenticated users from accessing login/register
  - [ ] Redirect to home if authenticated
- [ ] Create RoleGuard
  - [ ] Check user role (Creator, Retailer, Admin)
  - [ ] Redirect to appropriate page if unauthorized
- [ ] Create OnboardingGuard
  - [ ] Check if body profile is complete
  - [ ] Redirect to onboarding if incomplete

#### 3.3.4 Auth UI Components
- [ ] Create LoginPage
  - [ ] Material form with email/password fields
  - [ ] Form validation with error messages
  - [ ] "Remember me" checkbox
  - [ ] "Forgot password" link
  - [ ] Social login buttons section
  - [ ] Link to registration
  - [ ] Loading state during submission
- [ ] Create RegisterPage
  - [ ] Material stepper or single form
  - [ ] Email, password, confirm password fields
  - [ ] Password strength indicator
  - [ ] Terms of service checkbox
  - [ ] Privacy policy acceptance
  - [ ] Link to login
- [ ] Create ForgotPasswordPage
  - [ ] Email input with validation
  - [ ] Success message with instructions
  - [ ] Resend email option
- [ ] Create ResetPasswordPage
  - [ ] Token validation on load
  - [ ] New password with confirmation
  - [ ] Password requirements display
  - [ ] Success redirect to login
- [ ] Create SocialLoginButtons component
  - [ ] Google Sign-In button (Capacitor Google Auth)
  - [ ] Apple Sign-In button (iOS only, Capacitor Apple Auth)
  - [ ] Loading states per provider
  - [ ] Error handling per provider

---

## Phase 4: User Profile & Body Profile

### 4.1 Backend - Profile APIs (Minimal APIs)
- [x] GET /api/profile - Get current user profile
- [x] PUT /api/profile - Update user profile
- [x] GET /api/profile/body - Get body profile
- [x] POST /api/profile/body - Create body profile
- [x] PUT /api/profile/body - Update body profile
- [x] DELETE /api/profile/body - Delete body profile
- [x] GET /api/lookup/body-types - Get body type options
- [x] GET /api/lookup/fit-preferences - Get fit preference options

### 4.2 Backend Tasks
- [x] Create ProfileService with business logic
- [x] Implement body profile validation rules (FluentValidation)
- [x] Create DTOs for profile data (never expose raw weights to other users)
- [x] Implement profile anonymization for display (ranges/bands)
- [x] Add profile completeness calculation

### 4.3 Frontend - Profile Module

#### 4.3.1 Profile Services
- [ ] Create ProfileService
  - [ ] Get current user profile
  - [ ] Update user profile
  - [ ] Get body profile
  - [ ] Create/update body profile
  - [ ] Delete body profile
  - [ ] Get profile completeness
- [ ] Create LookupService
  - [ ] Get body types (cached)
  - [ ] Get fit preferences (cached)
  - [ ] Get fit tags (cached)
- [ ] Create ProfileStore (NgRx Signals)
  - [ ] User profile state
  - [ ] Body profile state
  - [ ] Lookup data state
  - [ ] Profile completeness computed signal
- [ ] Create UnitConversionService
  - [ ] Height: cm ↔ feet/inches
  - [ ] Weight: kg ↔ lbs
  - [ ] User preference persistence

#### 4.3.2 Onboarding Flow
- [ ] Create OnboardingPage (Material Stepper)
  - [ ] Linear stepper with 5 steps
  - [ ] Progress indicator
  - [ ] Skip option for optional steps
  - [ ] Completion celebration animation
- [ ] Create WelcomeStep component
  - [ ] App value proposition
  - [ ] Privacy assurance message
  - [ ] "Get Started" CTA
- [ ] Create HeightInputStep component
  - [ ] Material slider with visual feedback
  - [ ] Unit toggle (cm / ft-in)
  - [ ] Human silhouette visualization
  - [ ] Validation (reasonable height range)
- [ ] Create WeightInputStep component
  - [ ] Material slider with visual feedback
  - [ ] Unit toggle (kg / lbs)
  - [ ] Privacy notice ("never shared exactly")
  - [ ] Validation (reasonable weight range)
- [ ] Create BodyTypeStep component
  - [ ] Visual card grid with body type illustrations
  - [ ] Material card selection (single select)
  - [ ] Body type descriptions on tap
  - [ ] "Not sure" helper option
- [ ] Create FitPreferenceStep component
  - [ ] Material chip selection (Fitted, Regular, Relaxed)
  - [ ] Visual examples of each fit
  - [ ] Optional step indicator
- [ ] Create OnboardingCompleteStep component
  - [ ] Success animation (Lottie or CSS)
  - [ ] Profile summary card
  - [ ] "Start Exploring" CTA

#### 4.3.3 Profile Management UI
- [ ] Create ProfilePage
  - [ ] Profile header with avatar
  - [ ] User info section (name, email)
  - [ ] Body profile summary card
  - [ ] Profile completeness indicator
  - [ ] Edit buttons per section
  - [ ] Settings navigation
- [ ] Create EditProfileModal
  - [ ] Material bottom sheet or dialog
  - [ ] Name, display name fields
  - [ ] Avatar upload/change
  - [ ] Save/cancel actions
- [ ] Create BodyProfileCard component
  - [ ] Anonymized display (height range, body type)
  - [ ] Fit preference badge
  - [ ] Edit button
  - [ ] Privacy indicator
- [ ] Create EditBodyProfilePage
  - [ ] Reuse onboarding step components
  - [ ] Pre-filled with current values
  - [ ] Delete body profile option
  - [ ] Confirmation dialogs for destructive actions
- [ ] Create AvatarUploadComponent
  - [ ] Camera capture option
  - [ ] Gallery selection
  - [ ] Image cropping (circular)
  - [ ] Upload progress indicator

---

## Phase 5: Content Feed & Discovery

### 5.1 Matching Algorithm Service
- [x] Create BodyProfileMatchingService (integrated in FeedService)
  - Calculate similarity score between profiles
  - Weight factors: height (30%), weight (30%), body type (40%)
  - Configurable via appsettings (MatchingSettings)
- [x] Implement feed ranking algorithm
  - Primary: Body profile similarity
  - Secondary: Recency score
  - Configurable weights and tolerances
- [ ] Create caching layer for computed matches (Redis - future)
- [ ] Implement A/B testing hooks for algorithm tuning

### 5.2 Backend - Feed APIs (Minimal APIs)
- [x] GET /api/feed - Get personalized feed (paginated, filtered, sorted)
- [x] GET /api/feed/explore - Explore/discover content (trending)
- [x] GET /api/posts/{id} - Get post detail
- [x] GET /api/posts/{id}/similar - Get similar posts
- [x] POST /api/posts/{id}/engage - Record engagement (view, like, save, share)
- [x] DELETE /api/posts/{id}/engage - Remove engagement (unlike, unsave)
- [x] GET /api/posts/saved - Get user's saved posts
- [x] GET /api/search - Search posts, products, creators

### 5.3 Backend Tasks
- [x] Create FeedService with pagination
- [x] Implement feed query with EF Core
- [ ] Add Redis caching for hot content
- [x] Create filtering and sorting logic (category, retailer, fit tags)
- [x] Implement pagination support
- [ ] Add content pre-fetching hints

### 5.3.5 Frontend - Shared Components Library

Before building feature modules, establish a shared component library used across the app.

#### Material Theme & Styling
- [ ] Create custom Material theme
  - [ ] Define primary, accent, warn color palettes (LYKE branding)
  - [ ] Configure typography scale
  - [ ] Create dark mode theme variant
  - [ ] Configure component density
- [ ] Create global SCSS utilities
  - [ ] Spacing utilities (margins, padding)
  - [ ] Flexbox/grid helpers
  - [ ] Typography classes
  - [ ] Animation mixins

#### Layout Components
- [ ] Create PageLayout component
  - [ ] Standard page structure with header slot
  - [ ] Content area with safe area insets
  - [ ] Optional FAB slot
- [ ] Create TabLayout component
  - [ ] Bottom tab navigation (Material tabs)
  - [ ] Tab icons and labels
  - [ ] Badge indicators
  - [ ] Tab change animations
- [ ] Create PageHeader component
  - [ ] Title with optional back button
  - [ ] Action buttons slot
  - [ ] Search bar integration option
  - [ ] Scroll-aware hide/show behavior

#### Feedback Components
- [ ] Create LoadingSpinner component
  - [ ] Material progress spinner
  - [ ] Full-page overlay option
  - [ ] Inline option
  - [ ] Custom message
- [ ] Create EmptyState component
  - [ ] Illustration/icon
  - [ ] Title and description
  - [ ] Optional action button
  - [ ] Variants for different contexts (no posts, no results, etc.)
- [ ] Create ErrorState component
  - [ ] Error icon
  - [ ] Error message
  - [ ] Retry button
  - [ ] Technical details (collapsible, dev only)
- [ ] Create ToastService
  - [ ] Success, error, warning, info variants
  - [ ] Auto-dismiss with configurable duration
  - [ ] Action button option
  - [ ] Queue management

#### Form Components
- [ ] Create FormField wrapper component
  - [ ] Material form field styling
  - [ ] Label, hint, error message slots
  - [ ] Required indicator
- [ ] Create PasswordInput component
  - [ ] Show/hide toggle
  - [ ] Strength indicator
  - [ ] Requirements checklist
- [ ] Create SearchInput component
  - [ ] Debounced input
  - [ ] Clear button
  - [ ] Loading indicator
  - [ ] Recent searches dropdown

#### Media Components
- [ ] Create MediaCarousel component (Swiper)
  - [ ] Image and video support
  - [ ] Lazy loading
  - [ ] Page indicators
  - [ ] Pinch-to-zoom
  - [ ] Full-screen mode
- [ ] Create LazyImage component
  - [ ] Placeholder while loading
  - [ ] Error fallback
  - [ ] Blur-up effect
  - [ ] Aspect ratio preservation
- [ ] Create VideoPlayer component
  - [ ] Autoplay when visible
  - [ ] Mute/unmute toggle
  - [ ] Play/pause on tap
  - [ ] Progress indicator
  - [ ] Fullscreen support
- [ ] Create AvatarComponent
  - [ ] Image or initials fallback
  - [ ] Size variants (xs, sm, md, lg)
  - [ ] Online/verified badge overlay

#### Data Display Components
- [ ] Create Badge component
  - [ ] Variants (primary, success, warning, error, neutral)
  - [ ] Size options
  - [ ] Icon support
- [ ] Create Chip component (Material chip wrapper)
  - [ ] Selectable chips
  - [ ] Removable chips
  - [ ] Chip groups
- [ ] Create StatsCard component
  - [ ] Icon, value, label
  - [ ] Trend indicator (up/down/neutral)
  - [ ] Tap action
- [ ] Create PriceDisplay component
  - [ ] Currency formatting
  - [ ] Sale price with original strikethrough
  - [ ] Price range support

#### Action Components
- [ ] Create ConfirmationDialog component
  - [ ] Title, message, icon
  - [ ] Confirm/cancel buttons
  - [ ] Destructive action styling
- [ ] Create ActionSheet component
  - [ ] List of actions
  - [ ] Icons and descriptions
  - [ ] Destructive action highlighting
  - [ ] Cancel button
- [ ] Create FloatingActionButton component
  - [ ] Material FAB styling
  - [ ] Mini variant
  - [ ] Extended variant with label
  - [ ] Speed dial option

#### Utility Pipes
- [ ] Create RelativeTimePipe
  - [ ] "2 hours ago", "yesterday", "Mar 5"
  - [ ] Configurable thresholds
- [ ] Create TruncatePipe
  - [ ] Character limit with ellipsis
  - [ ] Word boundary awareness
- [ ] Create UnitFormatPipe
  - [ ] Height (cm or ft/in based on preference)
  - [ ] Weight (kg or lbs based on preference)
- [ ] Create PluralPipe
  - [ ] "1 like" vs "2 likes"
  - [ ] Custom plural forms

#### Utility Directives
- [ ] Create LazyLoadDirective
  - [ ] Intersection Observer based
  - [ ] Placeholder until visible
- [ ] Create HapticDirective
  - [ ] Trigger haptic feedback on tap
  - [ ] Configurable haptic type
- [ ] Create LongPressDirective
  - [ ] Configurable duration
  - [ ] Event emission
- [ ] Create AutoFocusDirective
  - [ ] Focus input on view enter

### 5.4 Frontend - Feed Module

#### 5.4.1 Feed Services
- [ ] Create FeedService
  - [ ] Get personalized feed (paginated)
  - [ ] Get explore/trending feed
  - [ ] Get similar posts
  - [ ] Search posts, products, creators
  - [ ] Pagination state management
- [ ] Create EngagementService
  - [ ] Record engagement (view, like, save, share)
  - [ ] Remove engagement (unlike, unsave)
  - [ ] Get saved posts
  - [ ] Optimistic updates for instant feedback
- [ ] Create FeedStore (NgRx Signals)
  - [ ] Feed posts state with pagination
  - [ ] Explore posts state
  - [ ] Saved posts state
  - [ ] Active filters state
  - [ ] Search results state
  - [ ] Loading/error states

#### 5.4.2 Feed Pages
- [ ] Create FeedPage (Tab: "For You")
  - [ ] Ionic infinite scroll with virtual scrolling
  - [ ] Pull-to-refresh (ion-refresher)
  - [ ] Filter button in header
  - [ ] Empty state for new users
  - [ ] Error state with retry
- [ ] Create ExplorePage (Tab: "Explore")
  - [ ] Trending content grid/list toggle
  - [ ] Category quick filters (horizontal scroll)
  - [ ] Search bar integration
  - [ ] Featured creators section
- [ ] Create SearchPage
  - [ ] Material search input with debounce
  - [ ] Tab segments: Posts, Products, Creators
  - [ ] Recent searches (local storage)
  - [ ] Search suggestions
  - [ ] Empty/no results states
- [ ] Create SavedPostsPage
  - [ ] Grid view of saved posts
  - [ ] Remove from saved action
  - [ ] Empty state with CTA to explore

#### 5.4.3 Post Card Component
- [ ] Create PostCard component
  - [ ] Creator header (avatar, anonymized stats, follow)
  - [ ] Match percentage badge (similarity score)
  - [ ] Media carousel (Swiper)
    - [ ] Image lazy loading
    - [ ] Video autoplay on visible (muted)
    - [ ] Pinch-to-zoom support
    - [ ] Page indicators
  - [ ] Product tags overlay (tappable hotspots)
  - [ ] Engagement bar (like, save, share, comment count)
  - [ ] Fit summary chips
  - [ ] "View Details" tap area
- [ ] Create PostCardSkeleton component
  - [ ] Animated placeholder matching card layout
  - [ ] Shimmer effect

#### 5.4.4 Post Detail Page
- [ ] Create PostDetailPage
  - [ ] Full-screen media viewer
    - [ ] Swipe between images/videos
    - [ ] Double-tap to zoom
    - [ ] Video controls (play/pause, mute, fullscreen)
  - [ ] Creator profile card (anonymized body stats)
  - [ ] Match score explanation
  - [ ] Tagged products list
    - [ ] Product cards with fit feedback
    - [ ] Size worn, fit rating
    - [ ] "Shop Now" buttons
  - [ ] Fit notes and styling tips
  - [ ] Similar posts carousel
  - [ ] Engagement actions (like, save, share)
  - [ ] Report post option

#### 5.4.5 Filter Components
- [ ] Create FilterDrawer component (Material bottom sheet)
  - [ ] Category filter (Material chips, multi-select)
  - [ ] Retailer filter (searchable autocomplete)
  - [ ] Size filter (XS-XXL chips)
  - [ ] Fit tags filter (grouped chips)
  - [ ] Body type filter (for explore)
  - [ ] Price range slider
  - [ ] Clear all / Apply buttons
  - [ ] Active filter count badge
- [ ] Create SortOptions component
  - [ ] Material select or action sheet
  - [ ] Options: Relevance, Newest, Most Liked
- [ ] Create ActiveFiltersBar component
  - [ ] Horizontal scroll of active filter chips
  - [ ] Remove individual filters
  - [ ] Clear all option

#### 5.4.6 Engagement Components
- [ ] Create LikeButton component
  - [ ] Heart icon with animation
  - [ ] Optimistic toggle
  - [ ] Like count display
  - [ ] Haptic feedback
- [ ] Create SaveButton component
  - [ ] Bookmark icon with animation
  - [ ] Optimistic toggle
  - [ ] Haptic feedback
- [ ] Create ShareButton component
  - [ ] Native share sheet (Capacitor Share)
  - [ ] Copy link fallback
  - [ ] Share tracking
- [ ] Create EngagementBar component
  - [ ] Combines like, save, share, comment buttons
  - [ ] View count display

---

## Phase 6: Product Linking & Commerce

### 6.1 Backend - Commerce APIs
```
POST   /api/clicks/track           - Track outbound click ✅
GET    /api/products/{id}          - Get product details ✅
GET    /api/products/search        - Search products (for creators) ✅
GET    /api/retailers              - List active retailers ✅
GET    /api/retailers/{id}/products - Get retailer products ✅
POST   /api/retailers/{id}/conversions - Conversion webhook ✅
```

### 6.2 Backend Tasks
- [x] Create CommerceService
  - [x] Generate unique click IDs
  - [x] Store attribution data (JSON in ClickEvent.AttributionData)
  - [x] Handle affiliate link generation
  - [x] Click deduplication for accuracy
  - [x] IP address hashing for privacy
- [x] Implement affiliate URL builder per retailer (uses Retailer.AffiliateConfig JSON)
- [x] Create conversion webhook endpoints (with HMAC signature verification)
- [x] Product search with filtering (name, SKU, retailer, category)
- [ ] Build click analytics aggregation (deferred to Phase 10)

### 6.3 Frontend - Commerce Module

#### 6.3.1 Commerce Services
- [ ] Create CommerceService
  - [ ] Track product click (before redirect)
  - [ ] Get product details
  - [ ] Search products
  - [ ] Get retailers list
  - [ ] Get retailer products
- [ ] Create ClickTrackingService
  - [ ] Generate session ID
  - [ ] Queue clicks for batch sending
  - [ ] Offline click storage and sync
  - [ ] Attribution data collection
- [ ] Create DeepLinkService
  - [ ] Parse incoming deep links
  - [ ] Route to appropriate content
  - [ ] Handle affiliate return links

#### 6.3.2 Product Components
- [ ] Create ProductCard component
  - [ ] Product image with retailer logo
  - [ ] Product name and brand
  - [ ] Price display (with sale price if applicable)
  - [ ] Fit feedback summary (from post context)
  - [ ] Size worn badge
  - [ ] "Shop Now" CTA button
- [ ] Create ProductDetailModal (Material bottom sheet)
  - [ ] Product images carousel
  - [ ] Full product info
  - [ ] Size guide link
  - [ ] Fit feedback from multiple posts
  - [ ] "Shop at [Retailer]" button
  - [ ] Similar products
- [ ] Create ProductListItem component
  - [ ] Compact horizontal layout
  - [ ] Image, name, price, retailer
  - [ ] Fit rating indicator

#### 6.3.3 Shopping Flow
- [ ] Create ShopTheLook component
  - [ ] Grid of tagged products from post
  - [ ] Total price calculation
  - [ ] "Shop All" option
  - [ ] Individual product selection
- [ ] Create ProductRedirectHandler
  - [ ] Show loading indicator
  - [ ] Track click before redirect
  - [ ] Open in-app browser (Capacitor Browser)
  - [ ] Handle redirect completion
- [ ] Create InAppBrowser wrapper
  - [ ] Custom toolbar with close button
  - [ ] URL display
  - [ ] Share button
  - [ ] "Open in Safari/Chrome" option

#### 6.3.4 Deep Linking
- [ ] Configure Universal Links (iOS)
  - [ ] apple-app-site-association file
  - [ ] Handle lyke.app/post/{id} links
  - [ ] Handle lyke.app/product/{id} links
  - [ ] Handle lyke.app/creator/{id} links
- [ ] Configure App Links (Android)
  - [ ] assetlinks.json file
  - [ ] Intent filters for deep links
- [ ] Create DeepLinkRouter
  - [ ] Parse link parameters
  - [ ] Navigate to correct page
  - [ ] Handle unauthenticated state

---

## Phase 7: Creator Features

### 7.1 Backend - Creator APIs
```
POST   /api/creators/register      - Register as creator ✅
GET    /api/creators/profile       - Get creator profile ✅
PUT    /api/creators/profile       - Update creator profile ✅
POST   /api/creators/posts         - Create new post ✅
PUT    /api/creators/posts/{id}    - Update post ✅
DELETE /api/creators/posts/{id}    - Delete post ✅
GET    /api/creators/posts         - Get creator's posts ✅
GET    /api/creators/posts/{id}    - Get specific post ✅
POST   /api/creators/posts/{id}/submit - Submit for review ✅
GET    /api/creators/analytics     - Get performance metrics ✅
GET    /api/creators/earnings/summary - Get earnings summary ✅
GET    /api/creators/earnings/history - Get earnings history ✅
```

### 7.2 Backend Tasks
- [x] Create CreatorService
- [x] Implement creator verification workflow
  - [x] VerificationStatus enum (NotSubmitted, Pending, Approved, Rejected)
  - [x] Creator entity verification fields and migration
  - [x] Submit verification endpoint for creators
  - [x] Admin endpoints to list, view, and review verifications
  - [x] Validators for verification requests
- [x] Build media upload service (Azure Blob/S3)
  - [x] Image optimization and resizing (ImageSharp)
  - [x] Video processing and thumbnail extraction (FFmpeg)
  - [x] Thumbnail generation
  - [x] SAS token generation for secure access
  - [x] Bulk upload support (up to 10 files)
- [x] Create post draft/publish workflow
- [x] Implement product search and tagging
- [x] Build earnings calculation service
- [x] Create analytics aggregation queries

### 7.3 Frontend - Creator Module

#### 7.3.1 Creator Services
- [ ] Create CreatorService
  - [ ] Register as creator
  - [ ] Get/update creator profile
  - [ ] Get creator posts (with filters)
  - [ ] Create/update/delete posts
  - [ ] Submit post for review
  - [ ] Get analytics data
  - [ ] Get earnings summary and history
- [ ] Create MediaUploadService
  - [ ] Single file upload with progress
  - [ ] Bulk upload (up to 10 files)
  - [ ] Image compression before upload
  - [ ] Video compression before upload
  - [ ] Upload cancellation
  - [ ] Retry failed uploads
- [ ] Create CreatorStore (NgRx Signals)
  - [ ] Creator profile state
  - [ ] Posts list state (drafts, published, rejected)
  - [ ] Current draft state
  - [ ] Analytics state
  - [ ] Earnings state
  - [ ] Verification status state

#### 7.3.2 Creator Registration
- [ ] Create CreatorRegistrationPage
  - [ ] Benefits overview
  - [ ] Requirements checklist
  - [ ] Display name input
  - [ ] Bio textarea
  - [ ] Social links (Instagram, TikTok, YouTube)
  - [ ] Terms acceptance for creators
  - [ ] Submit registration
- [ ] Create VerificationFlow
  - [ ] Verification status display
  - [ ] Document upload instructions
  - [ ] ID document upload
  - [ ] Selfie verification upload
  - [ ] Submission confirmation
  - [ ] Status tracking (Pending, Approved, Rejected)
  - [ ] Rejection reason display with resubmit option

#### 7.3.3 Creator Dashboard
- [ ] Create CreatorDashboardPage
  - [ ] Welcome header with creator name
  - [ ] Verification status banner (if pending/rejected)
  - [ ] Quick stats cards (views, likes, earnings)
  - [ ] "Create Post" FAB button
  - [ ] Recent posts preview
  - [ ] Earnings summary widget
  - [ ] Tips/announcements section
- [ ] Create QuickStatsCard component
  - [ ] Icon, value, label, trend indicator
  - [ ] Tap to view detailed analytics
- [ ] Create RecentPostsWidget component
  - [ ] Horizontal scroll of recent posts
  - [ ] Post status badges
  - [ ] "View All" link
- [ ] Create EarningsSummaryWidget component
  - [ ] Current balance
  - [ ] Pending earnings
  - [ ] "View Details" link

#### 7.3.4 Post Creation Wizard
- [ ] Create PostCreationPage (Material Stepper)
  - [ ] Step navigation with validation
  - [ ] Save draft at each step
  - [ ] Exit confirmation if unsaved changes
- [ ] Create MediaUploadStep component
  - [ ] Camera capture button
  - [ ] Gallery selection button
  - [ ] Drag-and-drop zone (web)
  - [ ] Media preview grid
  - [ ] Reorder media (drag handles)
  - [ ] Remove media button
  - [ ] Upload progress per file
  - [ ] File type validation (images, videos)
  - [ ] Max file count indicator
- [ ] Create ProductTaggingStep component
  - [ ] Media preview with tap-to-tag
  - [ ] Product search autocomplete
  - [ ] Tag position dragging
  - [ ] Tagged products list
  - [ ] Remove tag option
  - [ ] "Can't find product" option
- [ ] Create FitDetailsStep component
  - [ ] Per-product fit form
  - [ ] Size worn selector (XS-XXL)
  - [ ] Fit rating (TooSmall → TooLarge scale)
  - [ ] Fit tags selection (chips)
  - [ ] Fit notes textarea
- [ ] Create StylingNotesStep component
  - [ ] Post title input
  - [ ] Post description textarea
  - [ ] Styling tips textarea
  - [ ] Occasion tags (chips)
- [ ] Create PostPreviewStep component
  - [ ] Full post preview as it will appear
  - [ ] Edit buttons per section
  - [ ] Submit for review button
  - [ ] Save as draft button

#### 7.3.5 Product Search & Tagging
- [ ] Create ProductSearchModal
  - [ ] Search input with debounce
  - [ ] Filter by retailer
  - [ ] Search results list
  - [ ] Recent/suggested products
  - [ ] Product selection
- [ ] Create ProductTagMarker component
  - [ ] Draggable position on image
  - [ ] Product name tooltip
  - [ ] Edit/remove actions
- [ ] Create TaggedProductsList component
  - [ ] List of tagged products with fit info
  - [ ] Edit fit details action
  - [ ] Remove product action

#### 7.3.6 Post Management
- [ ] Create PostManagementPage
  - [ ] Tab segments: Drafts, Under Review, Published, Rejected
  - [ ] Post list with status badges
  - [ ] Sort options (newest, oldest, most views)
  - [ ] Bulk actions (delete drafts)
- [ ] Create PostListItem component
  - [ ] Thumbnail image
  - [ ] Title, date, status badge
  - [ ] Quick stats (views, likes)
  - [ ] Actions menu (edit, delete, view)
- [ ] Create PostStatusBadge component
  - [ ] Color-coded status indicator
  - [ ] Draft (gray), Under Review (yellow), Published (green), Rejected (red)
- [ ] Create RejectionDetailsModal
  - [ ] Rejection reason display
  - [ ] Moderator notes
  - [ ] "Edit and Resubmit" button

#### 7.3.7 Analytics Page
- [ ] Create CreatorAnalyticsPage
  - [ ] Date range selector
  - [ ] Overview stats cards
  - [ ] Performance charts
  - [ ] Top posts list
- [ ] Create PerformanceChart component (Chart.js)
  - [ ] Views over time (line chart)
  - [ ] Engagement rate (line chart)
  - [ ] Toggle between metrics
- [ ] Create TopPostsTable component
  - [ ] Sortable columns (views, likes, clicks)
  - [ ] Post thumbnail and title
  - [ ] Engagement metrics
- [ ] Create AudienceInsights component
  - [ ] Body type distribution (pie chart)
  - [ ] Geographic distribution (if available)

#### 7.3.8 Earnings Page
- [ ] Create EarningsPage
  - [ ] Current balance card
  - [ ] Pending earnings card
  - [ ] Payout request button
  - [ ] Earnings history list
  - [ ] Payout history list
- [ ] Create EarningsHistoryList component
  - [ ] Grouped by month
  - [ ] Earning type (Affiliate, Sponsored)
  - [ ] Amount, status, date
  - [ ] Click to view details
- [ ] Create PayoutRequestModal
  - [ ] Available balance
  - [ ] Payout method selection
  - [ ] Minimum payout threshold check
  - [ ] Confirmation
- [ ] Create PayoutMethodSettings component
  - [ ] Add/edit payout methods
  - [ ] PayPal, bank transfer options
  - [ ] Default method selection

---

## Phase 8: Retailer Portal (B2B)

### 8.1 Backend - Retailer APIs
```
POST   /api/retailers/register     - Register retailer
GET    /api/retailers/profile      - Get retailer profile
PUT    /api/retailers/profile      - Update retailer profile
POST   /api/retailers/products/import - Import product feed
GET    /api/retailers/products     - List products
PUT    /api/retailers/products/{id} - Update product
POST   /api/retailers/campaigns    - Create sponsored campaign
GET    /api/retailers/campaigns    - List campaigns
PUT    /api/retailers/campaigns/{id} - Update campaign
GET    /api/retailers/analytics    - Get engagement analytics
GET    /api/retailers/analytics/export - Export analytics CSV
GET    /api/retailers/insights/fit - Get fit feedback insights
GET    /api/retailers/insights/body-profiles - Get body profile insights
```

### 8.2 Backend Tasks
- [ ] Create RetailerService
- [ ] Build product feed importer
  - CSV parser
  - API endpoint for real-time updates
  - Validation and error reporting
- [ ] Create sponsored placement service
  - Budget management
  - Targeting logic
  - Impression/click tracking
- [ ] Build analytics aggregation service
  - Engagement by body profile band
  - Fit feedback trends
  - Category performance
- [ ] Implement data anonymization layer
- [ ] Create CSV/Excel export functionality

### 8.3 Frontend - Retailer Module (Separate Web App or Mobile)
- [ ] Create retailer dashboard
- [ ] Build product management interface
- [ ] Create campaign builder
- [ ] Build analytics dashboard with charts
  - Engagement metrics
  - Body profile distribution
  - Fit feedback analysis
- [ ] Create export functionality
- [ ] Build product feed upload interface

---

## Phase 9: Admin & Moderation

### 9.1 Backend - Admin APIs
```
GET    /api/admin/verifications         - Get creator verification requests ✅
GET    /api/admin/verifications/{id}    - Get verification details ✅
POST   /api/admin/verifications/{id}/review - Approve/reject verification ✅
GET    /api/admin/posts                 - Get posts pending review ✅
GET    /api/admin/posts/{id}            - Get post for moderation ✅
POST   /api/admin/posts/{id}/moderate   - Approve/reject post ✅
GET    /api/admin/users                 - List users (paginated) ✅
GET    /api/admin/users/{id}            - Get user details ✅
POST   /api/admin/users/{id}/suspend    - Suspend user ✅
POST   /api/admin/users/{id}/unsuspend  - Unsuspend user ✅
GET    /api/admin/stats                 - Platform statistics ✅
GET    /api/admin/reports               - Get reported content
POST   /api/admin/posts/{id}/tags       - Correct product tags
```

### 9.2 Backend Tasks
- [x] Create AdminEndpoints with verification management
- [x] Create AdminService with post moderation, user management, platform stats
- [x] Post moderation tracking (ModeratedByUserId, ModeratedAt)
- [x] User suspension tracking (SuspendedAt, SuspendedByUserId, SuspensionReason)
- [x] Platform statistics aggregation (users, content, verifications, engagement)
- [x] Validators for moderation requests
- [ ] Implement content flagging system
  - Duplicate detection (image hashing)
  - Text analysis for abuse
  - Product tag validation
- [ ] Build moderation queue with priority
- [ ] Create admin audit logging
- [ ] Implement bulk actions

### 9.3 Frontend - Admin Module (Web App)
- [ ] Create admin dashboard
- [ ] Build moderation queue interface
  - Post preview
  - Approve/reject buttons
  - Feedback input
- [ ] Create user management interface
- [ ] Build platform analytics dashboard
- [ ] Create content flagging review interface

---

## Phase 10: Analytics & Event Tracking

### 10.1 Backend Tasks
- [ ] Create EventTrackingService
- [ ] Define event schema
  ```
  - post.view
  - post.like
  - post.save
  - post.share
  - product.click
  - product.convert
  - feed.filter
  - search.execute
  - profile.complete
  ```
- [ ] Implement event batching and async processing
- [ ] Create analytics aggregation jobs (background service)
- [ ] Build real-time metrics endpoints
- [ ] Set up Application Insights / custom analytics

### 10.2 Frontend - Analytics Implementation

#### 10.2.1 Analytics Services
- [ ] Create AnalyticsService
  - [ ] Initialize analytics on app start
  - [ ] Track page views automatically
  - [ ] Track custom events
  - [ ] Set user properties (anonymous ID, user type)
  - [ ] Batch events for efficiency
  - [ ] Offline event queuing
- [ ] Create SessionService
  - [ ] Generate session ID
  - [ ] Track session duration
  - [ ] Track session start/end
  - [ ] Handle app background/foreground
- [ ] Create PerformanceService
  - [ ] Track app startup time
  - [ ] Track screen load times
  - [ ] Track API response times
  - [ ] Track media load times

#### 10.2.2 Event Tracking Implementation
- [ ] Implement page view tracking
  - [ ] Router event subscription
  - [ ] Screen name extraction
  - [ ] Previous screen tracking
- [ ] Implement engagement events
  - [ ] Post view (with duration)
  - [ ] Post like/unlike
  - [ ] Post save/unsave
  - [ ] Post share
  - [ ] Product click
  - [ ] Search query
  - [ ] Filter applied
- [ ] Implement conversion events
  - [ ] Registration complete
  - [ ] Profile complete
  - [ ] Body profile created
  - [ ] First post created (creator)
  - [ ] First purchase attributed
- [ ] Implement error tracking
  - [ ] API errors
  - [ ] JavaScript errors
  - [ ] Network failures

#### 10.2.3 Performance Monitoring
- [ ] Implement Core Web Vitals tracking
  - [ ] First Contentful Paint (FCP)
  - [ ] Largest Contentful Paint (LCP)
  - [ ] First Input Delay (FID)
  - [ ] Cumulative Layout Shift (CLS)
- [ ] Implement app-specific metrics
  - [ ] Time to interactive
  - [ ] Feed load time
  - [ ] Media load time
  - [ ] API latency per endpoint

---

## Phase 11: Privacy & GDPR Compliance

### 11.1 Backend Tasks
- [ ] Implement data export endpoint (GDPR Article 15)
- [ ] Create account deletion with full data purge
- [ ] Build consent management system
- [ ] Implement data retention policies
- [ ] Create anonymization utilities
- [ ] Add audit trail for data access
- [ ] Implement cookie consent tracking

### 11.2 Frontend - Privacy Implementation

#### 11.2.1 Privacy Services
- [ ] Create PrivacyService
  - [ ] Get consent status
  - [ ] Update consent preferences
  - [ ] Request data export
  - [ ] Request account deletion
  - [ ] Get privacy settings
- [ ] Create ConsentStore (NgRx Signals)
  - [ ] Consent preferences state
  - [ ] Consent collection required flag
  - [ ] Last consent update timestamp

#### 11.2.2 Privacy Settings
- [ ] Create PrivacySettingsPage
  - [ ] Data collection toggles
    - [ ] Analytics consent
    - [ ] Personalization consent
    - [ ] Marketing consent
  - [ ] Data visibility settings
    - [ ] Profile visibility (public/private)
    - [ ] Body profile sharing preferences
  - [ ] Data management section
    - [ ] Download my data button
    - [ ] Delete my account button
  - [ ] Privacy policy link
  - [ ] Cookie policy link

#### 11.2.3 Consent Collection
- [ ] Create ConsentModal (first launch)
  - [ ] Clear explanation of data usage
  - [ ] Granular consent options
  - [ ] "Accept All" button
  - [ ] "Customize" option
  - [ ] "Reject Non-Essential" option
  - [ ] Links to privacy policy
- [ ] Create ConsentBanner component
  - [ ] Minimal banner for returning users
  - [ ] "Manage Preferences" link
- [ ] Create CookiePreferencesModal
  - [ ] Essential (always on, disabled toggle)
  - [ ] Analytics (optional)
  - [ ] Personalization (optional)
  - [ ] Marketing (optional)
  - [ ] Save preferences button

#### 11.2.4 Data Management Flows
- [ ] Create DataExportRequestPage
  - [ ] Explanation of what's included
  - [ ] Format selection (JSON, CSV)
  - [ ] Email confirmation
  - [ ] Processing time notice
  - [ ] Submit request button
- [ ] Create DataExportStatusPage
  - [ ] Request status tracking
  - [ ] Download link when ready
  - [ ] Expiry notice
- [ ] Create AccountDeletionFlow
  - [ ] Warning about permanent deletion
  - [ ] What will be deleted list
  - [ ] Creator-specific warnings (earnings, posts)
  - [ ] Password confirmation
  - [ ] Deletion reason (optional survey)
  - [ ] Final confirmation dialog
  - [ ] Cooldown period notice (30 days)
- [ ] Create AccountDeletionConfirmPage
  - [ ] Countdown to deletion
  - [ ] Cancel deletion option
  - [ ] Contact support link

---

## Phase 11.5: Frontend - Settings Module

### 11.5.1 Settings Services
- [ ] Create SettingsService
  - [ ] Get/update app preferences
  - [ ] Get/update notification preferences
  - [ ] Manage unit preferences (metric/imperial)
- [ ] Create NotificationService
  - [ ] Request push notification permission
  - [ ] Register device token
  - [ ] Handle foreground notifications
  - [ ] Handle notification tap routing
- [ ] Create SettingsStore (NgRx Signals)
  - [ ] App preferences state
  - [ ] Notification preferences state
  - [ ] Theme preference (light/dark/system)

### 11.5.2 Settings Pages
- [ ] Create SettingsPage (main settings hub)
  - [ ] User section (profile, body profile links)
  - [ ] App preferences section
  - [ ] Notifications section
  - [ ] Privacy section link
  - [ ] Support section
  - [ ] Legal section
  - [ ] Logout button
  - [ ] App version display
- [ ] Create AppPreferencesPage
  - [ ] Theme selection (Light, Dark, System)
  - [ ] Unit preference (Metric, Imperial)
  - [ ] Language selection (future)
  - [ ] Feed preferences
    - [ ] Autoplay videos toggle
    - [ ] Data saver mode toggle
- [ ] Create NotificationPreferencesPage
  - [ ] Push notification master toggle
  - [ ] Email notification preferences
    - [ ] Marketing emails toggle
    - [ ] Product updates toggle
  - [ ] In-app notification preferences
    - [ ] New followers (creators)
    - [ ] Post engagement alerts
    - [ ] Earnings notifications (creators)
- [ ] Create SupportPage
  - [ ] FAQ accordion
  - [ ] Contact support button (email)
  - [ ] Report a bug option
  - [ ] Feature request option
  - [ ] Community links (if any)
- [ ] Create LegalPage
  - [ ] Terms of Service link
  - [ ] Privacy Policy link
  - [ ] Cookie Policy link
  - [ ] Open source licenses
- [ ] Create AboutPage
  - [ ] App logo and name
  - [ ] Version number
  - [ ] Build number
  - [ ] Changelog link
  - [ ] Social media links

### 11.5.3 Theme Management
- [ ] Create ThemeService
  - [ ] Apply theme based on preference
  - [ ] Listen to system theme changes
  - [ ] Persist theme preference
- [ ] Implement theme switching
  - [ ] Smooth transition animation
  - [ ] Update status bar color
  - [ ] Update navigation bar color (Android)

---

## Phase 12: Testing Strategy

### 12.1 Backend Testing
- [x] Unit tests for services (xUnit)
  - [x] AuthServiceTests (registration, login, token refresh, logout)
  - [x] ProfileServiceTests (body profile management)
  - [x] FeedServiceTests (feed generation, similarity matching)
  - [x] CreatorServiceTests (registration, post management, analytics)
  - [x] CommerceServiceTests (click tracking, product search)
- [x] Test infrastructure (TestDbContextFactory, MockUserManager, LykeWebApplicationFactory)
- [x] Integration tests for API endpoints (7 test suites)
  - [x] AuthEndpointsTests (registration, login, token refresh, logout)
  - [x] ProfileEndpointsTests (profile CRUD operations)
  - [x] CreatorEndpointsTests (creator registration, posts, verification)
  - [x] FeedEndpointsTests (feed generation, search, engagement)
  - [x] MediaEndpointsTests (media upload, deletion)
  - [x] CommerceEndpointsTests (click tracking, product/retailer endpoints)
  - [x] AdminEndpointsTests (user management, post moderation, verification review)
- [ ] Database tests with test containers
- [ ] Load testing with k6 or similar
- [ ] Security testing (OWASP ZAP)

### 12.2 Frontend Testing

#### 12.2.1 Unit Testing Setup
- [ ] Configure Jest for Angular
  - [ ] Install `jest`, `@types/jest`, `jest-preset-angular`
  - [ ] Configure `jest.config.js`
  - [ ] Set up code coverage reporting
  - [ ] Configure CI integration
- [ ] Create testing utilities
  - [ ] Mock services factory
  - [ ] Test data builders
  - [ ] Custom matchers

#### 12.2.2 Service Unit Tests
- [ ] AuthService tests
  - [ ] Login success/failure scenarios
  - [ ] Token refresh logic
  - [ ] Logout cleanup
- [ ] ProfileService tests
  - [ ] CRUD operations
  - [ ] Error handling
- [ ] FeedService tests
  - [ ] Pagination logic
  - [ ] Filter application
- [ ] CreatorService tests
  - [ ] Post creation workflow
  - [ ] Media upload handling
- [ ] CommerceService tests
  - [ ] Click tracking
  - [ ] Product search
- [ ] Store tests (NgRx Signals)
  - [ ] State transitions
  - [ ] Computed signals
  - [ ] Side effects

#### 12.2.3 Component Tests
- [ ] Configure Angular Testing Library
- [ ] Auth component tests
  - [ ] LoginPage form validation
  - [ ] RegisterPage form submission
  - [ ] Password reset flow
- [ ] Profile component tests
  - [ ] Onboarding wizard navigation
  - [ ] Body profile form
  - [ ] Unit conversion
- [ ] Feed component tests
  - [ ] PostCard rendering
  - [ ] Engagement interactions
  - [ ] Filter application
- [ ] Creator component tests
  - [ ] Post creation wizard
  - [ ] Media upload
  - [ ] Product tagging
- [ ] Shared component tests
  - [ ] All reusable components

#### 12.2.4 E2E Testing
- [ ] Configure Cypress
  - [ ] Install and configure
  - [ ] Set up test database seeding
  - [ ] Configure CI pipeline
- [ ] Auth flow E2E tests
  - [ ] Complete registration flow
  - [ ] Login and logout
  - [ ] Password reset
- [ ] Onboarding E2E tests
  - [ ] Complete body profile setup
  - [ ] Skip and return later
- [ ] Feed E2E tests
  - [ ] Browse feed
  - [ ] Apply filters
  - [ ] Search content
  - [ ] Engage with posts
- [ ] Creator E2E tests
  - [ ] Creator registration
  - [ ] Create and submit post
  - [ ] View analytics
- [ ] Commerce E2E tests
  - [ ] Product click tracking
  - [ ] External redirect

#### 12.2.5 Visual Regression Testing
- [ ] Configure Percy or Chromatic
- [ ] Capture baseline screenshots
  - [ ] All page layouts
  - [ ] Component states
  - [ ] Dark/light mode
- [ ] Set up CI integration
- [ ] Review workflow for visual changes

#### 12.2.6 Performance Testing
- [ ] Configure Lighthouse CI
- [ ] Set performance budgets
  - [ ] FCP < 1.5s
  - [ ] LCP < 2.5s
  - [ ] TTI < 3.5s
  - [ ] Bundle size limits
- [ ] Create performance test suite
  - [ ] Initial load time
  - [ ] Feed scroll performance
  - [ ] Media loading
  - [ ] Route transitions

### 12.3 Test Coverage Targets
- Backend: 80% code coverage minimum
- Frontend: 70% code coverage minimum
- Critical paths: 100% coverage

---

## Phase 13: Performance & Optimization

### 13.1 Backend Optimization
- [ ] Implement response caching
- [ ] Add Redis caching for:
  - Feed results
  - User profiles
  - Product data
- [ ] Optimize EF Core queries (no N+1)
- [ ] Implement database connection pooling
- [ ] Add pagination everywhere
- [ ] Implement request rate limiting

### 13.2 Frontend Optimization
- [ ] Implement lazy loading for modules
- [ ] Add image lazy loading
- [ ] Optimize bundle size (tree shaking)
- [ ] Implement virtual scrolling for feeds
- [ ] Add service worker for caching
- [ ] Optimize images (WebP, srcset)

---

## Phase 14: Deployment & DevOps

### 14.1 Infrastructure Setup
- [ ] Set up Azure/AWS infrastructure
  - App Service / ECS for API
  - Azure SQL / RDS for PostgreSQL
  - Blob Storage / S3 for media
  - Redis Cache
  - CDN for static assets
- [ ] Configure auto-scaling rules
- [ ] Set up monitoring and alerting
- [ ] Configure backup strategies

### 14.2 Mobile App Deployment
- [ ] Configure iOS build (Xcode, certificates)
- [ ] Configure Android build (Gradle, signing)
- [ ] Set up App Store Connect
- [ ] Set up Google Play Console
- [ ] Create app store assets (screenshots, descriptions)
- [ ] Implement CodePush/OTA updates

---

## Phase 15: MVP Launch Checklist

### Pre-Launch
- [ ] Security audit completed
- [ ] Performance benchmarks met
- [ ] GDPR compliance verified
- [ ] App store compliance checked
- [ ] Legal review of terms/privacy policy
- [ ] Single retailer integration tested
- [ ] Creator cohort onboarded
- [ ] Content seeded for launch

### Launch
- [ ] Soft launch to limited audience
- [ ] Monitor error rates and performance
- [ ] Gather initial user feedback
- [ ] Iterate on critical issues
- [ ] Full launch

### Post-Launch
- [ ] Monitor KPIs (conversion, returns, engagement)
- [ ] A/B test matching algorithm
- [ ] Gather retailer feedback
- [ ] Plan Phase 2 features

---

## Appendix A: API Response Standards

```json
// Success Response
{
  "success": true,
  "data": { },
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}

// Error Response
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      { "field": "email", "message": "Email is required" }
    ]
  }
}
```

---

## Appendix B: Folder Structure

### Backend (Implemented Structure)
```
/src
  /Lyke.Api
    /Endpoints          - Minimal API endpoint definitions
    /Middleware         - Exception handling middleware
    Program.cs          - Application bootstrap
  /Lyke.Core
    /Entities           - Domain entities (User, Post, Product, etc.)
    /Interfaces         - Service and repository interfaces
    /Enums              - Domain enumerations
    /Exceptions         - Custom domain exceptions
  /Lyke.Application
    /Services           - Business logic services
    /DTOs               - Data transfer objects (requests/responses)
    /Validators         - FluentValidation validators
    /Configuration      - Settings classes (JwtSettings, MediaUploadSettings, etc.)
    /Interfaces         - Service interfaces (IMediaService, IStorageService, etc.)
  /Lyke.Infrastructure
    /Data
      /Configurations   - EF Core entity configurations
      /Migrations       - Database migrations
      LykeDbContext.cs  - DbContext with Identity
    /Repositories       - Generic repository implementation
    /Storage            - Azure Blob Storage implementation
    /Services           - Image/Video processing services
    /Configuration      - Infrastructure settings (AzureBlobSettings)
  /Lyke.AppHost         - .NET Aspire orchestration
  /Lyke.ServiceDefaults - Aspire service defaults
/tests
  /Lyke.UnitTests       - Service unit tests (5 test suites)
  /Lyke.IntegrationTests - API integration tests
```

### Frontend (Ionic/Angular with Material UI)
```
/mobile
  /src
    /app
      /core
        /services
          auth.service.ts
          token.service.ts
          profile.service.ts
          feed.service.ts
          engagement.service.ts
          creator.service.ts
          commerce.service.ts
          media-upload.service.ts
          analytics.service.ts
          privacy.service.ts
          lookup.service.ts
          unit-conversion.service.ts
          deep-link.service.ts
        /guards
          auth.guard.ts
          guest.guard.ts
          role.guard.ts
          onboarding.guard.ts
        /interceptors
          auth.interceptor.ts
          error.interceptor.ts
          loading.interceptor.ts
        /state
          auth.store.ts
          profile.store.ts
          feed.store.ts
          creator.store.ts
          consent.store.ts
        /utils
          validators.ts
          helpers.ts
      /shared
        /components
          /buttons
            like-button/
            save-button/
            share-button/
          /cards
            post-card/
            post-card-skeleton/
            product-card/
            stats-card/
          /forms
            height-input/
            weight-input/
            search-input/
          /media
            media-carousel/
            image-viewer/
            video-player/
          /feedback
            loading-spinner/
            empty-state/
            error-state/
          /layout
            page-header/
            tab-layout/
        /pipes
          relative-time.pipe.ts
          unit-format.pipe.ts
          truncate.pipe.ts
        /directives
          lazy-load.directive.ts
          infinite-scroll.directive.ts
          haptic-feedback.directive.ts
        /validators
          password-strength.validator.ts
          match-fields.validator.ts
      /features
        /auth
          /pages
            login/
            register/
            forgot-password/
            reset-password/
          /components
            social-login-buttons/
          auth.routes.ts
        /onboarding
          /pages
            onboarding/
          /components
            welcome-step/
            height-input-step/
            weight-input-step/
            body-type-step/
            fit-preference-step/
            complete-step/
          onboarding.routes.ts
        /feed
          /pages
            feed/
            explore/
            search/
            saved-posts/
          /components
            filter-drawer/
            sort-options/
            active-filters-bar/
            engagement-bar/
          feed.routes.ts
        /post
          /pages
            post-detail/
          /components
            creator-profile-card/
            tagged-products-list/
            similar-posts/
          post.routes.ts
        /profile
          /pages
            profile/
            edit-body-profile/
          /components
            profile-header/
            body-profile-card/
            avatar-upload/
            edit-profile-modal/
          profile.routes.ts
        /creator
          /pages
            creator-dashboard/
            post-creation/
            post-management/
            analytics/
            earnings/
            verification/
          /components
            quick-stats-card/
            recent-posts-widget/
            earnings-summary-widget/
            media-upload-step/
            product-tagging-step/
            fit-details-step/
            styling-notes-step/
            post-preview-step/
            product-search-modal/
            post-list-item/
            performance-chart/
          creator.routes.ts
        /commerce
          /components
            shop-the-look/
            product-detail-modal/
            in-app-browser/
          commerce.routes.ts
        /settings
          /pages
            settings/
            privacy-settings/
            account-deletion/
            data-export/
          /components
            consent-modal/
            consent-banner/
            cookie-preferences-modal/
          settings.routes.ts
      /models
        user.model.ts
        body-profile.model.ts
        post.model.ts
        product.model.ts
        creator.model.ts
        engagement.model.ts
        api-response.model.ts
      app.component.ts
      app.config.ts
      app.routes.ts
    /assets
      /icons
      /images
      /animations (Lottie files)
    /environments
      environment.ts
      environment.staging.ts
      environment.prod.ts
    /theme
      _variables.scss
      _material-theme.scss
      _dark-theme.scss
      _typography.scss
      global.scss
  /capacitor
    /ios
    /android
  /cypress
    /e2e
    /fixtures
    /support
  angular.json
  capacitor.config.ts
  jest.config.js
  package.json
```

---

## Appendix C: Environment Variables

### Backend
```
DATABASE_URL=postgresql://user:pass@host:5432/lyke
JWT_SECRET=<secret>
JWT_EXPIRY_MINUTES=60
REFRESH_TOKEN_EXPIRY_DAYS=30
AZURE_STORAGE_CONNECTION=<connection-string>
REDIS_CONNECTION=<connection-string>
GOOGLE_CLIENT_ID=<client-id>
APPLE_CLIENT_ID=<client-id>
```

### Frontend
```
API_BASE_URL=https://api.lyke.app
GOOGLE_CLIENT_ID=<client-id>
APPLE_CLIENT_ID=<client-id>
ANALYTICS_KEY=<key>
```

---

## Estimated Task Breakdown

| Phase | Backend Tasks | Frontend Tasks | Total | Completed | Status |
|-------|---------------|----------------|-------|-----------|--------|
| Phase 1: Foundation | 8 | 42 | 50 | 8 | 16% |
| Phase 2: Database | 8 | 0 | 8 | 7 | 88% |
| Phase 3: Authentication | 14 | 48 | 62 | 14 | 23% |
| Phase 4: User Profile | 13 | 45 | 58 | 13 | 22% |
| Phase 5: Content Feed + Shared UI | 14 | 105 | 119 | 14 | 12% |
| Phase 6: Commerce | 7 | 28 | 35 | 7 | 20% |
| Phase 7: Creator | 14 | 95 | 109 | 14 | 13% |
| Phase 8: Retailer Portal | 12 | 6 | 18 | 0 | 0% |
| Phase 9: Admin | 14 | 5 | 19 | 7 | 37% |
| Phase 10: Analytics | 6 | 22 | 28 | 0 | 0% |
| Phase 11: Privacy + Settings | 7 | 52 | 59 | 0 | 0% |
| Phase 12: Testing | 5 | 42 | 47 | 9 | 19% |
| Phase 13: Performance | 6 | 8 | 14 | 0 | 0% |
| Phase 14: Deployment | 10 | 6 | 16 | 0 | 0% |
| Phase 15: Launch | 8 | 0 | 8 | 0 | 0% |

**Backend Tasks: ~136 (~93 completed, ~68% complete)**
**Frontend Tasks: ~504 (~0 completed, 0% complete)**
**Total: ~650 actionable tasks (~97 completed, ~15% overall)**

### Task Breakdown by Category

| Category | Tasks | Priority |
|----------|-------|----------|
| Backend API & Services | 93 ✅ | Complete |
| Backend Testing | 9 ✅ | Complete |
| Backend Remaining | 34 | High |
| Frontend Core (Auth, Profile, Settings) | 145 | Critical |
| Frontend Feed & Discovery | 105 | Critical |
| Frontend Creator Module | 95 | High |
| Frontend Commerce | 28 | High |
| Frontend Shared Components | 65 | Critical (prerequisite) |
| Frontend Testing | 42 | High |
| DevOps & Deployment | 24 | High |

---

## Recommended MVP Sequence

### Sprint 1-2: Frontend Foundation
1. **Project Setup** (Phase 1.2): Initialize Ionic Angular project, configure Material UI, set up Capacitor
2. **Shared Components** (Phase 5.3.5): Build reusable component library (critical prerequisite)
3. **Core Services**: Create API service layer, auth interceptors, state management

### Sprint 3-4: Authentication & Onboarding
4. **Auth Module** (Phase 3.3): Login, register, password reset, social login
5. **Profile Module** (Phase 4.3): Onboarding wizard, body profile setup

### Sprint 5-6: Core User Experience
6. **Feed Module** (Phase 5.4): Personalized feed, explore, search, engagement
7. **Post Detail** (Phase 5.4.4): Full post view, media viewer, product tags

### Sprint 7-8: Commerce & Creator Basics
8. **Commerce Module** (Phase 6.3): Product views, click tracking, shopping flow
9. **Creator Registration** (Phase 7.3.1-7.3.2): Creator signup, verification

### Sprint 9-10: Creator Tools
10. **Post Creation** (Phase 7.3.4): Media upload, product tagging, fit details
11. **Creator Dashboard** (Phase 7.3.3): Analytics, earnings, post management

### Sprint 11-12: Polish & Compliance
12. **Settings & Privacy** (Phase 11.2, 11.5): Settings, consent, data management
13. **Testing** (Phase 12.2): E2E tests, visual regression, performance

### Sprint 13-14: Launch Preparation
14. **Native Builds** (Phase 14): iOS and Android builds, app store prep
15. **Launch** (Phase 15): Soft launch, monitoring, iteration

### Parallel Workstreams
- **Retailer Portal** (Phase 8): Can be developed separately as a web app
- **Admin Dashboard** (Phase 9): Can use existing web-based admin tools initially
- **Backend Enhancements**: Redis caching, analytics aggregation, email service
