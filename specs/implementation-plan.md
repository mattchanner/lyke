# LYKE Mobile Application - Implementation Plan

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend API | C# ASP.NET Core 10.0 |
| ORM | Entity Framework Core 9.0 |
| Database | PostgreSQL 16 |
| Frontend | Ionic 8 + Angular 20 |
| Authentication | ASP.NET Core Identity + JWT |
| File Storage | Azure Blob Storage / AWS S3 |
| Search | PostgreSQL Full-Text Search (MVP) |
| Caching | Redis |
| Analytics | Application Insights / Custom Events |

---

## Current Progress Summary

**Last Updated:** 2026-02-28

| Component | Status |
|-----------|--------|
| Backend Foundation | ✅ Complete |
| Database Schema & Migrations | ✅ Complete |
| Entity Configurations | ✅ Complete (16 entities) |
| Repository Pattern | ✅ Complete |
| Exception Handling | ✅ Complete |
| JWT Infrastructure | ✅ Complete |
| Auth Service & Endpoints | ✅ Complete (7 endpoints) |
| Profile Service & Endpoints | ✅ Complete (8 endpoints) |
| Feed Service & Endpoints | ✅ Complete (9 endpoints) |
| Commerce Service & Endpoints | ✅ Complete (6 endpoints) |
| Creator Service & Endpoints | ✅ Complete (15 endpoints) |
| Media Upload Service | ✅ Complete (5 endpoints, async via Azure Queue + Functions) |
| Async Media Processing | ✅ Complete (Lyke.Functions isolated worker, queue trigger, status polling) |
| Creator Verification Workflow | ✅ Complete (5 endpoints) |
| Admin Service & Endpoints | ✅ Complete (17 endpoints) |
| Retailer Portal Service & Endpoints | ✅ Complete (14 endpoints) |
| FluentValidation | ✅ Complete (37 validators) |
| Seed Data (Lookups) | ✅ Complete (8 body types, 12 fit tags) |
| Role-Based Authorization | ✅ Complete (5 policies) |
| Unit Tests | ✅ Complete (5 service test suites) |
| Integration Tests | ✅ Complete (7 endpoint test suites, no RetailerEndpoints tests) |
| Frontend Foundation | ✅ Complete (Ionic 8 + Angular 20) |
| Frontend Models & Enums | ✅ Complete (11 enums, all DTOs) |
| Frontend Core Services | ✅ Complete (Auth, API, Storage, Toast, Creator, Commerce, Admin, Retailer, SocialAuth, DeepLink, AppInsights, Analytics, WebVitals) |
| Frontend Auth Pages | ✅ Complete (Login, Register, Password Reset) |
| Frontend Onboarding | ✅ Complete (Body Profile Wizard) |
| Frontend Feed | ✅ Complete (FeedHome, Explore, PostDetail, Saved, FilterModal) |
| Frontend Shared Components | ✅ Complete (PostCard, MediaCarousel, ProductCard, ShopTheLook, LoadingSkeleton) |
| Frontend Profile Module | ✅ Complete (ProfileView, ProfileEdit, BodyProfileEdit) |
| Frontend Creator Module | ✅ Complete (8 pages: Dashboard, Posts, PostCreate, PostEdit, Analytics, Earnings, Verification, Register) |
| Frontend Settings | ✅ Complete (Settings hub, Privacy, DeleteAccount) |
| Frontend Admin Module | ✅ Complete (7 pages: Dashboard, PostModeration, UserManagement, UserDetail, VerificationReview, ContentReports, ModerationQueue) |
| Frontend Commerce Pages | ✅ Complete (ProductDetail, ProductSearch, RetailerList, RetailerStorefront, ProductGridCard) |
| Frontend Social Login | ⏸️ On Hold (backend + frontend implemented, Google GIS web flow ready, disabled pending OAuth credential setup) |
| Frontend Deep Linking | ✅ Complete (DeepLinkService with universal link handling) |
| Frontend Retailer Module | ✅ Complete (9 pages: Dashboard, Profile, Products, ProductEdit, Campaigns, CampaignCreate, Analytics, Insights, Register) |
| Email / Transactional Messaging | ✅ Complete (Azure ACS, 6 Liquid templates, rate limiting, all 5 flows wired) |
| Account Management Web Pages | ✅ Complete (Razor Pages: password reset form, email verification, result pages) |
| Frontend App Insights | ✅ Complete (exception tracking, page views, HTTP error telemetry, custom ErrorHandler) |
| Android Platform | ✅ Complete (Capacitor 8, cleartext network config, Gradle setup) |
| CI/CD Pipelines | 🔄 Partial (Azure deploy on push to develop for API + Functions, Android APK build; unit tests in both pipelines; auto-migrations on startup) |
| Infrastructure as Code | ✅ Complete (Terraform: 9 Azure resources, dev/prod tfvars, validated) |
| Creator Payout System | ❌ Not started (design complete — see `specs/payout-design.md`) |
| Affiliate Network Integration | ❌ Not started (design complete — see `specs/affiliate-network-design.md`) |

**Overall Backend Progress: ~99%** | **Overall Frontend Progress: ~98%** | **Overall Project: ~90%**

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
- [x] Initialize Ionic Angular project (Ionic 8 + Angular 20 standalone + Capacitor 8)
- [x] Configure project structure
  - `/src/app/core` - Services, guards, interceptors
  - `/src/app/shared` - Shared components, pipes, directives
  - `/src/app/features` - Feature modules (feed, profile, creator, etc.)
  - `/src/app/models` - TypeScript interfaces/models
- [x] Set up environment configuration (dev/prod)
- [x] Configure HTTP interceptors for auth tokens
- [x] Set up state management (Angular Signals + Services)
- [x] Configure Capacitor 8 for native builds
- [x] Install dependencies (jwt-decode, date-fns, Swiper, @capgo/capacitor-social-login, Capacitor plugins)

### 1.3 CI/CD Pipeline
- [x] Set up GitHub Actions for backend deployment (Azure Web App, triggers on push to develop)
- [x] Set up GitHub Actions for Azure Functions deployment (`develop_lyke-dev-functions-media.yml`; uses `az functionapp deployment source config-zip` via OIDC to avoid SCM/IP restriction issues)
- [x] Set up GitHub Actions for Android APK build (triggers on frontend changes)
- [x] Add `dotnet test` step to backend CI pipeline
- [x] Set up database migrations automation
- [ ] Configure environment deployments (staging/production)

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
- [x] Set up PostgreSQL full-text search indexes
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
- [x] Implement account lockout (manual increment via AccessFailedAsync/ResetAccessFailedCountAsync in AuthService.LoginAsync; 5 attempts, 15 min lockout)

