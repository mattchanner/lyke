# Authentication

All LYKE users share common authentication workflows regardless of their role.

## Registration

### Standard Registration

**Endpoint:** `POST /api/auth/v1/register`

Users create an account with email and password:

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "userType": "Shopper"
}
```

**User Types:**
- `Shopper` (0) - Default for consumer accounts
- `Creator` (1) - Content creators (can also register as Shopper first, then upgrade)
- `Retailer` (2) - B2B partner accounts
- `Admin` (3) - Platform operators (created internally)

**Response:**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@example.com",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh-token-guid",
    "expiresAt": "2024-01-15T12:00:00Z"
  }
}
```

### Social Login

**Endpoint:** `POST /api/auth/v1/social-login`

Login or register using OAuth providers:

```json
{
  "provider": "Google",
  "idToken": "google-oauth-id-token"
}
```

**Supported Providers:**
- Google
- Apple

If the social account email doesn't exist, a new account is created automatically.

## Login

### Email/Password Login

**Endpoint:** `POST /api/auth/v1/login`

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "user@example.com",
    "userType": "Shopper",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh-token-guid",
    "expiresAt": "2024-01-15T12:00:00Z"
  }
}
```

**JWT Claims Included:**
- `sub` - User ID
- `email` - User email
- `user_type` - User role (Shopper, Creator, Retailer, Admin)

## Token Management

### Refresh Token

**Endpoint:** `POST /api/auth/v1/refresh`

Access tokens expire after a short period. Use the refresh token to obtain a new access token:

```json
{
  "refreshToken": "refresh-token-guid"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2024-01-15T14:00:00Z"
  }
}
```

### Logout

**Endpoint:** `POST /api/auth/v1/logout`

Invalidates the current refresh token:

```json
{
  "refreshToken": "refresh-token-guid"
}
```

## Password Management

### Forgot Password

**Endpoint:** `POST /api/auth/v1/forgot-password`

Request a password reset email:

```json
{
  "email": "user@example.com"
}
```

A reset token is sent to the user's email.

### Reset Password

**Endpoint:** `POST /api/auth/v1/reset-password`

Complete the password reset:

```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123!"
}
```

## Account Deletion (GDPR)

**Endpoint:** `DELETE /api/auth/v1/account`

**Requires:** Authentication

Permanently delete the user's account and associated data:

**Data Actions:**
- User account deactivated (`IsActive = false`)
- Body profile deleted (cascade)
- Creator profile deleted if applicable (cascade)
- All engagements deleted
- Click events anonymized (UserId set to null, metrics preserved)
- Posts remain with anonymized creator reference

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Account successfully deleted"
  }
}
```

## Authorization Policies

The API uses role-based authorization with the following policies:

| Policy | Required Claim |
|--------|---------------|
| `CreatorOnly` | `user_type` = "Creator" |
| `RetailerOnly` | `user_type` = "Retailer" |
| `AdminOnly` | `user_type` = "Admin" |
| `CreatorOrAdmin` | `user_type` = "Creator" OR "Admin" |
| `RetailerOrAdmin` | `user_type` = "Retailer" OR "Admin" |

## Error Responses

### Invalid Credentials
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password"
  }
}
```

### Account Suspended
```json
{
  "success": false,
  "error": {
    "code": "ACCOUNT_SUSPENDED",
    "message": "Your account has been suspended",
    "details": {
      "reason": "Violation of community guidelines",
      "suspendedAt": "2024-01-10T10:00:00Z"
    }
  }
}
```

### Token Expired
```json
{
  "success": false,
  "error": {
    "code": "TOKEN_EXPIRED",
    "message": "Access token has expired"
  }
}
```
