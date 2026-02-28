# Creator Payout System Design

**Status:** Proposed
**Created:** 2026-02-28
**Updated:** 2026-02-28 — Payment schedule and earning confirmation updated to reflect AWIN affiliate network integration (see `specs/affiliate-network-design.md`). Earning confirmation is now driven by AWIN transaction status, not elapsed time. Payout eligibility requires AWIN payment receipt. Clawback mechanism added.

---

## Overview

This document describes the design for the creator payout system — the mechanism by which LYKE transfers earned commissions and sponsored fees to content creators. It covers the full lifecycle from an unconfirmed affiliate earning to a settled bank transfer, and defines all data model changes, service interfaces, background jobs, external integrations, and new API endpoints required.

---

## Current State

The existing implementation records earnings but does not move money:

| Concern | Current State |
|---------|--------------|
| Click tracking | ✅ Complete — `ClickEvent` created on every outbound product link click |
| Conversion ingestion | ✅ Complete — AWIN postback creates a `CreatorEarning` with `Status = Pending` (see `affiliate-network-design.md` Part 3) |
| Earning confirmation | ✅ Complete — driven by AWIN transaction status updates via postback and `AwinReconciliationFunction` (see `affiliate-network-design.md` Part 4) |
| AWIN payment tracking | ❌ Missing — no record of when AWIN settles payments to LYKE |
| Payout disbursement | ❌ Missing — `Creator.PayoutDetails` stored but never read; no payment processor |
| Payout account management | ❌ Missing — no onboarding flow to collect banking info |
| Sponsored earnings | ❌ Missing — `SponsoredPlacement` entity exists but never generates `CreatorEarning` records |

---

## Payment Processor: Stripe Connect

**Chosen approach: Stripe Connect Express accounts.**

Rationale:
- Creator KYC (identity verification, bank account collection) is fully handled by Stripe via its hosted onboarding UI — LYKE never touches bank details, reducing PCI and regulatory scope
- Express accounts support payouts to creators in 40+ countries in 135+ currencies
- Stripe's API has first-class .NET SDK support (`Stripe.net`)
- Consistent with the existing webhook pattern (`HMAC-SHA256` signature verification already in `CommerceService`)

LYKE operates as the Stripe platform account. Affiliate commissions and sponsored fees flow into LYKE's Stripe balance, and transfers are issued to each creator's connected Express account on payout.

---

## Payment Schedule: AWIN → LYKE → Creator

The affiliate network integration introduces a cash flow dependency that determines when creators can be paid. LYKE cannot pay creators until AWIN has paid LYKE.

### Timeline for a Single Conversion

```
Day 0        Shopper buys from retailer via LYKE link
             → AWIN postback creates CreatorEarning (Pending)

Day 1–45     Retailer validation window
             → Retailer confirms or declines the transaction within AWIN
             → AWIN postback or reconciliation job promotes Pending → Confirmed (or Reversed)

End of month AWIN closes the monthly billing cycle
             → All transactions confirmed during the month are batched

+30 days     AWIN pays LYKE (NET-30 after month-end)
             → LYKE records AwinPaymentBatch; marks covered earnings as Payable

+30–37 days  Creator can request payout (or auto-payout triggers on 1st of month)
             → PayoutRequest created, approved, Stripe transfer issued
```

**Realistic end-to-end timeline:** A sale on day 1 of a month, confirmed on day 30, would not be paid to LYKE until ~day 60, and the creator could receive funds around day 60–67. In the worst case (sale on day 1, confirmed on day 45, AWIN pays NET-30 from end of that month), the creator waits ~10 weeks.

This delay is standard in affiliate marketing and should be clearly communicated in the creator earnings UI with an estimated payment date.

---

## Earning Lifecycle

```
AWIN postback received (status=pending)
         │
         ▼
  CreatorEarning (Pending)          ← conversion recorded; awaiting retailer validation
         │
         │  AWIN postback (status=confirmed) or AwinReconciliationFunction
         │
         ├──────────────────────────────────────────────────┐
         │                                                  │
         ▼                                                  ▼
  CreatorEarning (Confirmed)        ← AWIN validated    CreatorEarning (Reversed)
         │                                               ← retailer declined/returned
         │  AWIN monthly payment received by LYKE         (excluded from all balances)
         │  (AwinPaymentBatch recorded)
         ▼
  CreatorEarning (Payable)          ← LYKE has the cash; safe to pay creator
         │
         │  Creator requests payout (or auto-payout threshold reached)
         ▼
  PayoutRequest (Pending)           ← bundles all Payable earnings into one request
         │
         │  Admin approves (auto-approved below threshold)
         ▼
  PayoutRequest (Processing)        ← Stripe Transfer created
         │
         │  Stripe transfer.paid webhook received
         ▼
  PayoutRequest (Completed)         ← funds sent to creator's bank
  CreatorEarning (Paid)             ← all associated earnings marked Paid
```