### 3.2 API Endpoints - Authentication (Minimal APIs)
- [x] POST /api/auth/register - Register new user
- [x] POST /api/auth/login - Login with email/password
- [x] POST /api/auth/refresh - Refresh access token
- [x] POST /api/auth/logout - Invalidate refresh token
- [x] POST /api/auth/forgot-password - Request password reset ⚠️ (token generated but no email sent — requires Phase 3B)
- [x] POST /api/auth/reset-password - Reset password with token
- [x] DELETE /api/auth/account - Delete account (GDPR full cascade: body profile, engagements, creator data deleted; click events anonymized)
- [x] POST /api/auth/social/google - Google OAuth login (backend + SocialTokenValidator)
- [x] POST /api/auth/social/apple - Apple Sign-In (backend + SocialTokenValidator)

### 3.3 Frontend Auth Implementation
- [x] Create AuthService with token management (Angular Signals state)
- [x] Implement HTTP interceptor for JWT injection (authInterceptor)
- [x] Create auth guard for protected routes (authGuard, noAuthGuard, roleGuard)
- [x] Build login page component
- [x] Build registration page component
- [x] Implement social login buttons (Google, Apple) — SocialAuthService with native (Capacitor plugin) + web (Google GIS) flows; temporarily disabled pending OAuth credential configuration
- [x] Create forgot/reset password flow
- [x] Implement secure token storage (Capacitor Preferences)

---

## Phase 3B: Email & Transactional Messaging

### 3B.1 Backend - Email Service
- [x] Set up email infrastructure (Azure Communication Services)
- [x] Create IEmailService interface and implementation (7 methods)
- [x] Configure email settings (EmailSettings: sender address, connection string, rate limiting, DryRun mode)
- [x] Wire up DI registration (Singleton) and configuration

### 3B.2 Transactional Email Templates (Fluid/Liquid)
- [x] Password reset email (`password-reset.liquid`)
- [x] Email verification / confirmation email (`email-verification.liquid`)
- [x] Welcome email after registration (`welcome.liquid`)
- [x] Creator verification approved/rejected notification (`creator-verification-result.liquid`)
- [x] Post moderation approved/rejected notification (`post-moderation-result.liquid`)
- [x] Account suspension notification (`account-suspension.liquid`)

### 3B.3 Integration
- [x] Wire password reset endpoint to send email (fire-and-forget in ForgotPasswordAsync)
- [x] Wire email verification into registration flow (fire-and-forget in RegisterAsync)
- [x] Wire welcome email into registration flow (fire-and-forget in RegisterAsync)
- [x] Wire creator verification status change to email notification (in ReviewVerificationAsync)
- [x] Wire post moderation result to email notification (in ModeratePostAsync)
- [x] Wire account suspension to email notification (in SuspendUserAsync)
- [x] Add rate limiting for email sends (in-memory sliding window, configurable per-window max)
- [ ] Add email delivery logging and retry logic

### 3B.4 Account Management Web Pages (Razor Pages)
- [x] Password reset form page (`/account/reset-password`) with validation
- [x] Password reset result page (`/account/reset-password-result`)
- [x] Email verification page (`/account/verify-email`) — replaces inline HTML endpoint
- [x] Shared LYKE-branded layout (`_Layout.cshtml`)

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
- [x] Create onboarding flow component (multi-step wizard)
  - Step 1: Height input (with unit toggle cm/ft)
  - Step 2: Weight input (with unit toggle kg/lbs)
  - Step 3: Body type selection (visual cards)
  - Step 4: Fit preferences
- [x] Create profile view component
- [x] Create profile edit component (email update with validation and dirty-check)
- [x] Create body profile edit component (full form with unit conversion, body type selector, fit preference)
- [x] Implement profile data management (view/edit/delete)
- [x] Add unit conversion utilities (in onboarding and body profile edit pages)

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

### 5.4 Frontend - Feed Module
- [x] Create feed page with infinite scroll
- [x] Build post card component
  - Creator avatar and anonymized stats
  - Media display (single image)
  - Product tags chips
  - Fit feedback summary
  - Engagement buttons (like, save, share)
- [x] Create filter drawer/modal component (FeedFilterModal)
  - Category filter (chips)
  - Retailer filter (searchable list)
  - Fit tags filter (multi-select grouped by category)
- [x] Create post detail page
  - Full media viewer
  - Creator profile summary (anonymized)
  - All tagged products list
  - Fit notes and feedback
  - "Shop Now" CTAs
- [x] Implement pull-to-refresh
- [x] Add skeleton loading states (SkeletonPostCard, SkeletonPostDetail, SkeletonProfile, SkeletonList)
- [x] Create saved posts page
- [x] Create explore/search page
- [x] Build media carousel component (Swiper 12-based, supports images and videos)

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

### 6.3 Frontend Tasks
- [x] Create product card component (shared, barrel exported)
- [x] Implement click tracking before redirect
- [x] Build in-app browser for product views (optional)
- [x] Create "Shop the Look" component (shared, barrel exported)
- [x] Add deep linking support (DeepLinkService with universal link handling)
- [x] Create product detail page
- [x] Create product search page with filters
- [x] Create retailer list page
- [x] Create retailer storefront page
- [x] Create product grid card component

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
  - [x] **Async processing via Azure Storage Queue + Function App**
    - `POST /upload` uploads original and returns immediately with `status: Pending` (< 500ms)
    - `GET /{mediaId}/status` polls until `Completed` or `Failed`
    - `IMessageQueue<T>` abstraction in Application layer (Service Bus can swap in later)
    - `AzureStorageQueueService<T>` in Infrastructure (plain-text JSON, `QueueMessageEncoding.None`)
    - `MediaProcessingJob` entity tracks per-upload state in PostgreSQL
    - `MediaProcessingStatus` enum: `Pending | Processing | Completed | Failed`
    - `Lyke.Functions` — Azure Functions v4 isolated-worker project
      - `MediaProcessingFunction` queue trigger processes images (ImageSharp) and videos (FFmpeg)
      - Uses `AddDatabase` + `AddStorage` from Infrastructure (no Identity/background services)
      - `host.json` with `messageEncoding: None`; `Microsoft.Azure.Functions.Worker.Sdk` generates `.azurefunctions` metadata at publish
    - `AddDatabase()` extracted from `AddInfrastructure()` so Functions can reuse DB without Identity
    - Connection string resolution reads `ConnectionStrings:X` (Aspire) then falls back to `configuration["X"]` (Azure app settings)
- [x] Create post draft/publish workflow
- [x] Implement product search and tagging
- [x] Build earnings calculation service
- [x] Create analytics aggregation queries

### 7.3 Frontend - Creator Module
- [x] Create creator registration flow
- [x] Create CreatorService (wraps all 13 `/api/creators/v1/` endpoints)
- [x] Build creator dashboard page
  - Profile summary card with verification badge
  - Quick stats row (views, likes, clicks, earnings)
  - Recent posts list with thumbnails and status badges
  - Earnings card with payout threshold progress bar
  - Navigation menu to all creator pages
