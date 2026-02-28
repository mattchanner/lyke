# Infrastructure Cost Forecast — Year 1

**Created:** 2026-02-28
**Currency:** GBP (£). Azure bills in USD; estimates use £1 = $1.27 (approximate). All figures are estimates and should be re-validated against current Azure pricing at time of provisioning.

---

## Assumptions

- **Launch timeline:** Q1 = soft launch (single retailer, curated creator cohort ~50); Q2 = growth (AWIN live, multiple retailers); Q3 = payouts live, scaling; Q4 = established product
- **Region:** Azure UK South (all resources), consistent with current Terraform config
- **Team size:** 2-3 developers
- **Traffic profile:** low but real from day one; meaningful growth from Q2; no viral spike scenario planned for (handle separately if needed)
- **Included features by quarter:** Q1 core MVP; Q2 AWIN affiliate integration + Redis caching; Q3 Stripe creator payouts + AWIN reconciliation; Q4 sustained growth
- **Not included:** VAT (add 20% for UK business costs where applicable); exchange rate fluctuation risk

---

## Environment Architecture

Two permanent environments, matching current Terraform structure:

| Environment | Purpose | Key difference |
|-------------|---------|---------------|
| **Dev/Staging** | CI/CD deployments, QA, integration testing | Scale-to-zero, smaller SKUs, email dry-run |
| **Production** | Live users | Always-on, larger SKUs, GRS storage, 90-day log retention |

The Container Registry (Basic SKU) is shared between both environments.

---

## Part 1: Azure Infrastructure — Dev/Staging

Dev/Staging costs are relatively stable throughout the year. Redis is added in Q2 once the caching implementation is complete.

| Service | SKU / Config | Monthly (Q1) | Monthly (Q2+) | Notes |
|---------|-------------|-------------|--------------|-------|
| Container App (API) | 0.5 vCPU, 1Gi, scale-to-zero | £7 | £7 | Charged only when running; CI deployments drive most usage |
| PostgreSQL Flexible Server | B_Standard_B1ms, 32GB, LRS | £14 | £14 | Dev DB; burstable 1 vCore |
| Azure Functions | Y1 Consumption | £1 | £1 | Scale-to-zero; dev traffic is negligible |
| Storage Account | Standard LRS | £2 | £2 | Media blobs + processing queue |
| Log Analytics Workspace | 30-day retention | £3 | £3 | Dev logs; typically stays within 5GB free tier |
| Key Vault | Standard, ~20 secrets | £1 | £1 | Negligible ops cost |
| Container Registry | Basic SKU (shared with prod) | £4 | £4 | One registry; charged once |
| Azure Cache for Redis | — | — | £13 | Basic C0 (250MB); added Q2 |
| ACS Email | Dry-run mode | £0 | £0 | No emails sent in dev |
| **Dev/Staging Total** | | **£32/month** | **£45/month** | |

**Annual Dev/Staging: (£32 × 3) + (£45 × 9) = £96 + £405 = £501**

---

## Part 2: Azure Infrastructure — Production

Production scales across four quarters as the user base and feature set grows.

### Service Sizing by Quarter

| Service | Q1 (M1–3) | Q2 (M4–6) | Q3 (M7–9) | Q4 (M10–12) |
|---------|-----------|-----------|-----------|-------------|
| Container App | 1 vCPU, 2Gi, min 1 | 1 vCPU, 2Gi, min 1 | 1 vCPU, 2Gi, min 1–2 | 2 vCPU, 4Gi, min 1–2 |
| PostgreSQL | D2s_v3, 64GB, GRS | D2s_v3, 64GB, GRS | D4s_v3, 128GB, GRS | D4s_v3, 128GB, GRS |
| Functions | EP1 Elastic Premium | EP1 | EP1 | EP1 |
| Redis | — | Standard C1 (1GB) | Standard C1 (1GB) | Standard C2 (6GB) |
| Storage | ~20GB GRS | ~50GB GRS | ~100GB GRS | ~200GB GRS |
| CDN | — | Basic | Standard | Standard |

### Monthly Cost Breakdown by Quarter

**Q1 — Soft Launch (months 1–3)**

