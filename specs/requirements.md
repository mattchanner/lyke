1. Product scope and objectives (non-negotiables)
The app (LYKE) must:
•	Enable shoppers to view fashion content worn by people with genuinely similar body profiles.
•	Reduce purchase uncertainty and reduce multi-size ordering and returns.
•	Provide measurable, attributable commercial value to retailers (conversion, returns, insight).
•	Operate with minimal retailer integration and no sharing of identifiable user data.
Primary user groups:
•	Shoppers (end consumers)
•	Content creators (community contributors)
•	Retailers (B2B insight consumers)
•	Platform admins (moderation and operations)

2. Shopper-facing functional requirements
2.1 User account and profile
•	Users must be able to create an account using email and/or social login.
•	During onboarding, users must be able to define a body profile including:
o	Height (required)
o	Weight (required)
o	Body type/category (required – predefined taxonomy)
o	Fit preferences (optional; e.g. fitted, relaxed)
•	Users must be able to edit or delete profile data at any time.
•	Profile data must not be shared directly with retailers or creators.
2.2 Content feed and discovery
•	The app must display a personalised feed of outfit content matched primarily by:
o	Body profile similarity
o	Garment category
o	Fit relevance
•	Users must be able to filter and refine content by:
o	Product type (e.g. dresses, denim)
o	Retailer/brand
o	Fit feedback tags (e.g. “true to size”, “tight on hips”)
•	Content shown must prioritise relevance over popularity.
2.3 Outfit detail view
•	Each outfit post must display:
o	Creator images/video
o	Creator body profile summary (ranges or anonymised bands, not exact stats)
o	Garment size worn
o	Fit feedback and contextual notes
•	Each post must link directly to live retailer product pages (PDP).
2.4 Commerce and attribution
•	Clicking a product link must:
o	Redirect users to the retailer PDP
o	Track referral source and attribution (affiliate or performance tracking)
•	The system must record outbound clicks, engagement events and conversions where available.

3. Creator-facing functional requirements
3.1 Creator accounts
•	Users must be able to register as creators (with additional verification if required).
•	Creators must be able to manage their profile and connected social accounts.
3.2 Content creation
•	Creators must be able to upload:
o	Images and/or short-form video
o	Multiple tagged products per outfit
•	Creators must supply:
o	Garment size worn
o	Fit notes
o	Optional styling notes
•	Product tagging must support:
o	SKU-level linking
o	Multiple retailers per post (where applicable)
3.3 Monetisation
•	Creators must be able to earn revenue via:
o	Affiliate sales
o	Sponsored placements (where enabled)
•	The system must track creator attribution for revenue sharing.
•	Creators must have access to basic performance metrics (views, clicks, earnings).

4. Retailer-facing functional requirements (B2B)
4.1 Retailer onboarding
•	Retailers must be onboarded with minimal technical effort.
•	Product data ingestion must support:
o	Feed-based SKU import (CSV/API)
o	Pricing and availability updates
•	No requirement for deep e-commerce platform integration in MVP.
4.2 Product linking and promotion
•	Retailers must be able to:
o	Promote selected SKUs within matched feeds
o	Define PPC or sponsored placement budgets
•	Targeting must be based on body profile relevance, not demographics.
4.3 Insight and reporting
•	Retailers must have access to dashboards showing aggregated, anonymised data including:
o	Engagement by body profile band
o	Fit feedback trends
o	Conversion vs engagement gaps
o	Return-risk indicators (where signals are available)
•	Data must be exportable for reporting and internal analysis.
•	No retailer should be able to identify individual users or creators beyond agreed attribution.

5. Matching and insight logic (core intelligence)
•	The platform must implement a profile-based matching algorithm that:
o	Prioritises similarity in body profile and garment category
o	Improves accuracy as data density increases
•	The system must support:
o	Aggregation of fit feedback by profile type
o	Longitudinal insight across categories and retailers
•	Matching logic must be configurable and evolvable without full app redeploys.

6. Moderation and quality control
•	All uploaded content must pass moderation before becoming publicly visible.
•	Moderation must support:
o	Manual review (admin tools)
o	Automated flagging (duplicate content, abuse, mis-tagging)
•	Admins must be able to:
o	Remove content
o	Suspend accounts
o	Adjust or correct product tags

7. Data, privacy and compliance requirements
•	The system must be GDPR-first by design.
•	Explicit requirements:
o	No sale of individual user data
o	No sharing of personal profiles with retailers
o	All retailer insight must be aggregated and anonymised
•	Users must have:
o	Clear visibility of how data is used
o	Control over profile visibility
o	Ability to delete their data completely

8. Non-functional requirements
8.1 Performance and scalability
•	The platform must support:
o	Rapid feed loading
o	Scalable content ingestion
o	Growth from single-retailer pilots to multi-retailer ecosystem
8.2 Analytics
•	Full event tracking for:
o	Views
o	Engagement
o	Click-through
o	Conversion signals (where available)
8.3 Architecture
•	Modular architecture to support:
o	Future on-site retailer embeds
o	Additional verticals or categories
o	Expansion to network-level insight

9. MVP boundaries (what not to overbuild yet)
Explicitly out of scope for MVP:
•	AR or virtual try-on
•	Precise body scanning
•	Deep retailer checkout integration
•	Demographic-based targeting
The MVP should focus on:
•	One retailer
•	One high-return category
•	Curated creator cohort
•	Clear measurement of return reduction and conversion lift

