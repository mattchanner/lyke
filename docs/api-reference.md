# API Reference

Complete reference of all LYKE API endpoints.

## Base URL

```
https://api.be-lyke.clothing/api
```

## Authentication

All authenticated endpoints require a Bearer token in the Authorization header:

```
Authorization: Bearer <access_token>
```

## Response Format

### Success Response
```json
{
  "success": true,
  "data": { },
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100
  }
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": { }
  }
}
```

## Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `VALIDATION_ERROR` | 400 | Invalid request data |
| `UNAUTHORIZED` | 401 | Missing or invalid token |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource conflict (e.g., duplicate) |
| `RATE_LIMITED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Server error |

---

## Authentication Endpoints

Base path: `/api/auth/v1`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Create new account | No |
| POST | `/login` | Login with email/password | No |
| POST | `/refresh` | Refresh access token | No |
| POST | `/logout` | Invalidate refresh token | No |
| POST | `/forgot-password` | Request password reset | No |
| POST | `/reset-password` | Complete password reset | No |
| POST | `/social-login` | OAuth login (Google/Apple) | No |
| DELETE | `/account` | Delete account (GDPR) | Yes |

---

## Profile Endpoints

Base path: `/api/profile/v1`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/me` | Get current user profile | Yes |
| PUT | `/` | Update profile | Yes |
| GET | `/body` | Get body profile | Yes |
| POST | `/body` | Create body profile | Yes |
| PUT | `/body` | Update body profile | Yes |
| DELETE | `/body` | Delete body profile | Yes |
| POST | `/me/image` | Upload profile image | Yes |
| DELETE | `/me/image` | Delete profile image | Yes |

---

## Feed Endpoints

Base path: `/api/feed/v1`

| Method | Endpoint | Description | Auth | Query Params |
|--------|----------|-------------|------|--------------|
| GET | `/` | Personalized feed | Yes | page, pageSize, category |
| GET | `/explore` | Explore/trending feed | Yes | page, pageSize, sortBy |
| GET | `/search` | Search posts/products/creators | Yes | query, type, page, pageSize |

---

## Post Endpoints

Base path: `/api/posts/v1`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get post details | Yes |
| GET | `/{id}/similar` | Get similar posts | Yes |
| POST | `/{id}/engage` | Record engagement | Yes |
| DELETE | `/{id}/engage` | Remove engagement | Yes |
| GET | `/saved` | Get saved posts | Yes |
| POST | `/{id}/report` | Report content | Yes |

---

## Creator Endpoints

Base path: `/api/creators/v1`

**Authorization Policy:** `CreatorOnly` (except registration)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register as creator | Yes |
| GET | `/profile` | Get creator profile | CreatorOnly |
| PUT | `/profile` | Update creator profile | CreatorOnly |
| GET | `/verification` | Get verification status | CreatorOnly |
| POST | `/verification` | Submit verification request | CreatorOnly |
| POST | `/posts` | Create new post | CreatorOnly |
| GET | `/posts` | List creator's posts | CreatorOnly |
| GET | `/posts/{id}` | Get post details | CreatorOnly |
| PUT | `/posts/{id}` | Update post (draft only) | CreatorOnly |
| DELETE | `/posts/{id}` | Delete post | CreatorOnly |
| POST | `/posts/{id}/submit` | Submit for review | CreatorOnly |
| GET | `/analytics` | Performance analytics | CreatorOnly |
| GET | `/earnings` | Earnings summary | CreatorOnly |
| GET | `/earnings/history` | Detailed earnings history | CreatorOnly |

---

## Commerce Endpoints