| Service | Monthly | Notes |
|---------|---------|-------|
| Container App (API) | £57 | 1 vCPU, 2Gi, min 1 replica always on; ~£75 including peak scaling |
| PostgreSQL D2s_v3 + 64GB GRS | £80 | ~$92/hr compute + $0.115/GB storage + GRS backup |
| Azure Functions EP1 | £122 | ⚠️ 1 always-ready instance; eliminates cold starts for media processing |
| Storage GRS ~20GB | £3 | Hot tier, geo-redundant |
| Log Analytics + App Insights | £10 | 90-day retention; within or just above 5GB free tier |
| Key Vault | £2 | ~20 secrets + low ops volume |
| ACS Email (~3,000/month) | £1 | Within or just above 3,000/month free tier |
| **Q1 Monthly Total** | **£275** | |

**Q2 — Growth Phase (months 4–6)** ← *Redis and CDN added; AWIN live*

| Service | Monthly | Notes |
|---------|---------|-------|
| Container App (API) | £80 | Increased traffic from AWIN clicks |
| PostgreSQL D2s_v3 + 64GB GRS | £80 | Same SKU; no upgrade needed yet |
| Azure Functions EP1 | £122 | Now running AWIN reconciliation job in addition to media processing |
| Redis Standard C1 (1GB) | £63 | Feed caching, session data; Standard tier for HA/SLA |
| Storage GRS ~50GB | £5 | Growing media library |
| CDN | £8 | Azure CDN Standard for static assets + media delivery |
| Log Analytics + App Insights | £12 | Slightly more telemetry with AWIN |
| Key Vault | £2 | Additional AWIN API token secret |
| ACS Email (~6,000/month) | £1 | Verification, moderation, onboarding emails |
| **Q2 Monthly Total** | **£373** | |

**Q3 — Scaling Phase (months 7–9)** ← *Stripe payouts live; DB upgraded; creator base growing*

| Service | Monthly | Notes |
|---------|---------|-------|
| Container App (API) | £100 | Average 1.5 replicas as shopper traffic grows |
| PostgreSQL D4s_v3 + 128GB GRS | £157 | ⚠️ Upgrade trigger: read latency or CPU > 70% sustained |
| Azure Functions EP1 | £130 | 2 additional timer jobs (AWIN reconciliation, payout confirmation); slightly more compute |
| Redis Standard C1 (1GB) | £63 | Same tier; monitor memory usage |
| Storage GRS ~100GB | £8 | Media growing with creator cohort expansion |
| CDN | £15 | Growing asset delivery volume |
| Log Analytics + App Insights | £15 | More services generating telemetry |
| Key Vault | £3 | Stripe secrets + AES encryption key added |
| ACS Email (~10,000/month) | £2 | Payout notifications added to email volume |
| **Q3 Monthly Total** | **£493** | |

**Q4 — Established (months 10–12)** ← *Full feature set; sustained growth*

| Service | Monthly | Notes |
|---------|---------|-------|
| Container App (API) | £130 | Average 1.8 replicas; may need 2 vCPU if CPU-bound |
| PostgreSQL D4s_v3 + 128GB GRS | £157 | Same SKU; add read replica if query load warrants it (+£145/month) |
| Azure Functions EP1 | £150 | 2 minimum instances for redundancy at scale |
| Redis Standard C2 (6GB) | £133 | ⚠️ Upgrade trigger: C1 memory utilisation > 70% |
| Storage GRS ~200GB | £13 | ~200GB accumulated media |
| CDN | £20 | Standard volume |
| Log Analytics + App Insights | £20 | 90-day retention of growing telemetry |
| Key Vault | £4 | Low growth |
| ACS Email (~20,000/month) | £3 | Growing user base, more notification triggers |
| **Q4 Monthly Total** | **£630** | |

### Annual Production Total

| Quarter | Monthly | × Months | Subtotal |
|---------|---------|----------|---------|
| Q1 | £275 | 3 | £825 |
| Q2 | £373 | 3 | £1,119 |
| Q3 | £493 | 3 | £1,479 |
| Q4 | £630 | 3 | £1,890 |
| **Annual Production** | | | **£5,313** |

> **Note on Azure Functions EP1:** The Elastic Premium plan (currently configured in `prod.tfvars`) costs ~£122/month for 1 always-ready instance, primarily to eliminate cold starts for media processing. If a 10–30 second cold start on media jobs is acceptable (they run asynchronously via queue), switching to Y1 Consumption for prod functions would save approximately **£1,400/year**. The trade-off: first media processing job after a period of inactivity will be slower. Timer-triggered jobs (earnings confirmation, AWIN reconciliation) are unaffected by cold starts. This is worth revisiting before launch.

---

## Part 3: Stripe Connect — Creator Payout Fees

Stripe fees are not infrastructure costs but are operational costs directly linked to creator payout activity. They are included here for completeness.

