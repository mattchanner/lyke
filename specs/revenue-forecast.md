# Revenue Forecast — Year 1

**Created:** 2026-02-28
**Updated:** 2026-02-28 — Costs updated to reflect Y1 Consumption plan for Azure Functions (was EP1 Elastic Premium). Annual infrastructure saving of ~£1,488 flows through to all scenarios.
**Currency:** GBP (£)
**Related:** `specs/infrastructure-cost-forecast.md`, `specs/affiliate-network-design.md`, `specs/payout-design.md`

---

## Purpose & Scope

This document models LYKE's projected revenue, costs, and net profit or loss across four user adoption scenarios for the first year. It covers affiliate commission income only — the sole implemented revenue stream. Sponsored placements and retailer insight subscriptions are noted at the end as unmodelled upside.

All cost figures are drawn directly from `specs/infrastructure-cost-forecast.md`. Revenue figures are derived from the affiliate commission model described in `specs/affiliate-network-design.md`.

---

## Revenue Model

LYKE earns money when a shopper clicks a product link in a creator's post and subsequently buys from the retailer. The commission chain is:

```
Retailer agrees 8% commission on sale with AWIN
    ↓
Shopper buys £75 item → retailer pays AWIN £6.00 (8%)
    ↓
AWIN retains 25% network override = £1.50
LYKE (publisher) receives £4.50
    ↓
Creator receives 70% = £3.15
LYKE retains 30% = £1.35
```

**LYKE's net take per £1 of GMV = 8% × 75% × 30% = 1.8%**

At a £75 average order value this yields **£1.35 per conversion** attributable to LYKE's platform revenue. This is LYKE's gross income before operating costs.

Note: Q1 revenue is modelled at 50% efficiency. AWIN is not live until Q2 and the first retailer direct integration will have slower validation and limited attribution tracking.

---

## Shared Assumptions

### Engagement Model

Conversions per active shopper per month are derived from a simple funnel:

```
Sessions/month × posts per session × products per post × CTR × conversion rate
```

| Parameter | Q1–Q2 | Q3–Q4 | Rationale |
|-----------|-------|-------|-----------|
| Sessions per MAU per month | 4 | 6 | Content supply and habit formation grows over time |
| Posts per session | 10 | 10 | Fixed browsing depth |
| Products visible per post | 2.5 | 2.5 | Average tagged products |
| **Total product impressions/MAU/month** | **100** | **150** | |

CTR and conversion rate vary by scenario — see below.

### Commission Parameters (fixed across all scenarios)

| Parameter | Value | Source |
|-----------|-------|--------|
| Retailer commission rate | 8% | Typical UK fashion on AWIN; varies 5–15% by retailer |
| AWIN network override | 25% | AWIN retains this from gross commission |
| Creator revenue share | 70% | Configured in `CommerceSettings.CreatorCommissionShare` |
| LYKE net share | 30% | Platform revenue |
| LYKE effective take rate | **1.8% of GMV** | 8% × 75% × 30% |

### Infrastructure & Operating Costs

Monthly running costs are taken directly from the cost forecast and held constant across Scenarios 1–3. Scenario 4 incurs additional infrastructure costs from Q3 due to scale.

| Period | Base monthly cost | Notes |
|--------|-----------------|-------|
| M1–3 | £208 | Azure dev+prod (Functions on Y1 Consumption) + subscriptions |
| M4–6 | £321 | Redis added; AWIN live |
| M7–9 | £436 | Base infrastructure; Stripe fees added separately |
| M10–12 | £557 | Full infrastructure; Stripe fees added separately |

Stripe Connect fees scale with the number of creators receiving monthly payouts and are added per scenario.

---

## Four Scenarios

### Scenario 1 — Minimal Traction

The app launches but struggles to retain shoppers. Content supply is thin, social sharing doesn't materialise, and the creator cohort remains small. The product works but hasn't found its audience.

**Monthly Active Users (Shoppers)**

| Q1 | Q2 | Q3 | Q4 | Year-end CMGR |
|----|----|----|----|----|
| 150 | 400 | 900 | 1,500 | ~22%/month |