### Reversal After Payment (Clawback)

If a transaction is reversed by AWIN after the creator has already been paid, LYKE faces a shortfall. See "Clawback Mechanism" below.

---

## Data Model Changes

### New Entity: `CreatorPayoutAccount`

Stores the creator's Stripe Connect account details. One per creator.

```
CreatorPayoutAccount
├── Id (GUID)
├── CreatorId (FK → Creator, unique)
├── StripeAccountId (string, encrypted)   ← e.g. "acct_1PxABCDEFGHIJKL"
├── AccountStatus (enum)                  ← Pending | Active | Restricted | Suspended
├── OnboardingComplete (bool)
├── PayoutCurrency (string)               ← ISO 4217, e.g. "GBP"
├── CreatedAt (DateTime)
└── UpdatedAt (DateTime?)
```

`StripeAccountId` is encrypted at rest using AES-256 via a `IEncryptionService` (same pattern as existing `PayoutDetails` intent). The existing `Creator.PayoutDetails` field is superseded by this entity and should be left in place but ignored for now (migration path: null it out once onboarding is complete).

**Enum: `StripeAccountStatus`**
```
Pending      = 0   ← account created, onboarding not started
Active       = 1   ← KYC complete, payouts enabled
Restricted   = 2   ← Stripe has restricted the account (action needed)
Suspended    = 3   ← account disabled by LYKE or Stripe
```

### New Entity: `PayoutRequest`

Tracks a single payout event that bundles multiple payable earnings.

```
PayoutRequest
├── Id (GUID)
├── CreatorId (FK → Creator)
├── Amount (decimal)
├── Currency (string)
├── Status (enum)                         ← Pending | Approved | Processing | Completed | Failed | Rejected
├── StripeTransferId (string?)            ← populated after Stripe transfer created
├── FailureReason (string?)
├── AdminNotes (string?)
├── RequestedAt (DateTime)
├── ReviewedAt (DateTime?)
├── ProcessedAt (DateTime?)
└── Creator (navigation)
```

**Enum: `PayoutRequestStatus`**
```
Pending     = 0   ← submitted by creator, awaiting admin review
Approved    = 1   ← approved (by admin or auto-approved below threshold)
Processing  = 2   ← Stripe Transfer API call made
Completed   = 3   ← Stripe webhook confirmed transfer paid
Failed      = 4   ← Stripe transfer failed; earnings reverted to Payable
Rejected    = 5   ← admin rejected (e.g. fraud suspicion)
```

### New Entity: `AwinPaymentBatch`

Tracks when AWIN pays LYKE, enabling the Confirmed → Payable transition.

```
AwinPaymentBatch
├── Id (GUID)
├── AwinPaymentReference (string)         ← AWIN's payment reference from their report
├── PeriodStart (DateTime)                ← start of AWIN billing period covered
├── PeriodEnd (DateTime)                  ← end of AWIN billing period covered
├── TotalAmount (decimal)                 ← total amount received from AWIN
├── Currency (string)                     ← ISO 4217
├── ReceivedAt (DateTime)                 ← date LYKE received the AWIN payment
├── RecordedByUserId (Guid)              ← admin who recorded the payment
├── CreatedAt (DateTime)
└── Notes (string?)
```

When an `AwinPaymentBatch` is recorded, all `Confirmed` earnings with `CreatedAt` within the batch's `PeriodStart–PeriodEnd` range are promoted to `Payable`. This is the gate that ensures LYKE has the cash before paying creators.

For MVP with a single retailer and low volume, this is a **manual admin action**: the admin records the batch when the AWIN payment lands in LYKE's bank account. Automation via AWIN's reporting API can be added later.

### Modified Entity: `CreatorEarning`

Add a nullable FK to `PayoutRequest` and the `AwinPaymentBatch`:

```
CreatorEarning
├── ... (existing fields)
├── PayoutRequestId (Guid?, FK → PayoutRequest)       ← set when earning is included in a payout
└── AwinPaymentBatchId (Guid?, FK → AwinPaymentBatch) ← set when AWIN payment received
```

When a `PayoutRequest` enters `Failed` status, `PayoutRequestId` on all associated earnings is cleared and their `Status` is reverted to `Payable` so they can be included in the next payout attempt.

**Updated Enum: `EarningStatus`**
```
Pending   = 0   ← AWIN transaction pending retailer validation
Confirmed = 1   ← AWIN confirmed; awaiting AWIN payment to LYKE
Payable   = 2   ← AWIN has paid LYKE; creator can request payout
Paid      = 3   ← included in a completed PayoutRequest
Reversed  = 4   ← AWIN declined/deleted; excluded from all balances
```

Note: `Payable` is a new status inserted between `Confirmed` and `Paid`. This is the key change driven by the AWIN payment schedule — `Confirmed` alone no longer means the money is available for creator payout.

**Updated Enum: `EarningType`**
```
Affiliate  = 0   ← commission from affiliate sale
Sponsored  = 1   ← fee from sponsored placement
Clawback   = 2   ← negative earning to recover reversed paid commissions
```

### Modified Entity: `SponsoredPlacement`

Add a `CreatorId` FK so sponsored placements can be attributed to a specific creator:

```
SponsoredPlacement
├── ... (existing fields)
└── CreatorId (Guid?, FK → Creator)   ← nullable; null = platform-placed, non-null = creator-attributed
```

---

## Clawback Mechanism

If AWIN reverses a transaction after the creator has already been paid (`EarningStatus = Paid`), LYKE needs to recover the funds. This is addressed with a tiered approach:

### Rules

| Reversal amount | Action |
|----------------|--------|
| ≤ £10 | **Absorb** — LYKE takes the loss. The reversed earning is marked `Reversed` but no clawback is created. |
| > £10 | **Clawback** — A `CreatorEarning` of type `Clawback` is created with a negative `Amount`. This is deducted from the creator's next payout balance. |

### Implementation