- [x] Create post creation wizard (4-step)
  - Step 1: Media upload with type selector and preview grid
  - Step 2: Title and description inputs
  - Step 3: Product search, tagging, size/fit rating/notes per product
  - Step 4: Review all data, save as draft or submit for review
- [x] Build post edit page
  - Loads existing post data from route param
  - Pre-fills all fields including tagged products
  - Read-only for Published/PendingReview, editable for Draft/Rejected
  - Displays moderation notes for rejected posts
  - Save changes and submit for review actions
- [x] Build product search and tag component (integrated in post create/edit)
- [x] Create post management page (status filter tabs, swipe-to-delete, FAB create, infinite scroll)
- [x] Build analytics page (date range chips, summary cards, CSS bar chart for daily views, top 5 posts)
- [x] Create earnings page (summary cards, payout threshold, filter tabs, earnings history with infinite scroll)
- [x] Create verification page (status-based UI: submit form, pending review, approved, rejected with resubmit)

---

## Phase 8: Retailer Portal (B2B)

### 8.1 Backend - Retailer APIs
```
POST   /api/retailers/v1/portal/register             - Register retailer ✅
GET    /api/retailers/v1/portal/profile              - Get retailer profile ✅
PUT    /api/retailers/v1/portal/profile              - Update retailer profile ✅
POST   /api/retailers/v1/portal/products/import      - Import product feed (CSV) ✅
GET    /api/retailers/v1/portal/products             - List products ✅
PUT    /api/retailers/v1/portal/products/{id}        - Update product ✅
POST   /api/retailers/v1/portal/campaigns            - Create sponsored campaign ✅
GET    /api/retailers/v1/portal/campaigns            - List campaigns ✅
PUT    /api/retailers/v1/portal/campaigns/{id}       - Update campaign ✅
GET    /api/retailers/v1/portal/analytics            - Get engagement analytics ✅
GET    /api/retailers/v1/portal/analytics/export     - Export analytics CSV ✅
GET    /api/retailers/v1/portal/insights/fit         - Get fit feedback insights ✅
GET    /api/retailers/v1/portal/insights/body-profiles - Get body profile insights ✅
```
Uses `/portal` sub-path to avoid collision with existing public `/api/retailers/v1` routes in CommerceEndpoints.

### 8.2 Backend Tasks
- [x] Create RetailerService (13 methods, ~680 lines)
- [x] Build product feed importer
  - CSV parser (handles quoted fields, row limits)
  - Upsert by (RetailerId, ExternalSku)
  - Validation and error reporting (per-row errors)
- [x] Create sponsored placement service
  - Budget management (max budget validation)
  - Targeting logic (body types, categories as JSON)
  - Computed campaign status (Scheduled/Active/Paused/BudgetExhausted/Ended)
- [x] Build analytics aggregation service
  - Engagement by body profile band (k-anonymity)
  - Fit feedback trends (per-product, per-size breakdown)
  - Category performance (clicks, conversions)
  - Daily metrics with date range filtering
  - Top products ranking
- [x] Implement data anonymization layer (GDPR-compliant body profile insights with k-anonymity, min group size = 5)
- [x] Create CSV export functionality
- [x] Create 14 DTOs, 6 validators, IRetailerService interface
- [x] Wire up DI registration and endpoint mapping
- [x] RetailerSettings configuration (MaxImportRows, MinAnonymityGroupSize, DefaultCurrency, MaxCampaignBudget)

### 8.3 Frontend - Retailer Module
- [x] Create retailer registration page
- [x] Create retailer dashboard (stats cards, quick-link navigation)
- [x] Build retailer profile page (edit company info, affiliate config)
- [x] Build product management interface (product list with search/filter)
- [x] Create product edit page (update product details)
- [x] Create campaign builder (campaign list + create campaign wizard with targeting)
- [x] Build analytics dashboard with Chart.js area chart
  - Segment toggle for views/clicks/conversions/revenue
  - Summary cards with key metrics
- [x] Build fit & body insights page
  - Fit feedback distribution bars
  - Body profile band distribution (with k-anonymity counts)
  - Dark mode compatible styling