**Engagement assumptions:** CTR 2%, Conversion 1.0%, AOV £65

LYKE net per conversion = £65 × 1.8% = **£1.17**

| Period | MAU | Revenue/MAU/month | Monthly Revenue | Quarterly Revenue |
|--------|-----|-------------------|----------------|------------------|
| Q1 (×50% AWIN) | 150 | £0.012 | £2 | £6 |
| Q2 | 400 | £0.023 | £9 | £28 |
| Q3 | 900 | £0.035 | £32 | £95 |
| Q4 | 1,500 | £0.035 | £53 | £158 |
| **Annual** | | | | **£287** |

---

### Scenario 2 — Slow but Steady

The app gains genuine traction through word of mouth within a niche early-adopter community. Creator content quality is high but the shopper base grows slowly. A small number of creators earn meaningfully.

**Monthly Active Users (Shoppers)**

| Q1 | Q2 | Q3 | Q4 | Year-end CMGR |
|----|----|----|----|----|
| 400 | 1,200 | 3,500 | 8,000 | ~30%/month |

**Engagement assumptions:** CTR 3%, Conversion 1.5%, AOV £70

LYKE net per conversion = £70 × 1.8% = **£1.26**

| Period | MAU | Revenue/MAU/month | Monthly Revenue | Quarterly Revenue |
|--------|-----|-------------------|----------------|------------------|
| Q1 (×50% AWIN) | 400 | £0.029 | £11 | £34 |
| Q2 | 1,200 | £0.057 | £68 | £205 |
| Q3 | 3,500 | £0.085 | £298 | £893 |
| Q4 | 8,000 | £0.085 | £680 | £2,040 |
| **Annual** | | | | **£3,172** |

---

### Scenario 3 — Good Product-Market Fit

Body-profile matching resonates strongly. Shoppers return frequently because recommendations convert well. Creators actively promote their LYKE presence to grow their earnings. One retailer partnership delivers measurable return-reduction results, unlocking a second.

**Monthly Active Users (Shoppers)**

| Q1 | Q2 | Q3 | Q4 | Year-end CMGR |
|----|----|----|----|----|
| 800 | 4,000 | 15,000 | 40,000 | ~37%/month |

**Engagement assumptions:** CTR 5%, Conversion 2.5%, AOV £75

LYKE net per conversion = £75 × 1.8% = **£1.35**

| Period | MAU | Revenue/MAU/month | Monthly Revenue | Quarterly Revenue |
|--------|-----|-------------------|----------------|------------------|
| Q1 (×50% AWIN) | 800 | £0.084 | £68 | £203 |
| Q2 | 4,000 | £0.169 | £676 | £2,028 |
| Q3 | 15,000 | £0.253 | £3,795 | £11,385 |
| Q4 | 40,000 | £0.253 | £10,120 | £30,360 |
| **Annual** | | | | **£43,976** |

---

### Scenario 4 — Breakout Growth

A combination of press coverage, retailer co-marketing, and organic social sharing drives rapid acquisition. The creator community becomes a self-sustaining flywheel: more creators bring more shoppers who bring more creators. LYKE becomes a known name in UK fashion.

**Monthly Active Users (Shoppers)**

| Q1 | Q2 | Q3 | Q4 | Year-end CMGR |
|----|----|----|----|----|
| 2,000 | 12,000 | 50,000 | 150,000 | ~43%/month |

**Engagement assumptions:** CTR 7%, Conversion 3.5%, AOV £80

LYKE net per conversion = £80 × 1.8% = **£1.44**

| Period | MAU | Revenue/MAU/month | Monthly Revenue | Quarterly Revenue |
|--------|-----|-------------------|----------------|------------------|
| Q1 (×50% AWIN) | 2,000 | £0.177 | £353 | £1,059 |
| Q2 | 12,000 | £0.353 | £4,236 | £12,708 |
| Q3 | 50,000 | £0.529 | £26,450 | £79,350 |
| Q4 | 150,000 | £0.529 | £79,350 | £238,050 |
| **Annual** | | | | **£331,167** |

