# Revenue Forecast — Year 1

**Created:** 2026-02-28
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
| M1–3 | £327 | Azure dev+prod + subscriptions |
| M4–6 | £438 | Redis added; AWIN live |
| M7–9 | £558 | Base infrastructure; Stripe fees added separately |
| M10–12 | £695 | Full infrastructure; Stripe fees added separately |

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
| Azure infrastructure | £5,814 | £5,814 | £5,814 | £8,814 |
| Subscriptions (GitHub, Apple, domain) | £241 | £241 | £241 | £241 |
| Stripe Connect fees | £100 | £800 | £2,200 | £9,000 |
| **Total annual costs** | **£6,155** | **£6,855** | **£8,255** | **£18,055** |

### Net Profit / Loss

| | S1 | S2 | S3 | S4 |
|--|----|----|----|----|
| Annual revenue | £287 | £3,172 | £43,976 | £331,167 |
| Annual costs | £6,155 | £6,855 | £8,255 | £18,055 |
| **Net year 1 P&L** | **−£5,868** | **−£3,683** | **+£35,721** | **+£313,112** |

One-time setup costs (Mac Mini £540, Google Play £20) are not included in the P&L above as they are capital items rather than operating expenditure.

---

## Monthly Cash Flow by Scenario

Positive monthly cash flow means LYKE's revenue in that month covers that month's operating costs. Cumulative surplus means total revenue to date exceeds total costs to date.

### Scenario 1

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £2 | £327 | −£325 | −£325 |
| 2 | £2 | £327 | −£325 | −£650 |
| 3 | £2 | £327 | −£325 | −£975 |
| 4 | £9 | £438 | −£429 | −£1,404 |
| 5 | £9 | £438 | −£429 | −£1,833 |
| 6 | £9 | £438 | −£429 | −£2,262 |
| 7 | £32 | £570 | −£538 | −£2,800 |
| 8 | £32 | £570 | −£538 | −£3,338 |
| 9 | £32 | £570 | −£538 | −£3,876 |
| 10 | £53 | £707 | −£654 | −£4,530 |
| 11 | £53 | £707 | −£654 | −£5,184 |
| 12 | £53 | £707 | −£654 | **−£5,838** |

M7–9 costs include estimated Stripe of ~£12/month. M10–12 include ~£12/month.

### Scenario 2

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £11 | £327 | −£316 | −£316 |
| 2 | £11 | £327 | −£316 | −£632 |
| 3 | £11 | £327 | −£316 | −£948 |
| 4 | £68 | £438 | −£370 | −£1,318 |
| 5 | £68 | £438 | −£370 | −£1,688 |
| 6 | £68 | £438 | −£370 | −£2,058 |
| 7 | £298 | £591 | −£293 | −£2,351 |
| 8 | £298 | £591 | −£293 | −£2,644 |
| 9 | £298 | £591 | −£293 | −£2,937 |
| 10 | £680 | £762 | −£82 | −£3,019 |
| 11 | £680 | £762 | −£82 | −£3,101 |
| 12 | £680 | £762 | −£82 | **−£3,183** |

M7–9 costs include estimated Stripe of ~£33/month. M10–12 include ~£67/month.

### Scenario 3 ✦

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | £68 | £327 | −£259 | −£259 |
| 2 | £68 | £327 | −£259 | −£518 |
| 3 | £68 | £327 | −£259 | −£777 |
| 4 | **£676** | £438 | **+£238** | −£539 |
| 5 | £676 | £438 | +£238 | −£301 |
| 6 | £676 | £438 | +£238 | −£63 |
| 7 | £3,795 | £741 | +£3,054 | **+£2,991** |
| 8 | £3,795 | £741 | +£3,054 | +£6,045 |
| 9 | £3,795 | £741 | +£3,054 | +£9,099 |
| 10 | £10,120 | £1,428 | +£8,692 | +£17,791 |
| 11 | £10,120 | £1,428 | +£8,692 | +£26,483 |
| 12 | £10,120 | £1,428 | +£8,692 | **+£35,175** |

M7–9 costs include estimated Stripe of ~£183/month. M10–12 include ~£733/month.

✦ **Monthly cash flow turns positive in month 4. Cumulative break-even achieved in month 7.**

### Scenario 4

| Month | Revenue | Costs | Monthly P&L | Cumulative P&L |
|-------|---------|-------|-------------|---------------|
| 1 | **£353** | £327 | **+£26** | +£26 |
| 2 | £353 | £327 | +£26 | +£52 |
| 3 | £353 | £327 | +£26 | +£78 |
| 4 | £4,236 | £438 | +£3,798 | +£3,876 |
| 5 | £4,236 | £438 | +£3,798 | +£7,674 |
| 6 | £4,236 | £438 | +£3,798 | +£11,472 |
| 7 | £26,450 | £1,608 | +£24,842 | +£36,314 |
| 8 | £26,450 | £1,608 | +£24,842 | +£61,156 |
| 9 | £26,450 | £1,608 | +£24,842 | +£85,998 |
| 10 | £79,350 | £4,695 | +£74,655 | +£160,653 |
| 11 | £79,350 | £4,695 | +£74,655 | +£235,308 |
| 12 | £79,350 | £4,695 | +£74,655 | **+£309,963** |