- [x] Create CSV export functionality (frontend trigger for backend export endpoint)
- [x] Build product feed CSV upload interface (frontend for backend import endpoint)

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
GET    /api/admin/reports               - Get reported content ✅
GET    /api/admin/reports/{id}          - Get report details ✅
POST   /api/admin/reports/{id}/review   - Review content report ✅
GET    /api/admin/moderation-queue      - Priority moderation queue ✅
POST   /api/admin/posts/bulk-moderate   - Bulk moderate posts ✅
POST   /api/admin/users/bulk-suspend    - Bulk suspend users ✅
POST   /api/posts/{id}/report           - Report post (shopper) ✅
POST   /api/admin/posts/{id}/tags       - Correct product tags
```

### 9.2 Backend Tasks
- [x] Create AdminEndpoints with verification management
- [x] Create AdminService with post moderation, user management, platform stats
- [x] Post moderation tracking (ModeratedByUserId, ModeratedAt)
- [x] User suspension tracking (SuspendedAt, SuspendedByUserId, SuspensionReason)
- [x] Platform statistics aggregation (users, content, verifications, engagement)
- [x] Validators for moderation requests
- [x] Implement content flagging system
  - ContentReport entity with ReportReason/ReportStatus enums
  - Auto-flag posts after configurable report threshold (ModerationSettings)
  - Shopper report endpoint (POST /api/posts/{id}/report)
  - Admin report review workflow (query, view, review with post action)
  - [ ] Duplicate detection (image hashing) - future enhancement
  - [ ] Text analysis for abuse - future enhancement
  - [ ] Product tag validation - future enhancement
- [x] Build moderation queue with priority
  - Priority scoring: flagged(+100), reports≥10(+75), ≥5(+50), HateSpeech/Inappropriate(+30), new creator(+20), >48h queue(+40)
- [x] Create admin audit logging
- [x] Implement bulk actions
  - Bulk moderate posts (approve, reject, remove, flag) - max 100 per request
  - Bulk suspend users - max 100 per request

### 9.3 Frontend - Admin Module
- [x] Create AdminService (17 methods: pending posts, moderate post, users, user detail, suspend/unsuspend, verifications, stats, content reports, moderation queue, bulk moderate, bulk suspend)
- [x] Create admin dashboard (platform stats cards, flagged/reports badges, quick-link navigation with 5 sections, profile nav)
- [x] Build post moderation interface
  - Post list with 6 status segment tabs (All, Pending, Published, Rejected, Flagged, Removed)
  - Approve/reject with feedback input (for PendingReview and Flagged posts)
  - Post detail preview with expandable cards
  - Bulk actions (Approve, Flag, Reject, Remove) with selection mode
- [x] Create user management interface (user list, search, status filtering, bulk suspend with confirmation)
- [x] Create user detail page (profile info, suspension controls, activity)
- [x] Create verification review page (verification queue, approve/reject with notes)
- [x] Create content reports page (segment tabs by status, reason filter chips, expandable detail cards, dismiss/take-action with post moderation options)
- [x] Create moderation queue page (priority-sorted list, color-coded priority scores, flag indicators, report count badges, approve/reject actions)
- [x] Update sidebar navigation with Content Reports and Moderation Queue links
- [x] Build platform analytics dashboard (charts and trends)

---

## Phase 10: Analytics & Event Tracking

### 10.1 Backend Tasks
- [x] Create EventTrackingService
- [x] Define event schema
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
- [x] Implement event batching and async processing
- [x] Create analytics aggregation jobs (background service)
- [x] Build real-time metrics endpoints
- [x] Set up Application Insights / custom analytics

### 10.2 Frontend Tasks
- [x] Create AppInsightsService (Application Insights SDK integration)
- [x] Implement automatic page view tracking (Router NavigationEnd events)
- [x] Add AppInsightsErrorHandler for unhandled exception tracking
- [x] Add HTTP error telemetry in error interceptor
- [x] Implement authenticated user context (setAuthenticatedUser/clearAuthenticatedUser)
- [x] Add engagement event triggers (custom trackEvent calls for likes, saves, shares, unlikes, unsaves, product clicks)
- [x] Add performance monitoring (Core Web Vitals)

---

## Phase 11: Privacy & GDPR Compliance

### 11.1 Backend Tasks
- [x] Implement data export endpoint (GDPR Article 15/20) - `GET /api/privacy/v1/export`
- [x] Create account deletion with full data purge (GDPR Article 17) - cascade delete/anonymize all related data
- [x] Build consent management system - `GET/PUT /api/privacy/v1/consent`
- [x] Create anonymization utilities - click events anonymized on deletion, user record anonymized
- [x] Add consent fields to User entity (PrivacyPolicyAcceptedAt, PrivacyPolicyVersion, MarketingOptIn)
- [x] Record privacy policy acceptance during registration (validator + auto-set on register)
- [x] Data export email notification
- [x] Implement data retention policies (scheduled cleanup of expired data)
- [x] Add audit trail for data access
- [ ] Implement cookie consent tracking (deferred - mobile app, not web)

### 11.2 Frontend Tasks
- [x] Create privacy settings page
- [x] Build consent collection UI (marketing toggle, reconsent banner)
- [x] Implement data export request flow (downloads JSON blob)
- [x] Create account deletion confirmation flow
- [x] Add privacy policy acceptance tracking (checkbox on registration form)

---

## Phase 12: Testing Strategy

### 12.1 Backend Testing
- [x] Unit tests for services (xUnit)
  - [x] AuthServiceTests (registration, login, token refresh, logout)
  - [x] ProfileServiceTests (body profile management)
  - [x] FeedServiceTests (feed generation, similarity matching)
  - [x] CreatorServiceTests (registration, post management, analytics)
  - [x] CommerceServiceTests (click tracking, product search)
  - [x] AdminServiceTests (moderation, suspension, platform stats, bulk actions, analytics)
  - [x] RetailerServiceTests (registration, profile, products, campaigns)
  - [x] PrivacyServiceTests (data export, consent status, consent update)
- [x] Test infrastructure (TestDbContextFactory, MockUserManager)
- [x] Integration tests for API endpoints (100% pass — 105/105; 1 intentionally skipped)
- [ ] Database tests with test containers
- [ ] Load testing with k6 or similar
- [ ] Security testing (OWASP ZAP)

### 12.2 Frontend Testing
- [x] Unit tests for services (Jasmine/Karma — 190 tests across 16 spec files)
  - [x] PostEngagementService, StorageService, ToastService (simple services)
  - [x] ApiService (HTTP method, URL building, param serialization, getData, uploadFile, getPaginated)
  - [x] CreatorService, AdminService, RetailerService, CommerceService (API wrapper services)
  - [x] AuthService (init, login, register, socialLogin, refresh, logout, getAccessToken, signals)
  - [x] AnalyticsService (batching, flush timer, ngOnDestroy)
  - [x] Guards: authGuard, noAuthGuard, roleGuard, creatorGuard, adminGuard, creatorOrAdminGuard
  - [x] Interceptors: errorInterceptor (status codes, toast suppression, AppInsights), authInterceptor (token injection, public endpoints)
  - [x] Karma CI config (ChromeHeadless, ChromeHeadlessCI launcher)
- [ ] Component tests (Angular Testing Library)
- [ ] E2E tests (Cypress or Playwright)
- [ ] Visual regression testing
- [ ] Performance testing (Lighthouse)

### 12.3 Test Coverage Targets
- Backend: 80% code coverage minimum
- Frontend: 70% code coverage minimum
- Critical paths: 100% coverage

---

## Phase 13: Performance & Optimization

### 13.1 Backend Optimization
- [x] Implement response caching (OutputCache for lookup endpoints, MemoryCache for service-level caching)
- [x] Add in-memory caching for:
  - Lookup data (body types, fit tags — 1hr TTL)
  - Retailer list (15min TTL)
  - Fit preferences (static, no DB call)
- [ ] Add Redis caching for:
  - Feed results
  - User profiles
  - Product data
- [x] Optimize EF Core queries — AsNoTracking on all read-only queries, AsSplitQuery for multi-Include queries
- [x] Fix critical N+1 / full-table-load issues (FeedService, AdminService)
- [x] Implement database connection pooling + retry (Npgsql EnableRetryOnFailure, CommandTimeout)
- [x] Push pagination to database (FeedService feed/explore/similar/saved queries)
- [x] Implement request rate limiting

### 13.2 Frontend Optimization
- [x] Implement lazy loading for route modules
- [x] Add image lazy loading (loading="lazy" on ~19 img tags, above-fold kept eager)
- [x] Optimize bundle size (tree shaking, standalone components)
- [ ] Implement virtual scrolling for feeds
- [ ] Add service worker for caching
- [ ] Optimize images (WebP, srcset)

---

## Phase 14: Deployment & DevOps

### 14.1 Infrastructure Setup (Terraform)
- [x] Create Terraform infrastructure-as-code (`infra/` directory)
  - Resource Group (`rg-lyke-{env}`)
  - Log Analytics Workspace (`law-lyke-{env}`)
  - Container Registry (`lyke{env}acr`, Basic SKU, admin enabled)
  - PostgreSQL Flexible Server v16 (`psql-lyke-{env}`) + database + firewall
  - Storage Account (`lyke{env}stor`) + "media" blob container + "media-processing" queue
  - Azure Communication Services (`acs-lyke-{env}`, Europe data location)
  - Key Vault (`kv-lyke-{env}`) + secrets for all sensitive values
  - Container Apps Environment (`cae-lyke-{env}`) + Container App (`ca-lyke-{env}-api`)
- [x] Azure Function App (`lyke-dev-functions-media`) provisioned in dev environment (manual, pending Terraform)
  - Required app settings: `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`, `AzureWebJobsStorage`, `AzureStorage`, `ConnectionStrings__DefaultConnection`
- [x] Configure per-environment tfvars (dev: scale-to-zero B1ms, prod: always-on D2s_v3)
- [x] Map all appsettings.json keys to Container App env vars via `__` convention
- [x] Configure health probes (startup, liveness, readiness on `/health:8080`)
- [x] Validate with `terraform fmt` and `terraform validate`
- [ ] Bootstrap remote state storage (manual one-time setup)
- [ ] Deploy dev environment
- [ ] Deploy prod environment
- [ ] Configure auto-scaling rules (Container Apps HTTP scaling rule)
- [ ] Set up monitoring and alerting
- [ ] Add Redis Cache resource
- [ ] Add CDN for static assets

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
- [ ] GDPR compliance verified (core data export, consent management, cascade deletion implemented)
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

## Phase 16: Creator Payout System

> **Design reference:** `specs/payout-design.md`

### 16.1 Backend - Data Model & Migrations

- [ ] Add `CreatorPayoutAccount` entity (`Lyke.Core`)
  - Fields: `Id`, `CreatorId` (FK, unique), `StripeAccountId` (encrypted), `AccountStatus` (enum), `OnboardingComplete`, `PayoutCurrency`, `CreatedAt`, `UpdatedAt`
- [ ] Add `PayoutRequest` entity (`Lyke.Core`)
  - Fields: `Id`, `CreatorId` (FK), `Amount`, `Currency`, `Status` (enum), `StripeTransferId`, `FailureReason`, `AdminNotes`, `RequestedAt`, `ReviewedAt`, `ProcessedAt`
- [ ] Add `PayoutRequestStatus` enum: `Pending`, `Approved`, `Processing`, `Completed`, `Failed`, `Rejected`
- [ ] Add `StripeAccountStatus` enum: `Pending`, `Active`, `Restricted`, `Suspended`
- [ ] Add `PayoutRequestId` (nullable FK) to `CreatorEarning`
- [ ] Add `CreatorId` (nullable FK) to `SponsoredPlacement`
- [ ] Add EF Core configurations for `CreatorPayoutAccount` and `PayoutRequest`
  - Encrypt/decrypt `StripeAccountId` via value converter
  - Unique index on `CreatorPayoutAccount.CreatorId`
  - Index on `PayoutRequest.CreatorId, Status`
- [ ] Create and apply EF Core migration

### 16.2 Backend - Infrastructure: Encryption & Stripe

- [ ] Add `IEncryptionService` interface to `Lyke.Application`
  - `string Encrypt(string plaintext)` / `string Decrypt(string ciphertext)`
- [ ] Implement `AesEncryptionService` in `Lyke.Infrastructure`
  - AES-256-GCM, key read from Azure Key Vault via existing Key Vault reference in Container App env vars
- [ ] Add `Stripe.net` NuGet package to `Lyke.Infrastructure`
- [ ] Add `PayoutSettings` configuration class to `Lyke.Application.Configuration`
  - `AutoApproveThreshold` (default 200.00), `AttributionWindowDays` (default 30), `StripeSecretKey`, `StripeWebhookSecret`, `ConfirmationJobSchedule`, `ConfirmationBatchSize` (default 500)
- [ ] Register `PayoutSettings` in DI (`appsettings.json` section: `Payout`)
- [ ] Register `IEncryptionService` / `AesEncryptionService` in `AddInfrastructure()`

### 16.3 Backend - Services

- [ ] Add `IPayoutService` interface to `Lyke.Application`
- [ ] Add `IEarningConfirmationService` interface to `Lyke.Application`
- [ ] Implement `PayoutService` in `Lyke.Application.Services`
  - `CreateOnboardingUrlAsync` — create Stripe Express account + AccountLink, persist `CreatorPayoutAccount`
  - `GetPayoutAccountAsync` — return account status from DB (not live Stripe call)
  - `RequestPayoutAsync` — validate verified + account active + balance ≥ threshold + no pending request; bundle `Confirmed` earnings into `PayoutRequest`; auto-approve if ≤ `AutoApproveThreshold`
  - `GetPayoutHistoryAsync` — paginated `PayoutRequest` list for creator
  - `ApprovePayoutAsync` (admin) — set `Approved`, call `ExecuteStripeTransferAsync` internally, set `Processing`
  - `RejectPayoutAsync` (admin) — set `Rejected`, earnings remain `Confirmed`
  - `HandleStripeAccountUpdatedAsync` — update `AccountStatus` / `OnboardingComplete` from Stripe webhook payload
  - `HandleTransferPaidAsync` — set `PayoutRequest.Status = Completed`, set all associated `CreatorEarning.Status = Paid`, send email
  - `HandleTransferFailedAsync` — set `Failed`, revert earnings to `Confirmed`, clear `PayoutRequestId`, send email
- [ ] Implement `EarningConfirmationService` in `Lyke.Application.Services`
  - Query `CreatorEarning` where `Status = Pending` and `CreatedAt < now - AttributionWindowDays`
  - Bulk-update `Status = Confirmed` in batches of `ConfirmationBatchSize`
  - Return count confirmed

### 16.4 Backend - API Endpoints

Creator endpoints (add to `CreatorEndpoints.cs` or new `PayoutEndpoints.cs`):
```
POST   /api/creators/v1/payouts/onboarding    - Create Stripe account, return onboarding URL
GET    /api/creators/v1/payouts/account        - Get payout account status
POST   /api/creators/v1/payouts/request        - Request payout (bundles confirmed earnings)
GET    /api/creators/v1/payouts/history        - Paginated payout request history
```

Admin endpoints (add to `AdminEndpoints.cs`):
```
GET    /api/admin/v1/payouts                   - List payout requests (filter: status, creatorId, date)
POST   /api/admin/v1/payouts/{id}/approve      - Approve payout → triggers Stripe transfer
POST   /api/admin/v1/payouts/{id}/reject       - Reject payout, return earnings to Confirmed
```

Webhook endpoint (new `WebhookEndpoints.cs`):
```
POST   /api/webhooks/stripe                    - Handle Stripe Connect events (signature verified)
```

- [ ] Implement `POST /api/creators/v1/payouts/onboarding` (requires `CreatorOnly` + `VerificationStatus == Approved`)
- [ ] Implement `GET /api/creators/v1/payouts/account`
- [ ] Implement `POST /api/creators/v1/payouts/request`
- [ ] Implement `GET /api/creators/v1/payouts/history`
- [ ] Implement `GET /api/admin/v1/payouts` (requires `AdminOnly`)
- [ ] Implement `POST /api/admin/v1/payouts/{id}/approve`
- [ ] Implement `POST /api/admin/v1/payouts/{id}/reject`
- [ ] Implement `POST /api/webhooks/stripe` (no auth, HMAC verified inside handler)
- [ ] Add FluentValidation validators for all new request DTOs

### 16.5 Backend - Background Jobs (Azure Functions)

- [ ] Add `EarningConfirmationFunction` to `Lyke.Functions`
  - Timer trigger, cron from `PayoutSettings.ConfirmationJobSchedule`
  - Resolves `IEarningConfirmationService` from DI
  - Logs: "Confirmed {n} earnings"
- [ ] Add `AutoPayoutFunction` to `Lyke.Functions` _(Phase 2 — stub only for now)_
  - Timer trigger, 1st of month at 06:00 UTC
  - For each creator with confirmed balance ≥ threshold and Active account, submit `PayoutRequest`
- [ ] Add `PayoutSettings` to Functions host DI registration (`AddPayout()` extension)
- [ ] Add `PAYOUT__STRIPE_SECRET_KEY` and `PAYOUT__STRIPE_WEBHOOK_SECRET` to Azure Functions app settings in Terraform

### 16.6 Backend - Email Notifications

Add Liquid email templates to the existing Azure ACS email service:

- [ ] `payout-requested.liquid` — "Your payout request of £{amount} has been received"
- [ ] `payout-completed.liquid` — "Your payout of £{amount} has been sent to your bank"
- [ ] `payout-failed.liquid` — "Your payout failed. Reason: {reason}. Your earnings remain available."
- [ ] `payout-rejected.liquid` — "Your payout request was not approved. Reason: {reason}"
- [ ] `payout-admin-review.liquid` — Admin notification: new payout request above auto-approve threshold
- [ ] Wire all templates into `PayoutService` at appropriate lifecycle transitions

### 16.7 Backend - Sponsored Earnings (Fixed Fee)

- [ ] Update `SponsoredPlacement` entity with `CreatorId` (nullable FK) and `FeeAmount` / `FeeType` (`FixedFee` | `Cpm`) fields
- [ ] Update `RetailerService.CreateCampaignAsync` to accept `creatorId` and `feeAmount` in campaign creation request
- [ ] In `RetailerService.CreateCampaignAsync`: if `FeeType == FixedFee`, create a `CreatorEarning` of `EarningType.Sponsored` with `Status = Confirmed` immediately (no attribution window)
- [ ] Update campaign creation validator to require `feeAmount > 0` when `creatorId` is provided
- [ ] Extend `GET /api/creators/v1/earnings/summary` response to break out Affiliate vs Sponsored totals

### 16.8 Backend - GDPR Extension

- [ ] On account deletion (`AuthService.DeleteAccountAsync`): call `StripeClient.Accounts.DeleteAsync(stripeAccountId)` if `CreatorPayoutAccount` exists
- [ ] Null out / delete `CreatorPayoutAccount` record on deletion
- [ ] Add Stripe account data to GDPR data export (`GET /api/privacy/v1/export`)

### 16.9 Frontend - Payout Onboarding Page (new)

- [ ] Create `payout-onboarding` page in `features/creator/`
- [ ] Explain what bank connection means and what Stripe collects
- [ ] "Connect bank account" button → `POST /api/creators/v1/payouts/onboarding` → open `onboardingUrl` in Capacitor Browser
- [ ] Handle deep link return: `lyke://payouts/onboarding/complete`
- [ ] Poll `GET /api/creators/v1/payouts/account` every 3s for up to 30s after return; show success/pending state
- [ ] Register route `/creator/payouts/onboarding` with `creatorGuard`
- [ ] Add `PayoutService` to Angular core services (wraps new payout endpoints)

