import {
  EarningStatus,
  EarningType,
  FitRating,
  MediaType,
  PostStatus,
  VerificationStatus,
} from '../enums';

// Creator profile
export interface CreatorProfileResponse {
  id: string;
  displayName: string;
  bio: string | null;
  isVerified: boolean;
  verificationStatus: VerificationStatus;
  socialLinks: Record<string, string> | null;
  totalPosts: number;
  publishedPosts: number;
  draftPosts: number;
  pendingReviewPosts: number;
  createdAt: string;
}

export interface RegisterCreatorRequest {
  displayName: string;
  bio?: string;
  socialLinks?: Record<string, string>;
}

export interface UpdateCreatorProfileRequest {
  displayName?: string;
  bio?: string;
  socialLinks?: Record<string, string>;
}

// Posts
export interface CreatePostRequest {
  title?: string;
  description?: string;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls?: string[];
  products: PostProductRequest[];
}

export interface UpdatePostRequest {
  title?: string;
  description?: string;
  mediaType?: MediaType;
  mediaUrls?: string[];
  thumbnailUrls?: string[];
  products?: PostProductRequest[];
}

export interface PostProductRequest {
  productId: string;
  sizeWorn: string;
  fitRating?: FitRating;
  fitNotes?: string;
  stylingNotes?: string;
  fitTagIds?: number[];
}

export interface CreatorPostResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  status: PostStatus;
  moderationNotes: string | null;
  publishedAt: string | null;
  products: CreatorPostProductResponse[];
  engagements: CreatorPostEngagementResponse;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreatorPostProductResponse {
  id: string;
  productId: string;
  productName: string;
  productImage: string | null;
  productPrice: number;
  productCurrency: string;
  retailerName: string;
  sizeWorn: string;
  fitRating: FitRating | null;
  fitNotes: string | null;
  stylingNotes: string | null;
  fitTags: string[];
}

export interface CreatorPostEngagementResponse {
  views: number;
  likes: number;
  saves: number;
  shares: number;
  clicks: number;
}

export interface CreatorPostsRequest {
  status?: PostStatus;
  page?: number;
  pageSize?: number;
}

// Verification
export interface SubmitVerificationRequest {
  documentUrls: string[];
  notes?: string;
}

export interface VerificationStatusResponse {
  status: VerificationStatus;
  notes: string | null;
  documentUrls: string[] | null;
  requestedAt: string | null;
  reviewedAt: string | null;
  rejectionReason: string | null;
}

// Analytics
export interface CreatorAnalyticsRequest {
  startDate?: string;
  endDate?: string;
}

export interface CreatorAnalyticsResponse {
  summary: AnalyticsSummary;
  topPosts: TopPostAnalytics[];
  dailyMetrics: DailyMetrics[];
}

export interface AnalyticsSummary {
  totalViews: number;
  totalLikes: number;
  totalSaves: number;
  totalShares: number;
  totalClicks: number;
  totalEarnings: number;
  currency: string;
}

export interface TopPostAnalytics {
  postId: string;
  title: string | null;
  thumbnailUrl: string | null;
  views: number;
  likes: number;
  clicks: number;
  earnings: number;
}

export interface DailyMetrics {
  date: string;
  views: number;
  likes: number;
  clicks: number;
  earnings: number;
}

// Earnings
export interface EarningsSummaryResponse {
  totalEarnings: number;
  pendingEarnings: number;
  confirmedEarnings: number;
  paidEarnings: number;
  currency: string;
  minPayoutThreshold: number;
  eligibleForPayout: boolean;
}

export interface EarningDetailResponse {
  id: string;
  earningType: EarningType;
  amount: number;
  currency: string;
  status: EarningStatus;
  postId: string;
  postTitle: string | null;
  productId: string;
  productName: string;
  paidAt: string | null;
  createdAt: string;
}

export interface EarningsHistoryRequest {
  status?: EarningStatus;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