> Note: Q3–Q4 infrastructure for Scenario 4 requires scaling beyond the base forecast. Additional cost of approximately £300/month in Q3 and £700/month in Q4 is added for extra Container App replicas, a PostgreSQL read replica, and a Redis tier upgrade.

---

## Annual Profit & Loss Summary

### Operating Costs by Scenario

Stripe Connect fees scale with the number of active creators receiving monthly payouts. Creator count is estimated at roughly 1 per 30 MAU, with approximately 25–35% of creators hitting the £50 minimum payout threshold in any given month.

| Cost Category | S1 | S2 | S3 | S4 |
|---------------|----|----|----|----|
| Azure infrastructure | £4,326 | £4,326 | £4,326 | £7,326 |
| Subscriptions (GitHub, Apple, domain) | £241 | £241 | £241 | £241 |
| Stripe Connect fees | £100 | £800 | £2,200 | £9,000 |
| **Total annual costs** | **£4,667** | **£5,367** | **£6,767** | **£16,567** |

### Net Profit / Loss

| | S1 | S2 | S3 | S4 |
|--|----|----|----|----|
| Annual revenue | £287 | £3,172 | £43,976 | £331,167 |
| Annual costs | £4,667 | £5,367 | £6,767 | £16,567 |
| **Net year 1 P&L** | **−£4,380** | **−£2,195** | **+£37,209** | **+£314,600** |

One-time setup costs (Mac Mini £540, Google Play £20) are not included in the P&L above as they are capital items rather than operating expenditure.

---

## Monthly Cash Flow by Scenario

Positive monthly cash flow means LYKE's revenue in that month covers that month's operating costs. Cumulative surplus means total revenue to date exceeds total costs to date.

### Scenario 1

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £2 | £208 | −£206 | −£206 |
| 2 | £2 | £208 | −£206 | −£412 |
| 3 | £2 | £208 | −£206 | −£618 |
| 4 | £9 | £321 | −£312 | −£930 |
| 5 | £9 | £321 | −£312 | −£1,242 |
| 6 | £9 | £321 | −£312 | −£1,554 |
| 7 | £32 | £448 | −£416 | −£1,970 |
| 8 | £32 | £448 | −£416 | −£2,386 |
| 9 | £32 | £448 | −£416 | −£2,802 |
| 10 | £53 | £569 | −£516 | −£3,318 |
| 11 | £53 | £569 | −£516 | −£3,834 |
| 12 | £53 | £569 | −£516 | **−£4,350** |

M7–9 costs include estimated Stripe of ~£12/month. M10–12 include ~£12/month.

### Scenario 2

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £11 | £208 | −£197 | −£197 |
| 2 | £11 | £208 | −£197 | −£394 |
| 3 | £11 | £208 | −£197 | −£591 |
| 4 | £68 | £321 | −£253 | −£844 |
| 5 | £68 | £321 | −£253 | −£1,097 |
| 6 | £68 | £321 | −£253 | −£1,350 |
| 7 | £298 | £469 | −£171 | −£1,521 |
| 8 | £298 | £469 | −£171 | −£1,692 |
| 9 | £298 | £469 | −£171 | −£1,863 |
| 10 | £680 | £624 | +£56 | −£1,807 |
| 11 | £680 | £624 | +£56 | −£1,751 |
| 12 | £680 | £624 | +£56 | **−£1,695** |

M7–9 costs include estimated Stripe of ~£33/month. M10–12 include ~£67/month. **Monthly cash flow turns positive in month 10** — an improvement over the previous forecast where S2 never achieved positive monthly cash flow in year 1.

### Scenario 3 ✦

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £68 | £208 | −£140 | −£140 |
| 2 | £68 | £208 | −£140 | −£280 |
| 3 | £68 | £208 | −£140 | −£420 |
| 4 | **£676** | £321 | **+£355** | −£65 |
| 5 | £676 | £321 | +£355 | **+£290** |
| 6 | £676 | £321 | +£355 | +£645 |
| 7 | £3,795 | £619 | +£3,176 | +£3,821 |
| 8 | £3,795 | £619 | +£3,176 | +£6,997 |
| 9 | £3,795 | £619 | +£3,176 | +£10,173 |
| 10 | £10,120 | £1,290 | +£8,830 | +£19,003 |
| 11 | £10,120 | £1,290 | +£8,830 | +£27,833 |
| 12 | £10,120 | £1,290 | +£8,830 | **+£36,663** |