### 16.10 Frontend - Earnings Page (update existing)

- [ ] Add payout account status card at top of earnings page
  - If `onboardingComplete == false`: show "Set up payouts" CTA → navigate to onboarding page
  - If `accountStatus == Restricted`: show warning banner with Stripe dashboard link
  - If `accountStatus == Active`: show "Request payout" button (disabled if not `eligibleForPayout`)
- [ ] "Request payout" → calls `POST /api/creators/v1/payouts/request`; shows toast on success/error
- [ ] Add "Payouts" tab to existing earnings page tab bar (alongside existing history tabs)
- [ ] "Payouts" tab: paginated list of `PayoutRequest` records with status badge and amount

### 16.11 Frontend - Admin Payout Management Page (new)

- [ ] Create `payout-management` page in `features/admin/`
- [ ] Tab bar: "Pending Review" (Status=Pending) / "All"
- [ ] List rows: creator name, amount, currency, requested date, status badge
- [ ] Approve action → confirmation alert with optional notes field → `POST /api/admin/v1/payouts/{id}/approve`
- [ ] Reject action → alert with required reason input → `POST /api/admin/v1/payouts/{id}/reject`
- [ ] Register route `/admin/payouts` with `adminGuard`
- [ ] Add navigation link to admin dashboard

### 16.12 Infrastructure & Configuration

