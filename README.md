# LYKE

A fashion social mobile application that helps shoppers discover outfit content from people with similar body profiles. LYKE reduces purchase uncertainty and multi-size ordering/returns while providing anonymized fit insights to retailers.

## How It Works

1. **Shoppers** create a body profile (height, weight, body type, fit preference) and browse a personalized feed of outfit posts from creators with similar measurements.
2. **Creators** post outfit content, tag the products they're wearing, and provide fit feedback (size worn, fit rating, styling notes). They earn affiliate commissions from clicks.
3. **Retailers** list products, run sponsored campaigns with body-type targeting, and access aggregated (GDPR-anonymized) fit insights to improve sizing and reduce returns.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | C# / ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL 16 with full-text search |
| Frontend | Ionic 8 + Angular 20 (standalone components) |
| Mobile | Capacitor 8 (Android, iOS planned) |
| Auth | ASP.NET Core Identity + JWT + social login (Google, Apple) |
| Storage | Azure Blob Storage (Azurite for local dev) |
| Email | Azure Communication Services + Liquid templates |
| Orchestration | .NET Aspire |
| CI/CD | GitHub Actions |

## Project Structure

```
lyke/
├── backend/
│   ├── src/
│   │   ├── Lyke.Api/              # Minimal API endpoints, middleware
│   │   ├── Lyke.Core/             # Domain entities, interfaces, enums
│   │   ├── Lyke.Application/      # Services, DTOs, validators
│   │   └── Lyke.Infrastructure/   # EF Core, repositories, storage
│   └── tests/
│       ├── Lyke.UnitTests/
│       └── Lyke.IntegrationTests/
├── frontend/                      # Ionic 8 + Angular 20 app
│   └── src/app/
│       ├── core/                  # Services, guards, interceptors
│       ├── shared/                # Reusable components
│       ├── features/              # Feature modules
│       └── models/                # TypeScript interfaces & enums
├── aspire/
│   ├── Lyke.AppHost/              # Aspire orchestration (Postgres, Azurite, API)
│   └── Lyke.ServiceDefaults/      # Shared service configuration
├── specs/                         # Requirements & implementation plan
└── .github/workflows/             # CI/CD pipelines
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and npm
- [PostgreSQL 16](https://www.postgresql.org/) (or use Aspire, which runs it in Docker)
- [Docker](https://www.docker.com/) (for Aspire local development)
- [Android Studio](https://developer.android.com/studio) (for mobile builds, optional)

## Getting Started

### Option A: Using .NET Aspire (recommended)

Aspire orchestrates PostgreSQL, Azurite (blob storage emulator), and the API together:

```bash
cd aspire/Lyke.AppHost
dotnet run
```

This starts:
- **PostgreSQL** with PgAdmin on port 56652
- **Azurite** (blob emulator) on ports 27000-27002
- **Lyke.Api** on port 5104

The Aspire dashboard opens automatically and shows the health of all services.

### Option B: Manual setup

1. **Start PostgreSQL** and create a database:
   ```sql
   CREATE DATABASE lyke;
   ```

2. **Configure the backend** &mdash; update `backend/src/Lyke.Api/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=lyke;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Run database migrations:**
   ```bash
   dotnet ef database update \
     --project backend/src/Lyke.Infrastructure \
     --startup-project backend/src/Lyke.Api
   ```

4. **Start the API:**
   ```bash
   dotnet run --project backend/src/Lyke.Api
   ```
   The API is available at `https://localhost:7150` (HTTPS) or `http://localhost:5104` (HTTP).

### Frontend

```bash
cd frontend
npm install
npm start
```

The app is served at `http://localhost:4200` and proxies API requests to the backend.

### Android Build

```bash
cd frontend
npm run build
npx cap sync android
npx cap open android    # opens Android Studio
```

## Build & Test

### Backend

```bash
# Build the solution
dotnet build backend/Lyke.slnx

# Run all tests
dotnet test backend/Lyke.slnx

# Run unit tests only
dotnet test backend/tests/Lyke.UnitTests

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> \
  --project backend/src/Lyke.Infrastructure \
  --startup-project backend/src/Lyke.Api
```