M7–9 costs include estimated Stripe of ~£1,050/month + £300/month extra infra. M10–12 include ~£3,000/month Stripe + £700/month extra infra.

**Monthly cash flow positive from month 1. Never dips into cumulative deficit.**

---

## Break-Even Analysis

### Monthly Break-Even MAU

The number of monthly active shoppers at which revenue covers that month's running costs, shown at representative cost levels for each quarter.

| Quarter | Monthly costs | S1 metrics | S2 metrics | S3 metrics | S4 metrics |
|---------|--------------|-----------|-----------|-----------|-----------|
| Q1 | £327 | 136,250 MAU | 56,552 MAU | 19,345 MAU | 9,235 MAU |
| Q2 | £438 | 19,043 MAU | 7,684 MAU | 2,592 MAU | 1,238 MAU |
| Q3 | £558 | 15,943 MAU | 6,565 MAU | 2,205 MAU | 1,054 MAU |
| Q4 | £695 | 19,857 MAU | 8,176 MAU | 2,747 MAU | 1,314 MAU |

*(Q4 costs include baseline Stripe at the cost forecast's base-case creator volumes.)*

The key insight: at Scenario 3 engagement levels (5% CTR, 2.5% conversion, £75 AOV), LYKE needs only around **2,200–2,600 MAU** to cover its monthly running costs. This is a genuinely achievable target and an important early milestone to track.

At Scenario 2 engagement levels, the break-even MAU rises to ~6,600–8,200 — harder but still realistic. At Scenario 1 levels, the break-even is so high (~16,000–20,000 MAU) that a product-level intervention is likely needed before further scaling.

### Cumulative Break-Even (When Total Revenue Covers All Costs Incurred to Date)

| Scenario | Break-even point |
|----------|-----------------|
| S1 | Does not break even in year 1 |
| S2 | Does not break even in year 1 (deficit of ~£3,200 at year end) |
| S3 | **Early Q3 — approximately month 7** |
| S4 | **Month 1 — remains in surplus throughout** |

---

## Sensitivity Analysis

Using Scenario 3 as the base case (40,000 MAU at year end), the table below shows the impact on annual net profit of varying the three most material assumptions independently.

### Varying Click-Through Rate (base: 5%)

| CTR | Annual Revenue | Annual Net P&L |
|-----|--------------|---------------|
| 3% | £26,386 | +£18,131 |
| 4% | £35,181 | +£26,926 |
| **5% (base)** | **£43,976** | **+£35,721** |
| 6% | £52,771 | +£44,516 |
| 7% | £61,566 | +£53,311 |

### Varying Conversion Rate (base: 2.5%)

| Conversion | Annual Revenue | Annual Net P&L |
|-----------|--------------|---------------|
| 1.5% | £26,386 | +£18,131 |
| 2.0% | £35,181 | +£26,926 |
| **2.5% (base)** | **£43,976** | **+£35,721** |
| 3.0% | £52,771 | +£44,516 |
| 3.5% | £61,566 | +£53,311 |

*(CTR and conversion are proportionally equivalent in the model, so their sensitivity tables mirror each other.)*

### Varying Year-End MAU (base: 40,000)

| Year-end MAU | Annual Revenue | Annual Net P&L |
|-------------|--------------|---------------|
| 10,000 | £11,386 | +£3,131 |
| 20,000 | £22,165 | +£13,910 |
| **40,000 (base)** | **£43,976** | **+£35,721** |
| 60,000 | £65,748 | +£57,493 |
| 80,000 | £87,520 | +£79,265 |

### Varying Average Order Value (base: £75)

| AOV | LYKE net/conv | Annual Revenue | Annual Net P&L |
|-----|--------------|--------------|---------------|
| £50 | £0.90 | £29,317 | +£21,062 |
| £65 | £1.17 | £38,112 | +£29,857 |
| **£75 (base)** | **£1.35** | **£43,976** | **+£35,721** |
| £90 | £1.62 | £52,771 | +£44,516 |
| £110 | £1.98 | £64,497 | +£56,242 |

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
| Infrastructure costs fixed regardless of revenue | Loss in S1 and S2 | Cost reduction opportunity: downgrade Functions EP1 → Y1 (saves ~£1,400/year; see cost forecast) |
| Stripe fees scaling faster than expected | Erodes creator payout economics | Monitor active creator count monthly; the £2/account/month fee is the dominant Stripe cost |