- [ ] Add `Payout__StripeSecretKey` and `Payout__StripeWebhookSecret` secrets to Azure Key Vault (Terraform `azurerm_key_vault_secret`)
- [ ] Map Key Vault secrets to Container App environment variables (Terraform)
- [ ] Add `PAYOUT__ATTRIBUTION_WINDOW_DAYS=30` and `PAYOUT__AUTO_APPROVE_THRESHOLD=200` to Container App env vars
- [ ] Add `AesEncryptionKey` (32-byte base64) to Key Vault
- [ ] Register Stripe webhook in Stripe dashboard pointing to `https://api.lyke.app/api/webhooks/stripe`
- [ ] Update `appsettings.json` template with `Payout` section (placeholder values)

### 16.13 Testing

- [ ] Unit tests: `PayoutServiceTests` (xUnit)
  - Onboarding URL creation (Stripe mocked)
  - Payout request: happy path, insufficient balance, no active account, already pending
  - Auto-approve below threshold
  - Stripe transfer success and failure webhook handlers
  - Earnings reversion on failure
- [ ] Unit tests: `EarningConfirmationServiceTests`
  - Confirms earnings past window, skips earnings within window
  - Batching logic
- [ ] Integration tests: payout endpoints (creator and admin)
- [ ] Frontend unit tests: `PayoutService` spec (wraps API calls)

---

## Phase 17: Affiliate Network Integration (AWIN)

> **Design reference:** `specs/affiliate-network-design.md`
>
> **Dependency:** Phase 16 (Creator Payout System) must be complete before this phase, as it establishes `EarningStatus`, `CreatorEarning`, and the payout flow that this phase modifies.

### 17.1 Commercial Setup (Non-Code Prerequisites)

These steps are done once by LYKE commercially, before any development work begins.

- [ ] Register LYKE as an AWIN Publisher at `ui.awin.com`
- [ ] Record the assigned **Publisher ID** in `AwinSettings.PublisherId` config
- [ ] Apply to first retailer's AWIN programme; record their **Merchant ID** once approved
- [ ] Register postback URL in AWIN publisher dashboard: `https://api.lyke.app/api/webhooks/awin?transaction_id={transaction_id}&order_ref={order_ref}&pence={pence}&clickref={clickref}&merchant_id={merchant}&status={status}&currency={currency}`
- [ ] Obtain AWIN API access token (OAuth2) for reconciliation job

