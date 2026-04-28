import {
  BulkPostAction,
  MediaType,
  PostStatus,
  ReportReason,
  ReportStatus,
  UserType,
  VerificationStatus,
} from '../enums';

// Post moderation
export interface PendingPostResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  status: PostStatus;
  createdAt: string;
  submittedAt: string | null;
  author: AdminAuthorSummary;
  products: PostProductSummary[];
}

export interface AdminAuthorSummary {
  userId: string;
  creatorId: string | null;
  displayName: string;
  isVerified: boolean;
  userType: UserType;
  totalPosts: number;
  publishedPosts: number;
}

export interface PostProductSummary {
  productId: string;
  productName: string;
  sizeWorn: string | null;
  fitNotes: string | null;
}

export interface ModeratePostRequest {
  approve: boolean;
  rejectionReason?: string;
}

export interface PostModerationResponse {
  id: string;
  status: PostStatus;
  moderationNotes: string | null;
  moderatedAt: string | null;
  moderatedByUserId: string | null;
}

// User management
export interface UserListRequest {
  userType?: UserType;
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface UserListResponse {
  id: string;
  email: string | null;
  userName: string | null;
  userType: UserType;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  suspendedAt: string | null;
  suspensionReason: string | null;
}

export interface UserDetailResponse {
  id: string;
  email: string | null;
  userName: string | null;
  userType: UserType;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  updatedAt: string | null;
  suspendedAt: string | null;
  suspendedByUserId: string | null;
  suspensionReason: string | null;
  hasBodyProfile: boolean;
  isCreator: boolean;
  creatorInfo: CreatorInfo | null;
}

export interface CreatorInfo {
  creatorId: string;
  displayName: string;
  isVerified: boolean;
  verificationStatus: VerificationStatus;
  totalPosts: number;
  publishedPosts: number;
}

export interface SuspendUserRequest {
  reason: string;
}

export interface UserSuspensionResponse {
  userId: string;
  isActive: boolean;
  suspendedAt: string | null;
  suspendedByUserId: string | null;
  suspensionReason: string | null;
}

// Verification management
export interface PendingVerificationResponse {
  creatorId: string;
  displayName: string;
  bio: string | null;
  socialLinks: Record<string, string> | null;
  status: VerificationStatus;
  notes: string | null;
  documentUrls: string[] | null;
  requestedAt: string | null;
  totalPosts: number;
  publishedPosts: number;
  createdAt: string;
}

export interface ReviewVerificationRequest {
  approve: boolean;
  rejectionReason?: string;
}

// Platform stats
export interface PlatformStatsResponse {
  users: UserStats;
  content: ContentStats;
  verifications: VerificationStats;
  engagement: EngagementStats;
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  suspendedUsers: number;
  shoppers: number;
  creators: number;
  retailers: number;
  admins: number;
  newUsersLast7Days: number;
  newUsersLast30Days: number;
}

export interface ContentStats {
  totalPosts: number;
  publishedPosts: number;
  pendingReviewPosts: number;
  draftPosts: number;
  rejectedPosts: number;
  flaggedPosts: number;
  removedPosts: number;
  totalReports: number;
  pendingReports: number;
  postsLast7Days: number;
  postsLast30Days: number;
}

export interface VerificationStats {
  pendingVerifications: number;
  approvedCreators: number;
  rejectedVerifications: number;
  totalCreators: number;
}

export interface EngagementStats {
  totalViews: number;
  totalLikes: number;
  totalSaves: number;
  totalClicks: number;
  viewsLast7Days: number;
  clicksLast7Days: number;
}

// Content reports
export interface ContentReportResponse {
  id: string;
  postId: string;
  postTitle: string | null;
  reportedByUserId: string;
  reason: ReportReason;
  additionalDetails: string | null;
  status: ReportStatus;
  reviewedByUserId: string | null;
  reviewedAt: string | null;
  reviewNotes: string | null;
  createdAt: string;
}

export interface ContentReportQueryRequest {
  status?: ReportStatus;
  reason?: ReportReason;
  postId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface ReviewContentReportRequest {
  newStatus: ReportStatus;
  reviewNotes?: string;
  postAction?: PostStatus;
}

// Moderation queue
export interface ModerationQueueItemResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  status: PostStatus;
  createdAt: string;
  submittedAt: string | null;
  author: AdminAuthorSummary;
  products: PostProductSummary[];
  reportCount: number;
  topReportReason: ReportReason | null;
  isFlagged: boolean;
  priority: number;
}

// Post review modal
export interface PostReviewData {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  status: PostStatus;
  createdAt: string;
  submittedAt: string | null;
  author: AdminAuthorSummary;
  products: PostProductSummary[];
  // Optional — present on moderation-queue items
  reportCount?: number;
  topReportReason?: ReportReason | null;
  isFlagged?: boolean;
  priority?: number;
}

// Bulk actions
export interface BulkModeratePostsRequest {
  postIds: string[];
  action: BulkPostAction;
  reason?: string;
}

export interface BulkSuspendUsersRequest {
  userIds: string[];
  reason: string;
}

export interface BulkActionResult {
  successCount: number;
  failureCount: number;
  errors: BulkActionError[];
}

export interface BulkActionError {
  id: string;
  error: string;
}

// Analytics
export interface AdminAnalyticsRequest {
  startDate?: string;
  endDate?: string;
}

export interface AdminAnalyticsResponse {
  summary: AdminAnalyticsSummary;
  dailyMetrics: AdminDailyMetrics[];
  topCreators: TopCreatorAnalytics[];
}

export interface AdminAnalyticsSummary {
  newUsers: number;
  postsPublished: number;
  views: number;
  likes: number;
  saves: number;
  clicks: number;
}

export interface AdminDailyMetrics {
  date: string;
  newUsers: number;
  postsPublished: number;
  views: number;
  likes: number;
  saves: number;
  clicks: number;
}

export interface TopCreatorAnalytics {
  creatorId: string;
  displayName: string;
  isVerified: boolean;
  totalEngagements: number;
  views: number;
  likes: number;
  clicks: number;
}
