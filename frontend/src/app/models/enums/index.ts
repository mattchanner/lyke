// User & Auth
export enum UserType {
  Shopper = "Shopper",
  Creator = "Creator",
  Retailer = "Retailer",
  Admin = "Admin",
}

// Profile
export enum FitPreference {
  Fitted = "Fitted",
  Regular = "Regular",
  Relaxed = "Relaxed",
}

// Post & Content
export enum PostStatus {
  Draft = "Draft",
  PendingReview = "PendingReview",
  Published = "Published",
  Rejected = "Rejected",
}

export enum MediaType {
  Image = "Image",
  Video = "Video",
}

// Engagement
export enum EngagementType {
  View = "View",
  Like = "Like",
  Save = "Save",
  Share = "Share",
}

// Commerce
export enum FitRating {
  TooSmall = "TooSmall",
  SlightlySmall = "SlightlySmall",
  TrueToSize = "TrueToSize",
  SlightlyLarge = "SlightlyLarge",
  TooLarge = "TooLarge",
}

// Earnings
export enum EarningType {
  Affiliate = "Affiliate",
  Sponsored = "Sponsored",
}

export enum EarningStatus {
  Pending = "Pending",
  Confirmed = "Confirmed",
  Paid = "Paid",
}

// Verification
export enum VerificationStatus {
  NotSubmitted = "NotSubmitted",
  Pending = "Pending",
  Approved = "Approved",
  Rejected = "Rejected",
}

// Feed & Search (local to DTOs)
export enum FeedSortBy {
  Relevance = "Relevance",
  Recent = "Recent",
  MostLiked = "MostLiked",
}

export enum SearchType {
  All = "All",
  Posts = "Posts",
  Products = "Products",
  Creators = "Creators",
}