Stripe Connect Express (UK pricing):
- **£2/active connected account/month** (an account is "active" if it received a payout in that month)
- **0.25% per transfer** to a connected account (minimum £1.50 per transfer)

| Quarter | Active Creators (est.) | Account Fees | Transfer Fees | Monthly Total | Quarterly Total |
|---------|----------------------|-------------|--------------|--------------|----------------|
| Q1 | 0 (payouts not live) | £0 | £0 | £0 | £0 |
| Q2 | 0 (payouts not live) | £0 | £0 | £0 | £0 |
| Q3 | ~30 | £60 | £45 (min £1.50 × 30) | £105 | £315 |
| Q4 | ~75 | £150 | £113 (min £1.50 × 75) | £263 | £789 |
| **Annual Stripe Total** | | | | | **£1,104** |

These fees come out of LYKE's 30% commission share before any platform profit. At 30 active creators each paying out £80 average, LYKE's gross revenue from that cohort is £720/month; Stripe takes £105, leaving £615.

---

## Part 4: Developer Tooling & Recurring Subscriptions

| Service | Cost | Frequency | Annual | Notes |
|---------|------|-----------|--------|-------|
| GitHub Team (3 developers) | $4/user/month | Monthly | £113 | Includes 3,000 Actions minutes/month; private repos; required |
| GitHub Actions — Linux runners | Included | — | £0 | Covers Android builds, .NET tests, Docker builds |
| GitHub Actions — macOS runners (M1) | $0.08/minute | Per build | See below | Only needed if no Mac device available for iOS |
| Apple Developer Program | $99/year | Annual | £78 | Required for App Store distribution and TestFlight |
| Google Play Developer | $25 one-time | One-time | £20 | One-time registration fee |
| Domain — be-lyke.clothing | ~£30/year | Annual | £30 | .clothing TLD renewal; SSL is free via Azure managed certificates |
| **Annual Tooling Total** | | | **£241** | Excluding macOS CI runners |

### GitHub Actions macOS Runners (iOS builds)

If the team does not own a Mac, iOS builds must run on GitHub's macOS runners:

- M1 macOS runner: $0.08/minute
- Estimated build time: 20–25 minutes
- Estimated frequency: 20 builds/month (PRs + releases)
- Monthly cost: 20 × 22 min × $0.08 = ~$35 = **£28/month = £330/year**

If a Mac device is owned (see Part 5), this cost is £0.

---

## Part 5: One-Time Setup Costs

| Item | Cost | Required? | Notes |
|------|------|-----------|-------|
| Mac Mini M4 (16GB RAM, 256GB SSD) | £899 | Yes, if no Mac available | Required for iOS builds (Xcode only runs on macOS). Mac Mini is the lowest-cost option that meets requirements. |
| MacBook Pro 14" M4 (if portability needed) | £1,799 | Alternative to Mac Mini | Higher cost; prefer if developer needs a portable machine |
| Apple iPhone (for physical device testing) | £0–£700 | Recommended | Simulator catches most issues; physical device testing is best practice before App Store submission. Use existing device if available. |
| Android test device | £0–£300 | Recommended | A mid-range device (Pixel 7a ~£300) is sufficient; emulator covers most cases |
| AWIN publisher registration | £0 | Yes | Free for publishers |
| Azure initial resource provisioning | £0 | Yes | Covered by pay-as-you-go; no setup fee |
| Terraform remote state storage | ~£1/month | Yes | Azure Storage blob for state file; negligible |
| **One-time total (Mac Mini path)** | **£899–£1,899** | | Varies based on device testing requirements |

---

## Year 1 Cost Summary

### Azure Infrastructure

| Environment | Annual Cost |
|-------------|------------|
| Dev/Staging | £501 |
| Production | £5,313 |
| **Azure Total** | **£5,814** |

### All Costs Combined

| Category | Annual | Notes |
|----------|--------|-------|
| Azure — Dev/Staging | £501 | Stable throughout year |
| Azure — Production | £5,313 | Grows from £275 to £630/month |
| Stripe Connect fees | £1,104 | Variable; only from Q3 when payouts go live |
| GitHub Team plan | £113 | Fixed |
| Apple Developer Program | £78 | Fixed annual |
| Google Play (one-time) | £20 | One-time only |
| Domain renewal | £30 | Fixed annual |
| **Recurring Annual Total** | **£7,159** | |
| | | |
| Mac Mini M4 (one-time) | £899 | If no Mac owned |
| Additional test devices | £0–£1,000 | As needed |
| GitHub Actions macOS runners | £330 | Only if no Mac; otherwise £0 |
| **One-time / conditional costs** | **£899–£2,229** | |
| | | |
| **Year 1 Grand Total (Mac Mini path)** | **~£8,058** | |
| **Year 1 Grand Total (MacBook path)** | **~£8,958** | |
| **Year 1 if Mac already owned** | **~£7,159** | |