### 17.2 Backend - Data Model & Enum Changes

- [ ] Add `Reversed = 3` to `EarningStatus` enum (`Lyke.Core`)
- [ ] Add `Clawback = 2` to `EarningType` enum (`Lyke.Core`)
- [ ] Add `NetworkType` (`string`, default `"Direct"`) and `MerchantId` (`string?`) fields to `AffiliateConfig` internal class in `CommerceService.cs`
- [ ] Update `AffiliateConfig` JSON documentation comment on `Retailer.AffiliateConfig` to reflect new fields
- [ ] Create EF Core migration for `EarningStatus` enum change (no schema change needed — stored as int — but verify existing data is unaffected)

### 17.3 Backend - Configuration

- [ ] Add `AwinSettings` configuration class to `Lyke.Application.Configuration`
  - `PublisherId` (string), `ApiToken` (string), `AllowedIpRanges` (string, comma-separated CIDRs), `ApiBaseUrl` (default `https://api.awin.com`), `ClawbackThreshold` (decimal, default `10.00`)
- [ ] Register `AwinSettings` in DI (`appsettings.json` section: `Awin`)
- [ ] Add `AWIN__API_TOKEN` secret to Azure Key Vault (Terraform)
- [ ] Map Key Vault secret to Container App environment variable
- [ ] Add `AWIN__PUBLISHER_ID` and `AWIN__ALLOWED_IP_RANGES` to Container App env vars
- [ ] Add `AWIN__PUBLISHER_ID` and `AWIN__API_TOKEN` to Azure Functions app settings (needed by reconciliation job)
- [ ] Update `appsettings.json` template with `Awin` section (placeholder values)

### 17.4 Backend - AWIN Postback Endpoint

- [ ] Create `POST /api/webhooks/awin` endpoint (new `AwinWebhookEndpoints.cs` or add to `WebhookEndpoints.cs`)
  - No JWT auth; IP allowlist middleware validates caller against `AwinSettings.AllowedIpRanges`
  - Accepts query parameters: `transaction_id`, `order_ref`, `pence`, `clickref`, `merchant_id`, `status`, `currency`
  - Always returns `200 OK` (log errors internally; AWIN does not retry on non-200)
  - Delegates to `IAwinService.ProcessPostbackAsync()`
- [ ] Implement IP allowlist middleware/filter (`AwinIpAllowlistFilter`)
  - Reads CIDR ranges from `AwinSettings.AllowedIpRanges`
  - Returns `403` if caller IP is not in any allowed range
- [ ] Add `IAwinService` interface to `Lyke.Application`
- [ ] Implement `AwinService` in `Lyke.Application.Services`
  - `ProcessPostbackAsync`: handle `status=pending` (create earning), `status=confirmed` (promote to Confirmed), `status=declined`/`deleted` (reverse earning)
  - Idempotency: check for existing `CreatorEarning` by `awinTransactionId` before creating
  - Store `awinTransactionId` and `awinMerchantId` in `ClickEvent.AttributionData` JSON
  - Resolve `Retailer` from `merchant_id` via `Retailer.AffiliateConfig` JSON
  - Convert `pence` (int, minor units) to `decimal` commission amount
  - On `Reversed`: if earning is `Paid` and amount > `ClawbackThreshold`, create a `Clawback` earning (negative `Amount`) against the same creator
- [ ] `ReconcileTransactionsAsync`: call AWIN transactions API, diff against local `CreatorEarning` records, apply status updates
- [ ] `HandleReversalAsync`: encapsulate reversal + optional clawback creation logic (shared between postback and reconciliation paths)

### 17.5 Backend - AWIN Reconciliation Job (replaces `EarningConfirmationFunction`)

- [ ] Add `AwinReconciliationFunction` to `Lyke.Functions`
  - Timer trigger, daily at 06:00 UTC
  - Queries AWIN transactions API for all transactions modified in the past 48 hours
  - Calls `IAwinService.ReconcileTransactionsAsync()` with date window
  - Logs: confirmed count, reversed count, skipped count, any unmatched AWIN transactions
- [ ] Disable (or delete) the existing `EarningConfirmationFunction` — time-based confirmation logic is superseded
  - If keeping for non-AWIN ("Direct") retailers, gate it behind a check: only process earnings for retailers where `AffiliateConfig.NetworkType == "Direct"`
- [ ] Register `IAwinService` / `AwinService` in Functions DI (`AddAwin()` extension, alongside existing `AddDatabase()` / `AddStorage()`)

### 17.6 Backend - Retailer Config Updates

- [ ] Update the admin `PUT /api/admin/v1/retailers/{id}` endpoint (or add a dedicated config endpoint) to accept and persist the new `AffiliateConfig` fields: `NetworkType` and `MerchantId`
- [ ] Update the retailer admin UI validator to validate `MerchantId` is present when `NetworkType == "AWIN"`
- [ ] Seed the first retailer's `AffiliateConfig` with correct AWIN values (migration seed or admin action)

### 17.7 Backend - Retain Direct Retailer Webhook

- [ ] Verify `POST /api/retailers/{id}/conversions` still works for non-AWIN retailers (no change needed, but add integration test coverage for the `NetworkType == "Direct"` path)
- [ ] Add guard in `CommerceService.ProcessConversionAsync()`: if `AffiliateConfig.NetworkType != "Direct"`, return a clear error (conversions for AWIN retailers must come through `/api/webhooks/awin`)

### 17.8 Frontend - Creator Earnings UI (minor update)

- [ ] Show estimated confirmation date on pending earnings:
  - Formula: `ClickEvent.CreatedAt + retailer.CookieWindowDays` (surfaced via the earnings history API response)
  - Display as "Expected confirmation: {date}" on each `Pending` earning row
- [ ] Show `Reversed` status on earning rows with appropriate label ("Commission reversed") and muted styling
- [ ] Show `Clawback` earning type with negative amount and explanatory tooltip

### 17.9 Testing

- [ ] Unit tests: `AwinServiceTests` (xUnit)
  - `ProcessPostbackAsync`: pending → creates earning, confirmed → promotes, declined → reverses, duplicate transaction ID is idempotent
  - `HandleReversalAsync`: below clawback threshold (absorb), above threshold (creates Clawback earning)
  - `ReconcileTransactionsAsync`: AWIN API mocked; status diff applied correctly
  - `merchant_id` not found in `Retailer` table → logged and skipped, no exception
  - `clickref` not a valid GUID → logged and skipped
- [ ] Unit tests: `AwinIpAllowlistFilterTests`
  - Allowed IP passes, unknown IP blocked, CIDR range boundary cases