1. When `AwinReconciliationFunction` or an AWIN postback marks a `Paid` earning as `Reversed`:
   - If `|Amount| <= ClawbackThreshold` (£10 default, configured in `AwinSettings`): log and absorb
   - If `|Amount| > ClawbackThreshold`: create a `CreatorEarning` with `EarningType = Clawback`, `Amount = -originalAmount × 0.70` (creator's share only), `Status = Payable` (immediately deductible)
2. When `PayoutRequest` is created, `Clawback` earnings are included in the balance calculation, reducing the total payout amount
3. If clawbacks exceed the creator's available balance, the payout request is blocked until positive earnings cover the deficit

### Creator Communication

The creator earnings UI must clearly show clawbacks with an explanation:
- "A previous sale of £X was returned by the customer. £Y has been deducted from your balance."
- Link to FAQ explaining retailer return policies and how they affect creator earnings

---

## New Configuration: `PayoutSettings`

```csharp
public class PayoutSettings
{
    public const string SectionName = "Payout";

    // Auto-approve payouts below this amount; above requires admin review
    public decimal AutoApproveThreshold { get; set; } = 200.00m;

    // Minimum payout amount a creator can request
    public decimal MinPayoutThreshold { get; set; } = 50.00m;

    // Stripe platform account secret key
    public string StripeSecretKey { get; set; } = string.Empty;

    // Stripe webhook signing secret for /api/webhooks/stripe
    public string StripeWebhookSecret { get; set; } = string.Empty;
}
```

Note: `AttributionWindowDays` and `ConfirmationJobSchedule` have been removed. Earning confirmation is now driven by AWIN transaction status (see `affiliate-network-design.md` Part 4), not by elapsed time. The `AwinReconciliationFunction` schedule is configured in `AwinSettings`.

---

## New Service Interfaces

### `IPayoutService` (Application layer)

```csharp
public interface IPayoutService
{
    // Stripe Connect onboarding
    Task<string> CreateOnboardingUrlAsync(Guid userId, CancellationToken ct = default);
    Task<CreatorPayoutAccountResponse> GetPayoutAccountAsync(Guid userId, CancellationToken ct = default);
    Task HandleStripeAccountUpdatedAsync(string stripeAccountId, CancellationToken ct = default);

    // Payout requests
    Task<PayoutRequestResponse> RequestPayoutAsync(Guid userId, CancellationToken ct = default);
    Task<(IReadOnlyList<PayoutRequestResponse> Requests, PaginationMeta Meta)> GetPayoutHistoryAsync(Guid userId, PayoutHistoryRequest request, CancellationToken ct = default);

    // Admin actions
    Task<(IReadOnlyList<AdminPayoutResponse> Requests, PaginationMeta Meta)> ListPayoutRequestsAsync(AdminPayoutListRequest request, CancellationToken ct = default);
    Task ApprovePayoutAsync(Guid payoutRequestId, string? adminNotes, CancellationToken ct = default);
    Task RejectPayoutAsync(Guid payoutRequestId, string reason, CancellationToken ct = default);

    // AWIN payment tracking
    Task RecordAwinPaymentBatchAsync(AwinPaymentBatchRequest request, CancellationToken ct = default);

    // Stripe webhook
    Task HandleTransferCreatedAsync(string stripeTransferId, CancellationToken ct = default);
    Task HandleTransferPaidAsync(string stripeTransferId, CancellationToken ct = default);
    Task HandleTransferFailedAsync(string stripeTransferId, string failureReason, CancellationToken ct = default);
}
```

### `IEncryptionService` (Application layer)

```csharp
public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
```

AES-256-GCM implementation in Infrastructure, key sourced from `KeyVault` (already provisioned in Terraform).

---

## New API Endpoints

### Creator Endpoints

```
POST   /api/creators/v1/payouts/onboarding
  → Creates Stripe Connect Express account (if not exists), returns { onboardingUrl }
  → Creator must be Verified before calling this

GET    /api/creators/v1/payouts/account
  → Returns CreatorPayoutAccountResponse { accountStatus, onboardingComplete, payoutCurrency }

POST   /api/creators/v1/payouts/request
  → Bundles all Payable earnings (including any Clawback deductions) into a PayoutRequest
  → Validates: account Active, payable balance >= MinPayoutThreshold
  → Auto-approves if amount <= AutoApproveThreshold (triggers Stripe immediately)
  → Returns PayoutRequestResponse

GET    /api/creators/v1/payouts/history
  → Paginated list of PayoutRequest records for the creator

GET    /api/creators/v1/payouts/balance
  → Returns { confirmedBalance, payableBalance, pendingBalance, clawbackBalance }
  → confirmedBalance: earnings confirmed by AWIN but not yet paid to LYKE
  → payableBalance: earnings available for payout (AWIN has paid LYKE)
  → pendingBalance: earnings awaiting AWIN validation
  → clawbackBalance: outstanding clawback deductions
```

### Admin Endpoints

```
GET    /api/admin/v1/payouts
  → List all PayoutRequests with filter: status, creatorId, dateRange
  → Sorted by RequestedAt desc

POST   /api/admin/v1/payouts/{id}/approve
  → Sets status Approved → Processing, initiates Stripe Transfer
  → Body: { adminNotes? }

POST   /api/admin/v1/payouts/{id}/reject
  → Sets status Rejected, earnings revert to Payable for future payout
  → Body: { reason }

POST   /api/admin/v1/awin-payments
  → Records an AwinPaymentBatch (admin enters when AWIN payment lands in bank)
  → Body: { awinPaymentReference, periodStart, periodEnd, totalAmount, currency, notes? }
  → On success: all Confirmed earnings within the period are promoted to Payable
  → Returns { earningsPromoted: int, totalAmountPromoted: decimal }
```

### Webhook Endpoint

```
POST   /api/webhooks/stripe
  → Validates Stripe-Signature header (ConstructEvent with webhook secret)
  → Handles: account.updated, transfer.created, transfer.paid, transfer.failed
  → Returns 200 immediately (idempotent processing)
```

---

## Background Jobs: Azure Functions Timer Triggers

Consistent with the existing `Lyke.Functions` isolated-worker project. All functions use Y1 Consumption plan (scale-to-zero; see `infrastructure-cost-forecast.md`).

### `AwinReconciliationFunction`

- **Schedule:** `0 6 * * *` (6:00am UTC daily)
- **Logic:** Queries AWIN's transactions API for the past 48 hours. For each transaction, syncs the AWIN status to the matching `CreatorEarning` record. Handles `pending → confirmed`, `pending → declined`, and `confirmed → declined` transitions.
- **Replaces:** The previously planned time-based `EarningConfirmationFunction`. Confirmation is now driven entirely by AWIN transaction status, not elapsed time.
- **Details:** See `affiliate-network-design.md` Part 4.

### `AutoPayoutFunction` _(optional, Phase 2)_

- **Schedule:** `0 6 1 * *` (6:00am UTC on the 1st of each month)
- **Logic:** For each creator with `payable balance >= MinPayoutThreshold` and an Active payout account, automatically submits a `PayoutRequest` and approves it if below `AutoApproveThreshold`.
- **Note:** This runs on the 1st of the month. AWIN typically pays LYKE around the end of the previous month (NET-30). The admin must record the `AwinPaymentBatch` before auto-payouts run, otherwise no earnings will be in `Payable` status. Consider adding a pre-check that warns if no payment batch has been recorded for the previous month.

---

## Stripe Integration Details

### Stripe Connect Express Onboarding Flow

```
1. Creator taps "Set up payouts" in app
2. POST /api/creators/v1/payouts/onboarding
   → PayoutService creates Stripe account via StripeClient.Accounts.CreateAsync
   → Creates CreatorPayoutAccount with StripeAccountId (encrypted), Status=Pending
   → Calls StripeClient.AccountLinks.CreateAsync (type=account_onboarding)
   → Returns { onboardingUrl }
3. App opens onboardingUrl in in-app browser (Capacitor Browser plugin)
4. Creator completes Stripe's KYC flow (name, DOB, address, bank account)
5. Stripe redirects to app deep link: lyke://payouts/onboarding/complete
6. App calls GET /api/creators/v1/payouts/account → shows current status
7. Stripe sends account.updated webhook when KYC complete
   → PayoutService sets AccountStatus = Active, OnboardingComplete = true
```

### Stripe Transfer on Payout Approval

```
1. Admin approves PayoutRequest (or auto-approve triggers)
2. PayoutService calls StripeClient.Transfers.CreateAsync:
   {
     amount: <pence/cents>,
     currency: "gbp",
     destination: <StripeAccountId>,
     transfer_group: <PayoutRequest.Id>,
     metadata: { payoutRequestId, creatorId }
   }
3. PayoutRequest.StripeTransferId = transfer.Id, Status = Processing
4. Stripe sends transfer.paid webhook
5. PayoutService sets PayoutRequest.Status = Completed, ProcessedAt = now
6. All associated CreatorEarning records: Status = Paid, PaidAt = now
7. Email notification sent to creator: "Your payout of £X.XX has been sent"
```

### Stripe Transfer Failure

```
1. Stripe sends transfer.failed webhook
2. PayoutService sets PayoutRequest.Status = Failed, FailureReason = <stripe message>
3. Associated CreatorEarning.PayoutRequestId = null, Status reverted to Payable
4. Email notification sent to creator: "Your payout failed — please check your account"
5. Creator can retry by calling POST /api/creators/v1/payouts/request again
```

---

## Sponsored Earnings

The `SponsoredPlacement` entity currently tracks retailer ad campaigns but does not generate `CreatorEarning` records. The connection requires:

1. **`SponsoredPlacement.CreatorId` FK** — links a campaign to a specific creator who will be compensated
2. **Campaign serve event** — when a sponsored post impression is served (tracked via `EngagementType.View` on a post linked to an active placement), create a `CreatorEarning` of `EarningType.Sponsored`
3. **Budget deduction** — deduct `SpentAmount` from `SponsoredPlacement.BudgetAmount`; deactivate placement when budget exhausted
4. **Fixed fee vs CPM** — two models:
   - **Fixed fee**: agreed upfront, single `CreatorEarning` created when campaign starts (Status=Payable immediately — no AWIN dependency since sponsored fees are paid directly by the retailer to LYKE)
   - **CPM (cost per mille)**: earning created per 1000 impressions served, Status=Payable immediately (same rationale)

For MVP, implement **fixed fee only** as it is simpler and avoids impression fraud concerns.

Note: Sponsored earnings bypass the AWIN payment schedule entirely. The retailer pays LYKE directly for sponsored placements (via Stripe invoice or similar), so these earnings can be made `Payable` immediately upon confirmation. No `AwinPaymentBatch` dependency.

---

## Security Considerations

| Concern | Mitigation |
|---------|-----------|
| `StripeAccountId` exposure | Encrypted at rest (AES-256-GCM), decrypted only in `IPayoutService` impl |
| Stripe webhook spoofing | `StripeClient.ConstructEvent` validates `Stripe-Signature` HMAC |
| Double-payout | `PayoutRequest` creation checks for existing `Pending/Processing` request; `CreatorEarning.PayoutRequestId` prevents double-inclusion |
| Payout to unverified creator | `POST /payouts/onboarding` requires `VerificationStatus == Approved` |
| Payout account not Active | `POST /payouts/request` validates `AccountStatus == Active` |
| Payout before LYKE has cash | Only `Payable` earnings (covered by `AwinPaymentBatch`) can be included in payout requests |
| Earnings manipulation | `CreatorEarning` records are system-generated only (no creator write endpoint) |
| Clawback abuse | Clawbacks are system-generated from AWIN reversals only; creators cannot dispute via API |
| GDPR | Stripe stores PII/bank info under its own DPA; `StripeAccountId` stored encrypted; on GDPR deletion, call Stripe Accounts.DeleteAsync and null out `StripeAccountId` |

---

## Email Notifications (new templates)

| Event | Template name | Recipient |
|-------|--------------|-----------|
| Payout request received | `payout-requested.liquid` | Creator |
| Payout approved (above threshold) | `payout-approved.liquid` | Creator |
| Payout completed | `payout-completed.liquid` | Creator |
| Payout failed | `payout-failed.liquid` | Creator |
| Payout rejected by admin | `payout-rejected.liquid` | Creator |
| Payout requires admin review | `payout-admin-review.liquid` | Admin |
| Earnings now payable (AWIN payment received) | `earnings-payable.liquid` | Creator |
| Clawback applied | `clawback-applied.liquid` | Creator |

---

## Frontend Changes

### Creator — Earnings Page (update existing)

- Add "Set up payouts" CTA when `onboardingComplete == false`
- Show payout account status badge (Pending / Active / Restricted)
- **Balance breakdown:** Show three balances clearly:
  - **Pending** — awaiting retailer validation (not yet confirmed by AWIN)
  - **Confirmed** — validated by AWIN, awaiting AWIN payment to LYKE
  - **Available for payout** — AWIN has paid LYKE; creator can request payout
- "Request payout" button: enabled when `payableBalance >= MinPayoutThreshold && accountStatus == Active`
- Payout history tab: list of `PayoutRequest` records with status and amount
- **Clawback visibility:** If clawback balance exists, show a notice explaining the deduction with a link to the reversed transaction
- **Estimated payment date:** For `Pending` and `Confirmed` earnings, show an estimated date based on AWIN's validation + payment cycle (e.g., "Estimated available: mid-March")

### Creator — Payout Onboarding Page (new)

- Explains what Stripe Connect is and what data Stripe will collect
- "Connect bank account" button → opens `onboardingUrl` in in-app browser
- Returns to app via deep link `lyke://payouts/onboarding/complete`
- Polls `GET /api/creators/v1/payouts/account` every 3s for up to 30s to confirm `onboardingComplete`

### Admin — Payout Management Page (new)

- Tab-based: Pending Review / All
- Each row: creator name, amount, requested date, status badge, Approve/Reject actions
- Approve modal: optional admin notes field
- Reject modal: required reason field

### Admin — AWIN Payment Recording (new)

- Simple form: AWIN payment reference, period start/end, total amount, currency, notes
- On submit: calls `POST /api/admin/v1/awin-payments`
- Shows confirmation: "X earnings totalling £Y promoted to Payable"
- List of past `AwinPaymentBatch` records for audit

---

## Open Questions / Out of Scope for MVP

1. **Tax reporting (1099/HMRC)** — Stripe provides tax forms for US creators via the dashboard. UK creators will need annual summaries. Out of scope for now.
2. **Multi-currency conversion** — Stripe handles FX for international creators. LYKE always initiates transfers in the creator's `PayoutCurrency`. Rate risk is Stripe's.
3. **Automated AWIN payment ingestion** — MVP uses manual admin recording of AWIN payments. Future: parse AWIN's monthly payment report CSV or use their reporting API to auto-create `AwinPaymentBatch` records.
4. **Auto-payout (scheduled)** — `AutoPayoutFunction` described above is Phase 2; MVP requires creator to manually request payout.
5. **Minimum payout frequency cap** — prevent creators requesting payout more than once per 7 days (spam mitigation). Add to `POST /payouts/request` validation in Phase 2.
6. **Clawback dispute process** — currently no mechanism for creators to dispute a clawback. For MVP, direct creators to contact support. Consider a formal dispute flow in Phase 2.