### Monthly Progression

| Month | Dev/Staging | Production | Stripe | Other | Monthly Total |
|-------|------------|-----------|--------|-------|--------------|
| 1–3 | £32 | £275 | £0 | £20 | £327 |
| 4–6 | £45 | £373 | £0 | £20 | £438 |
| 7–9 | £45 | £493 | £105 | £20 | £663 |
| 10–12 | £45 | £630 | £263 | £20 | £958 |

"Other" = monthly share of annual subscriptions (GitHub, Apple, domain = £241/year ÷ 12 ≈ £20/month).

---

## Scaling Triggers

These are the specific thresholds that should prompt infrastructure changes. Monitoring alerts should be set on each.

| Trigger | Metric | Action | Estimated cost impact |
|---------|--------|--------|-----------------------|
| PostgreSQL CPU > 70% sustained | Azure Monitor: `cpu_percent` | Upgrade D2s_v3 → D4s_v3 | +£77/month |
| PostgreSQL storage > 80% | Azure Portal: storage % | Increase to 128GB or 256GB | +£8–16/month |
| PostgreSQL read latency > 20ms p95 | App Insights custom metric | Add read replica | +£72/month (D2s_v3 replica) |
| Redis memory > 70% (C1 = 1GB) | Azure Monitor: `used_memory` | Upgrade C1 → C2 (6GB) | +£70/month |
| Container App avg CPU > 60% | Azure Monitor: `CpuPercentage` | Add 1 replica (auto-scales) or upgrade vCPU | +£57–114/month |
| Container App p95 response time > 500ms | App Insights: `requests/duration` | Investigate; likely DB or N+1 query before scaling | Varies |
| Log Analytics ingestion > 8GB/month | Azure Portal billing | Review log verbosity; move verbose logs to cheaper tier | +£8/month per GB |
| Functions EP1 CPU > 60% | Azure Monitor | Add second always-ready instance | +£122/month |

---

## Cost Optimisation Opportunities

The following changes are not implemented yet but would reduce costs materially:

### 1. Functions: Downgrade EP1 → Y1 Consumption (~£1,400/year saving)

The prod Functions plan is EP1 (Elastic Premium) to eliminate cold starts. All three functions (media processing, AWIN reconciliation, payout confirmation) are queue-triggered or timer-triggered and run asynchronously. A 10–30 second cold start adds no user-visible latency. Switching to Y1 Consumption in prod saves approximately £122/month from day one. **Recommend evaluating before prod launch.**

### 2. Redis: Defer to Q2 (~£63/month saving in Q1)

Redis is planned but not yet provisioned. The app currently functions without it (in-memory caching for lookups). Defer provisioning until feed performance degrades under real traffic, rather than provisioning speculatively from day one.

### 3. CDN: Defer to Q2 (~£8/month saving in Q1)

With a small initial user base, the CDN adds complexity and cost without meaningful benefit. Enable once media assets are being loaded by a meaningful number of concurrent users.

### 4. Container Registry: Downgrade is not possible (Basic is already minimum)

Basic SKU at ~£4/month is the floor. No action needed.

### 5. Dev environment: Keep scale-to-zero aggressively

The dev Container App is already configured `min_replicas = 0`. Ensure the CI/CD pipeline does not leave dev running continuously — confirm the app spins down after pipeline smoke tests complete.

### 6. PostgreSQL dev: Consider pausing overnight

Azure PostgreSQL Flexible Server can be stopped (not deleted) manually to save compute costs when not in use. This is a manual operation, but for a pre-launch team not running 24/7 CI, stopping the dev DB overnight saves ~70% of dev DB compute cost (~£10/month). Requires a startup script or scheduled stop/start.

---

## Azure Budgets and Alerts (Recommended)

Set the following Azure Budget alerts before provisioning prod:

| Budget | Alert threshold | Alert at |
|--------|----------------|----------|
| Production — monthly | £700 (Q1), £1,000 (Q4) | 80%, 100%, 110% |
| Dev/Staging — monthly | £80 | 80%, 100% |
| Entire subscription — monthly | £1,200 | 90%, 100% |

The 110% alert on prod is critical: it catches unexpected scaling events or misconfigured retry loops (e.g., a Functions job stuck in a retry storm consuming EP1 compute).