M7–9 costs include estimated Stripe of ~£183/month. M10–12 include ~£733/month.

✦ **Monthly cash flow turns positive in month 4. Cumulative break-even achieved in month 5** — two months earlier than the previous EP1-based forecast where break-even was month 7.

### Scenario 4

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | **£353** | £208 | **+£145** | +£145 |
| 2 | £353 | £208 | +£145 | +£290 |
| 3 | £353 | £208 | +£145 | +£435 |
| 4 | £4,236 | £321 | +£3,915 | +£4,350 |
| 5 | £4,236 | £321 | +£3,915 | +£8,265 |
| 6 | £4,236 | £321 | +£3,915 | +£12,180 |
| 7 | £26,450 | £1,486 | +£24,964 | +£37,144 |
| 8 | £26,450 | £1,486 | +£24,964 | +£62,108 |
| 9 | £26,450 | £1,486 | +£24,964 | +£87,072 |
| 10 | £79,350 | £4,557 | +£74,793 | +£161,865 |
| 11 | £79,350 | £4,557 | +£74,793 | +£236,658 |
| 12 | £79,350 | £4,557 | +£74,793 | **+£311,451** |

M7–9 costs include estimated Stripe of ~£1,050/month + £300/month extra infra. M10–12 include ~£3,000/month Stripe + £700/month extra infra.

**Monthly cash flow positive from month 1. Never dips into cumulative deficit.** Month 1 surplus is now £145 (was £26 under EP1), providing a stronger buffer from day one.

---

## Break-Even Analysis

### Monthly Break-Even MAU

The number of monthly active shoppers at which revenue covers that month's running costs, shown at representative cost levels for each quarter.

| Quarter | Monthly costs | S1 metrics | S2 metrics | S3 metrics | S4 metrics |
|---------|--------------|-----------|-----------|-----------|-----------|
| Q1 | £208 | 86,667 MAU | 35,972 MAU | 12,303 MAU | 5,874 MAU |
| Q2 | £321 | 13,957 MAU | 5,634 MAU | 1,900 MAU | 907 MAU |
| Q3 | £436 | 12,451 MAU | 5,127 MAU | 1,722 MAU | 823 MAU |
| Q4 | £557 | 15,914 MAU | 6,553 MAU | 2,201 MAU | 1,053 MAU |