- [ ] Integration tests: `POST /api/webhooks/awin`
  - Valid postback → 200, earning created
  - Unknown merchant → 200 (no crash), earning not created
  - Already-processed transaction → 200, no duplicate earning
  - IP not in allowlist → 403
- [ ] Manual end-to-end test checklist (staging environment):
  - [ ] Click a product link → verify AWIN tracking URL is generated with correct `awinmid`, `awinpid`, `clickref`
  - [ ] Simulate AWIN postback (use AWIN publisher UI test conversion tool) → verify `CreatorEarning` created
  - [ ] Simulate AWIN confirmation postback → verify `EarningStatus` → `Confirmed`
  - [ ] Simulate AWIN declined postback → verify `EarningStatus` → `Reversed`
  - [ ] Run reconciliation job manually → verify no duplicate status updates

---

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
    /Queue              - AzureStorageQueueService<T> (IMessageQueue<T> implementation)
    /Services           - Image/Video processing services
    /Configuration      - Infrastructure settings (AzureBlobSettings, AzureQueueSettings)
  /Lyke.Functions       - Azure Functions isolated-worker (queue trigger for media processing)
    /Functions          - MediaProcessingFunction.cs
    Program.cs          - HostBuilder with AddDatabase + AddStorage
    host.json           - messageEncoding: None
  /Lyke.AppHost         - .NET Aspire orchestration
  /Lyke.ServiceDefaults - Aspire service defaults
/tests
  /Lyke.UnitTests       - Service unit tests (5 test suites)
  /Lyke.IntegrationTests - API integration tests
```

### Frontend (Ionic 8 + Angular 20 Standalone)
```
/frontend/src
  /app
    /core
      /services        - StorageService, ApiService, AuthService, ToastService, CreatorService,
                         CommerceService, AdminService, RetailerService, SocialAuthService, DeepLinkService
      /guards          - authGuard, noAuthGuard, roleGuard (creatorGuard, adminGuard), onboardingGuard
      /interceptors    - authInterceptor, errorInterceptor
    /shared
      /components
        /post-card        - Reusable post card component
        /media-carousel   - Swiper-based carousel (images/video)
        /product-card     - Product display card
        /shop-the-look    - Product listing for outfit posts
        /loading-skeleton - Generic skeleton loader
        /skeleton-*       - Specialized skeletons (post-card, post-detail, profile, list)
    /features
      /auth            - Login, Register, ForgotPassword, ResetPassword
      /onboarding      - Body profile wizard
      /feed            - FeedHome, Explore, Saved, PostDetail, FeedFilterModal
      /profile         - ProfileView, ProfileEdit, BodyProfileEdit
      /settings        - Settings, Privacy, DeleteAccount
      /creator         - Dashboard, Posts, PostCreate, PostEdit, Analytics, Earnings, Verification, Register
      /commerce        - ProductDetail, ProductSearch, RetailerList, RetailerStorefront, ProductGridCard
      /admin           - AdminDashboard, PostModeration, UserManagement, UserDetail, VerificationReview, ContentReports, ModerationQueue
      /retailer        - RetailerDashboard, RetailerProfile, Products, ProductEdit, Campaigns, CampaignCreate, RetailerAnalytics, Insights, Register
    /models
      /enums           - All 11 enums
      /api             - ApiResponse, PaginationMeta
      /auth            - Auth DTOs
      /profile         - Profile DTOs
      /feed            - Feed DTOs
      /commerce        - Commerce DTOs
      /creator         - Creator DTOs
      /media           - Media DTOs
      /admin           - Admin DTOs
    app.routes.ts      - Lazy-loaded routing with guards
  /assets
  /environments        - environment.ts, environment.prod.ts
  /theme
```

---

## Appendix C: Environment Variables

### Backend (API)
```
ConnectionStrings__DefaultConnection=postgresql://user:pass@host:5432/lyke
Jwt__Secret=<secret>
Jwt__ExpiryMinutes=60
Jwt__RefreshTokenExpiryDays=30
ConnectionStrings__AzureStorage=<storage-connection-string>   # Aspire injects this
REDIS_CONNECTION=<connection-string>
SocialAuth__GoogleClientId=<client-id>
SocialAuth__AppleAppId=<client-id>
```

### Azure Functions (lyke-dev-functions-media)
```
FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
AzureWebJobsStorage=<storage-connection-string>      # Required by Functions host
AzureStorage=<storage-connection-string>             # Queue trigger + blob/queue clients
ConnectionStrings__DefaultConnection=<postgres-conn> # Double underscore = nested config
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

| Phase | Tasks | Completed | Priority | Status |
|-------|-------|-----------|----------|--------|
| Phase 1: Foundation | 18 | 16 | Critical | 89% (CI test step + env deployments remaining) |
| Phase 2: Database | 8 | 8 | Critical | 100% |
| Phase 3: Authentication | 17 | 17 | Critical | 100% (social login on hold pending OAuth creds) |
| Phase 3B: Email & Account Mgmt | 20 | 19 | Critical | 95% (email retry logic remaining) |
| Phase 4: User Profile | 18 | 18 | Critical | 100% |
| Phase 5: Content Feed | 25 | 25 | Critical | 100% |
| Phase 6: Commerce | 18 | 18 | Critical | 100% |
| Phase 7: Creator | 30 | 30 | High | 100% (async media processing via queue + Functions added and complete) |
| Phase 8: Retailer Portal | 22 | 22 | High | 100% |
| Phase 9: Admin | 22 | 22 | High | 100% |
| Phase 10: Analytics | 12 | 12 | Medium | 100% |
| Phase 11: Privacy | 12 | 11 | Critical | ~95% (cookie consent deferred — mobile app) |
| Phase 12: Testing | 10 | 9 | High | 95% (120 backend unit tests + 190 frontend unit tests pass, 105/105 integration tests pass; CI test step remaining) |
| Phase 13: Performance | 14 | 11 | Medium | ~79% (backend optimization done, Redis/virtual scroll/service worker/WebP remaining) |
| Phase 14: Deployment | 17 | 5 | High | 29% (Terraform IaC complete, deploy + monitoring remaining) |
| Phase 15: Launch | 8 | 0 | Critical | 0% |
| Phase 16: Creator Payouts | 53 | 0 | High | 0% |
| Phase 17: Affiliate Network (AWIN) | 38 | 0 | High | 0% (commercial prerequisites first) |

**Total: ~344 actionable tasks (~238 completed, ~69% overall)**

---

## Recommended MVP Sequence

1. **Foundation** (Phases 1-2): Project setup, database, basic infrastructure
2. **Core User Journey** (Phases 3-5): Auth, profiles, feed viewing
3. **Commerce** (Phase 6): Click tracking, attribution
4. **Content Creation** (Phase 7): Creator posting workflow
5. **Compliance & Launch** (Phases 11, 15): GDPR, testing, deployment

Retailer portal (Phase 8) and Admin tools (Phase 9) can run in parallel with a smaller team or be delivered incrementally.
