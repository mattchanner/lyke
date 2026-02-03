# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LYKE is a fashion social mobile application that helps shoppers view outfit content from people with similar body profiles. The app reduces purchase uncertainty and multi-size ordering/returns while providing insights to retailers.

**Status**: Backend foundation implemented, frontend not yet started.

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | C# ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core 9.0 |
| Database | PostgreSQL |
| Frontend | Ionic 7 + Angular 17 (planned) |
| Auth | ASP.NET Core Identity + JWT |
| Storage | Azure Blob Storage / AWS S3 (planned) |
| Caching | Redis (planned) |

## Architecture

### Backend Structure
```
/src
  /Lyke.Api              - Web API (controllers, middleware, filters)
  /Lyke.Core             - Domain layer (entities, interfaces, enums, exceptions)
  /Lyke.Application      - Application layer (services, DTOs, validators)
  /Lyke.Infrastructure   - Infrastructure (EF Core, repositories, external services)
/tests
  /Lyke.UnitTests
  /Lyke.IntegrationTests
```

### Project Dependencies
```
Lyke.Api → Lyke.Application, Lyke.Infrastructure
Lyke.Application → Lyke.Core
Lyke.Infrastructure → Lyke.Core, Lyke.Application
```

## Build Commands

```bash
# Build solution
dotnet build Lyke.slnx

# Run tests
dotnet test Lyke.slnx

# Run API
dotnet run --project src/Lyke.Api

# EF Core migrations
dotnet ef migrations add <name> --project src/Lyke.Infrastructure --startup-project src/Lyke.Api
dotnet ef database update --project src/Lyke.Infrastructure --startup-project src/Lyke.Api
```

## Key Domain Entities

- **User** - Extends IdentityUser with UserType (Shopper, Creator, Retailer, Admin)
- **BodyProfile** - Height, weight, body type, fit preference for matching
- **Creator** - Extended profile for content creators with earnings
- **Post** - Outfit content with media and status workflow
- **PostProduct** - Links posts to products with fit feedback
- **Product** - Retailer products with SKU and pricing
- **Retailer** - B2B partners with affiliate configuration
- **ClickEvent** - Attribution tracking for commerce
- **SponsoredPlacement** - Retailer ad campaigns

## API Response Format

```json
{
  "success": true,
  "data": { },
  "meta": { "page": 1, "pageSize": 20, "totalCount": 100 }
}
```

Error responses use the same structure with `success: false` and an `error` object containing `code`, `message`, and optional `details`.

## Privacy Requirements

- GDPR-first design
- Body profile data never shared directly with retailers
- All retailer insights must be aggregated and anonymized
- Profile displays show ranges/bands, not exact measurements

## Configuration

Key settings in `appsettings.json`:
- `ConnectionStrings:DefaultConnection` - PostgreSQL connection
- `Jwt:Secret` - JWT signing key (min 32 chars)
- `Jwt:Issuer`, `Jwt:Audience` - Token validation
- `Cors:AllowedOrigins` - Mobile app origins

## Reference Documents

- `specs/requirements.md` - Product requirements
- `specs/implementation-plan.md` - Detailed implementation plan with phases and tasks