*(Q4 costs include baseline Stripe at the cost forecast's base-case creator volumes.)*

The key insight: at Scenario 3 engagement levels (5% CTR, 2.5% conversion, £75 AOV), LYKE needs only around **1,700–2,200 MAU** to cover its monthly running costs — down from 2,200–2,750 under the previous EP1 plan. This is a genuinely achievable target and an important early milestone to track.

At Scenario 2 engagement levels, the break-even MAU sits at ~5,100–6,600 — harder but still realistic. At Scenario 1 levels, the break-even is so high (~12,500–16,000 MAU) that a product-level intervention is likely needed before further scaling.

### Cumulative Break-Even (When Total Revenue Covers All Costs Incurred to Date)

| Scenario | Break-even point |
|----------|-----------------|
| S1 | Does not break even in year 1 (deficit of ~£4,350 at year end) |
| S2 | Does not break even in year 1 (deficit of ~£1,695 at year end; monthly cash flow positive from M10) |
| S3 | **Month 5** — two months earlier than the previous EP1-based forecast |
| S4 | **Month 1 — remains in surplus throughout** |

---

## Sensitivity Analysis

Using Scenario 3 as the base case (40,000 MAU at year end), the table below shows the impact on annual net profit of varying the three most material assumptions independently.

### Varying Click-Through Rate (base: 5%)

| CTR | Annual Revenue | Annual Net P&L |
|-----|--------------|---------------|
| 3% | £26,386 | +£19,619 |
| 4% | £35,181 | +£28,414 |
| **5% (base)** | **£43,976** | **+£37,209** |
| 6% | £52,771 | +£46,004 |
| 7% | £61,566 | +£54,799 |

### Varying Conversion Rate (base: 2.5%)

| Conversion | Annual Revenue | Annual Net P&L |
|-----------|--------------|---------------|
| 1.5% | £26,386 | +£19,619 |
| 2.0% | £35,181 | +£28,414 |
| **2.5% (base)** | **£43,976** | **+£37,209** |
| 3.0% | £52,771 | +£46,004 |
| 3.5% | £61,566 | +£54,799 |

*(CTR and conversion are proportionally equivalent in the model, so their sensitivity tables mirror each other.)*

### Varying Year-End MAU (base: 40,000)

| Year-end MAU | Annual Revenue | Annual Net P&L |
|-------------|--------------|---------------|
| 10,000 | £11,386 | +£4,619 |
| 20,000 | £22,165 | +£15,398 |
| **40,000 (base)** | **£43,976** | **+£37,209** |
| 60,000 | £65,748 | +£58,981 |
| 80,000 | £87,520 | +£80,753 |

### Varying Average Order Value (base: £75)

| AOV | LYKE net/conv | Annual Revenue | Annual Net P&L |
|-----|--------------|--------------|---------------|
| £50 | £0.90 | £29,317 | +£22,550 |
| £65 | £1.17 | £38,112 | +£31,345 |
| **£75 (base)** | **£1.35** | **£43,976** | **+£37,209** |
| £90 | £1.62 | £52,771 | +£46,004 |
| £110 | £1.98 | £64,497 | +£57,730 |

UK fashion average order values tend to be higher for dresses and outerwear (£100+) and lower for basics and accessories (£30–50). LYKE's body-profile targeting may naturally skew towards fit-sensitive categories (jeans, dresses, knitwear) where AOV sits in the £65–95 range.

---

## Additional Revenue Upside (Not Modelled)

Two further revenue streams exist in the design but have no implemented billing mechanism yet. They are excluded from all projections above but represent meaningful additional income in year 2 and beyond.

### Sponsored Placements

Retailers can pay to promote specific products within the matched feed. The `SponsoredPlacement` entity already exists in the data model with `BudgetAmount` and `SpentAmount` fields. A billing mechanism (Stripe) is needed to actually collect this revenue.

Indicative scale: if 3 retailers each spend £2,000/month on sponsored placements in Q4, that adds £6,000/month — comparable to the entire affiliate revenue from 24,000 MAU at Scenario 3 metrics. Sponsored revenue is high-margin for LYKE since it bypasses the AWIN/creator split.

### Retailer Insight Subscriptions

The retailer portal provides anonymised body profile analytics and fit feedback trends. This is currently gated only by authentication — there is no subscription paywall. A monthly SaaS fee of £300–£2,000 per retailer for data access is a natural model. With 5 retailers on paid plans at £500/month, this adds £30,000/year — again, largely pure margin.

---

## Key Risks and Caveats

| Risk | Impact | Note |
|------|--------|------|
| AWIN programme approval delays | Delayed Q2 revenue | Apply to multiple retailer programmes simultaneously |
| Conversion rate lower than assumed | Revenue scales linearly downward | The body-profile matching thesis is the main driver; test and measure CTR and conversion early |
| Creator churn | Reduced content → reduced engagement → lower MAU | Creator earnings are the main retention lever; payout experience must be excellent |
| AWIN transaction declines (returns) | Revenue reversal after payout | Fashion return rates can be 30–40%; only earnings past the attribution window should be treated as confirmed. See `payout-design.md`. |
| Single retailer concentration risk | Q1–Q2 revenue is entirely dependent on one retailer | Priority for Q2: second retailer on AWIN |
| Infrastructure costs fixed regardless of revenue | Loss in S1 and S2 | Mitigated: Y1 Consumption plan reduces fixed Azure costs by ~£1,488/year vs EP1; Functions now scale to near-zero when idle |
| Stripe fees scaling faster than expected | Erodes creator payout economics | Monitor active creator count monthly; the £2/account/month fee is the dominant Stripe cost |
| Functions cold starts under high load | Slower media processing for creators | Monitor p95 processing time; upgrade to EP1 (~£122/month) if creator experience degrades |