### Frontend

```bash
cd frontend
npm run build      # production build (output: www/)
npm test           # unit tests (Karma + Jasmine)
npm run lint       # ESLint
```

## API Overview

All endpoints return a consistent JSON envelope:

```json
{
  "success": true,
  "data": { ... },
  "meta": { "page": 1, "pageSize": 20, "totalCount": 100 }
}
```

### Endpoint Groups

| Group | Base Path | Auth | Endpoints |
|---|---|---|---|
| Auth | `/api/auth/v1` | Public | Register, login, refresh, logout, password reset, social login, account delete |
| Profile | `/api/profile/v1` | User | Get/update profile, body profile CRUD, lookups |
| Feed | `/api/feed/v1` | Public/User | Personalized feed, explore, post detail, search, engagements, saved posts |
| Commerce | `/api/commerce/v1` | Varies | Click tracking, product search, retailers, conversions |
| Creator | `/api/creators/v1` | Creator | Post CRUD, submit for review, analytics, earnings |
| Media | `/api/media/v1` | Creator | Upload images/video, bulk upload, delete |
| Retailer Portal | `/api/retailers/v1/portal` | Retailer | Profile, products, CSV import, campaigns, analytics, fit insights |
| Admin | `/api/admin/v1` | Admin | User management, post moderation, creator verification, platform stats |
| Privacy | `/api/privacy/v1` | User | Data export, consent management |

## Key Features

- **Body profile matching** &mdash; Feed ranked by similarity (height, weight, body type) with configurable weights
- **Full-text search** &mdash; PostgreSQL GIN indexes on posts, products, and creators with relevance ranking
- **Fit feedback system** &mdash; Per-product size worn, fit rating, fit tags, and styling notes
- **Affiliate commerce** &mdash; Click tracking with attribution, per-retailer affiliate URL generation, conversion webhooks
- **Creator earnings** &mdash; Affiliate and sponsored placement earnings with payout tracking
- **GDPR compliance** &mdash; Data export, account deletion with cascade, consent management, anonymized retailer insights (k-anonymity)
- **Content moderation** &mdash; Draft/review/publish workflow with admin moderation queue
- **Media processing** &mdash; Image resizing (ImageSharp), video thumbnails (FFmpeg), SAS token access
- **Rate limiting** &mdash; Request rate limiting on API and auth endpoints
- **Account lockout** &mdash; Automatic lockout after repeated failed login attempts
- **Email notifications** &mdash; Liquid-templated transactional emails via Azure Communication Services

## Configuration

Key configuration sections in `appsettings.json`:

| Section | Purpose |
|---|---|
| `ConnectionStrings` | PostgreSQL connection |
| `Jwt` | Token signing key, issuer, audience, expiry |
| `Matching` | Body profile similarity algorithm weights |
| `Commerce` | Commission share, attribution window |
| `Creator` | Post/media limits, payout threshold |
| `Retailer` | CSV import limits, anonymity group size, campaign budget cap |
| `MediaUpload` | File size limits, allowed MIME types, image dimensions |
| `SocialAuth` | Google and Apple OAuth client IDs |
| `AzureBlob` | Blob storage connection and container config |
| `Email` | Azure Communication Services sender and rate limits |
| `Cors` | Allowed origins |

## CI/CD

Two GitHub Actions workflows run on push to `develop`:

- **Backend** (`develop_lyke-dev-api.yml`) &mdash; Build, run unit tests, deploy to Azure Web App
- **Android** (`build-android.yml`) &mdash; Build Angular, run frontend tests, sync Capacitor, build debug APK artifact

## Documentation

- [`specs/requirements.md`](specs/requirements.md) &mdash; Product requirements
- [`specs/implementation-plan.md`](specs/implementation-plan.md) &mdash; Detailed phased implementation plan with task tracking
- [`CLAUDE.md`](CLAUDE.md) &mdash; AI assistant context for working with this codebase
