# Role-Based Authorization Implementation Plan

## Summary

Add policy-based authorization to protect endpoints by UserType (Shopper, Creator, Retailer, Admin). The JWT infrastructure already includes a `user_type` claim, so this implementation leverages existing claims for stateless authorization.

## Current State

### What Exists
- JWT authentication fully configured (`Program.cs:33-57`)
- `user_type` claim already included in JWT tokens (`AuthService.cs:244`)
- `UserType` enum: Shopper, Creator, Retailer, Admin (`UserType.cs`)
- Generic `RequireAuthorization()` on endpoint groups

### What's Missing
- No authorization policies defined (just `builder.Services.AddAuthorization()`)
- Creator endpoints accessible to ANY authenticated user
- No Admin-only protection (admin endpoints don't exist yet)
- No Retailer-only protection

## Implementation Plan

### Step 1: Define Authorization Policies

**File:** `backend/src/Lyke.Api/Program.cs`

Replace the generic authorization registration with policy definitions:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CreatorOnly", policy =>
        policy.RequireClaim("user_type", "Creator"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("user_type", "Admin"));

    options.AddPolicy("RetailerOnly", policy =>
        policy.RequireClaim("user_type", "Retailer"));

    options.AddPolicy("CreatorOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "user_type" &&
                (c.Value == "Creator" || c.Value == "Admin"))));
});
```

### Step 2: Apply CreatorOnly Policy to Creator Endpoints

**File:** `backend/src/Lyke.Api/Endpoints/CreatorEndpoints.cs`

Change line 15 from:
```csharp
.RequireAuthorization();
```
To:
```csharp
.RequireAuthorization("CreatorOnly");
```

### Step 3: Handle Registration Edge Case

The `/api/creators/register` endpoint needs special handling - a Shopper registering as Creator won't have the Creator claim yet.

**Option A (Recommended):** Use a separate policy for registration
```csharp
group.MapPost("/register", RegisterAsCreatorAsync)
    .RequireAuthorization()  // Any authenticated user
    .WithName("RegisterAsCreator");
```

**Option B:** Keep registration under generic auth, apply CreatorOnly to other endpoints individually.

### Step 4: Update Product Search Authorization

**File:** `backend/src/Lyke.Api/Endpoints/CommerceEndpoints.cs`

Product search is used by creators to tag products. Update to allow Creators:
```csharp
productGroup.MapGet("/search", SearchProductsAsync)
    .RequireAuthorization("CreatorOrAdmin");
```

### Step 5: Add Service-Level Authorization (Defense in Depth)

**File:** `backend/src/Lyke.Application/Services/CreatorService.cs`

Add UserType validation in `RegisterAsCreatorAsync` (already exists - validates user isn't already a creator).

Consider adding a helper method for services that need to verify the caller is the correct type.

---

## Files to Modify

| File | Change |
|------|--------|
| `backend/src/Lyke.Api/Program.cs` | Add authorization policies (line ~59) |
| `backend/src/Lyke.Api/Endpoints/CreatorEndpoints.cs` | Apply CreatorOnly policy, handle registration |
| `backend/src/Lyke.Api/Endpoints/CommerceEndpoints.cs` | Update product search policy |

## Files to Create (Optional)

| File | Purpose |
|------|---------|
| `backend/src/Lyke.Api/Authorization/Policies.cs` | Constants for policy names (clean code) |

---

## Detailed Changes

### Program.cs Changes

**Location:** Around line 59, replace:
```csharp
builder.Services.AddAuthorization();
```

With:
```csharp
builder.Services.AddAuthorization(options =>
{
    // Role-based policies using user_type claim from JWT
    options.AddPolicy("CreatorOnly", policy =>
        policy.RequireClaim("user_type", "Creator"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("user_type", "Admin"));

    options.AddPolicy("RetailerOnly", policy =>
        policy.RequireClaim("user_type", "Retailer"));

    // Combined policies for flexibility
    options.AddPolicy("CreatorOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "user_type" &&
                (c.Value == "Creator" || c.Value == "Admin"))));

    options.AddPolicy("RetailerOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "user_type" &&
                (c.Value == "Retailer" || c.Value == "Admin"))));
});
```

### CreatorEndpoints.cs Changes

**Change 1:** Update group authorization (line 13-15)
```csharp
var group = app.MapGroup("/api/creators")
    .WithTags("Creators")
    .RequireAuthorization("CreatorOnly");
```

**Change 2:** Override registration to allow any authenticated user (line 18)
```csharp
group.MapPost("/register", RegisterAsCreatorAsync)
    .RequireAuthorization()  // Override: any auth user can register
    .WithName("RegisterAsCreator")
    // ... rest of chain
```

### CommerceEndpoints.cs Changes

**Change:** Update product search (around line 49)
```csharp
productGroup.MapGet("/search", SearchProductsAsync)
    .RequireAuthorization("CreatorOrAdmin")
    .WithSummary("Search products (creators only)");
```

---

## Verification

### Build
```bash
dotnet build backend/Lyke.slnx
```

### Manual Testing

1. **Login as Shopper** - should get 403 on `/api/creators/profile`
2. **Login as Shopper** - should get 200 on `/api/creators/register`
3. **Login as Creator** - should get 200 on all `/api/creators/*` endpoints
4. **Login as Creator** - should get 200 on `/api/products/search`
5. **Login as Admin** - should get 200 on product search (CreatorOrAdmin)

### Test with curl
```bash
# Get token as Shopper
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"shopper@test.com","password":"Test123!"}' | jq -r '.data.accessToken')

# Should fail (403 Forbidden)
curl -X GET http://localhost:5000/api/creators/profile \
  -H "Authorization: Bearer $TOKEN"

# Should succeed (can register)
curl -X POST http://localhost:5000/api/creators/register \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Test Creator"}'
```

---

## Future Enhancements

1. **Admin Endpoints** - Create admin endpoint group with `AdminOnly` policy
2. **Retailer Endpoints** - Create retailer management with `RetailerOnly` policy
3. **Custom Authorization Handler** - For complex rules (e.g., user must be active AND correct type)
4. **Authorization Logging** - Track failed authorization attempts

---

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Token caching after role change | Document that users must re-login after becoming Creator |
| Breaking existing integrations | The change is additive; generic auth still works for registration |
| Policy name typos | Consider creating a constants class for policy names |