Base path: `/api/commerce/v1`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/clicks/track` | Track product click | Yes |
| GET | `/products/{id}` | Get product details | Yes |
| GET | `/products/{id}/posts` | Get posts for product | Yes |
| GET | `/retailers/` | List retailers | Yes |
| GET | `/retailers/{id}/products` | Get retailer products | Yes |
| POST | `/retailers/{id}/conversions` | Report conversion (webhook) | RetailerOnly |

---

## Retailer Endpoints

Base path: `/api/retailers/v1/portal`

**Authorization Policy:** `RetailerOnly`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register as retailer | Yes |
| GET | `/profile` | Get retailer profile | RetailerOnly |
| PUT | `/profile` | Update retailer profile | RetailerOnly |
| POST | `/products/import` | Bulk import products (CSV) | RetailerOnly |
| GET | `/products` | List products | RetailerOnly |
| PUT | `/products/{id}` | Update product | RetailerOnly |
| POST | `/campaigns` | Create campaign | RetailerOnly |
| GET | `/campaigns` | List campaigns | RetailerOnly |
| PUT | `/campaigns/{id}` | Update campaign | RetailerOnly |
| GET | `/analytics` | Engagement analytics | RetailerOnly |
| GET | `/analytics/export` | Export analytics (CSV/XLSX) | RetailerOnly |
| GET | `/insights/fit` | Fit feedback insights | RetailerOnly |
| GET | `/insights/body-profiles` | Anonymized body profile insights | RetailerOnly |

---

## Admin Endpoints

Base path: `/api/admin/v1`

**Authorization Policy:** `AdminOnly`

### Creator Verification

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/verifications` | List verification requests |
| GET | `/verifications/{creatorId}` | Get verification details |
| POST | `/verifications/{creatorId}/review` | Approve/reject verification |

### Post Moderation

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/posts` | List posts for moderation |
| GET | `/posts/{postId}` | Get post for moderation |
| POST | `/posts/{postId}/moderate` | Moderate post |
| POST | `/posts/bulk-moderate` | Bulk moderate posts |

### User Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users` | List users |
| GET | `/users/{userId}` | Get user details |
| POST | `/users/{userId}/suspend` | Suspend user |
| POST | `/users/{userId}/unsuspend` | Unsuspend user |
| POST | `/users/bulk-suspend` | Bulk suspend users |

### Content Reports

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/reports` | List content reports |
| GET | `/reports/{reportId}` | Get report details |
| POST | `/reports/{reportId}/review` | Review report |

### Platform Monitoring

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/stats` | Platform statistics |
| GET | `/analytics` | Platform analytics |
| GET | `/moderation-queue` | Priority moderation queue |
| GET | `/audit-logs` | Audit trail |

---

## Lookup Endpoints

Base path: `/api/lookup/v1`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/body-types` | List body types | No |
| GET | `/fit-preferences` | List fit preferences | No |
| GET | `/fit-tags` | List fit tags | No |

---

## Pagination

Paginated endpoints accept:

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `page` | int | 1 | - | Page number (1-indexed) |
| `pageSize` | int | 20 | 50 | Items per page |

Response includes `meta` object:

```json
{
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

---

## Rate Limits

| Endpoint Category | Limit |
|-------------------|-------|
| Authentication | 10/minute |
| Read operations | 100/minute |
| Write operations | 30/minute |
| File uploads | 10/minute |
| Webhooks | 100/minute |

Rate limit headers:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1705329600
```

---

## Enums

### UserType
| Value | Description |
|-------|-------------|
| 0 | Shopper |
| 1 | Creator |
| 2 | Retailer |
| 3 | Admin |

### PostStatus
| Value | Description |
|-------|-------------|
| Draft | Not yet submitted |
| PendingReview | Awaiting moderation |
| Published | Live in feeds |
| Rejected | Rejected by moderator |
| Flagged | Flagged by users |
| Removed | Removed by admin |

### VerificationStatus
| Value | Description |
|-------|-------------|
| NotSubmitted | No verification request |
| Pending | Awaiting review |
| Approved | Verified |
| Rejected | Verification denied |

### FitRating
| Value | Description |
|-------|-------------|
| TooSmall | Runs very small |
| SlightlySmall | Runs a bit small |
| TrueToSize | Fits as expected |
| SlightlyLarge | Runs a bit large |
| TooLarge | Runs very large |

### FitPreference
| Value | Description |
|-------|-------------|
| Fitted | Prefers form-fitting |
| Regular | Standard fit |
| Relaxed | Prefers loose fit |

### EngagementType
| Value | Description |
|-------|-------------|
| View | Post viewed |
| Like | Post liked |
| Save | Post saved |
| Share | Post shared |

### EarningType
| Value | Description |
|-------|-------------|
| Affiliate | Commission from sales |
| Sponsored | Sponsored content payment |

### EarningStatus
| Value | Description |
|-------|-------------|
| Pending | Awaiting confirmation |
| Confirmed | Confirmed, awaiting payout |
| Paid | Paid out |
