// User & Auth
export enum UserType {
  Shopper = 0,
  Creator = 1,
  Retailer = 2,
  Admin = 3,
}

// Profile
export enum FitPreference {
  Fitted = 0,
  Regular = 1,
  Relaxed = 2,
}

// Post & Content
export enum PostStatus {
  Draft = 0,
  PendingReview = 1,
  Published = 2,
  Rejected = 3,
}

export enum MediaType {
  Image = 0,
  Video = 1,
}

// Engagement
export enum EngagementType {
  View = 0,
  Like = 1,
  Save = 2,
  Share = 3,
}

// Commerce
export enum FitRating {
  TooSmall = 0,
  SlightlySmall = 1,
  TrueToSize = 2,
  SlightlyLarge = 3,
  TooLarge = 4,
}

// Earnings
export enum EarningType {
  Affiliate = 0,
  Sponsored = 1,
}

export enum EarningStatus {
  Pending = 0,
  Confirmed = 1,
  Paid = 2,
}

// Verification
export enum VerificationStatus {
  NotSubmitted = 0,
  Pending = 1,
  Approved = 2,
  Rejected = 3,
}

// Feed & Search (local to DTOs)
export enum FeedSortBy {
  Relevance = 0,
  Recent = 1,
  MostLiked = 2,
}

export enum SearchType {
  All = 0,
  Posts = 1,
  Products = 2,
  Creators = 3,
}
