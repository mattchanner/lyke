import {
  EngagementType,
  FeedSortBy,
  FitRating,
  MediaType,
  ReportReason,
  SearchType,
} from '../enums';
import { AnonymizedBodyProfileResponse } from '../profile';

// Creator summaries
export interface CreatorSummaryResponse {
  id: string;
  displayName: string;
  isVerified: boolean;
  bodyProfile: AnonymizedBodyProfileResponse | null;
  profileImageUrl: string | null;
}

export interface CreatorDetailResponse {
  id: string;
  displayName: string;
  bio: string | null;
  isVerified: boolean;
  bodyProfile: AnonymizedBodyProfileResponse | null;
  totalPosts: number;
  profileImageUrl: string | null;
}

// Product summaries
export interface PostProductSummaryResponse {
  id: string;
  productId: string;
  productName: string;
  productImageUrl: string | null;
  price: number;
  currency: string;
  sizeWorn: string;
  fitRating: FitRating | null;
  fitNotes: string | null;
  fitTags: string[];
}

export interface PostProductDetailResponse {
  id: string;
  productId: string;
  productName: string;
  productDescription: string | null;
  productImageUrls: string[];
  productUrl: string;
  price: number;
  currency: string;
  retailerName: string;
  sizeWorn: string;
  fitRating: FitRating | null;
  fitNotes: string | null;
  stylingNotes: string | null;
  fitTags: FitTagResponse[];
}

export interface FitTagResponse {
  id: number;
  name: string;
  category: string | null;
}

// Engagement
export interface EngagementCountsResponse {
  views: number;
  likes: number;
  saves: number;
  shares: number;
}

// Post responses
export interface FeedPostResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  creator: CreatorSummaryResponse;
  products: PostProductSummaryResponse[];
  engagements: EngagementCountsResponse;
  similarityScore: number;
  isLiked: boolean;
  isSaved: boolean;
  publishedAt: string;
}

export interface PostDetailResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  creator: CreatorDetailResponse;
  products: PostProductDetailResponse[];
  engagements: EngagementCountsResponse;
  similarityScore: number;
  isLiked: boolean;
  isSaved: boolean;
  publishedAt: string;
  createdAt: string;
}

// Requests
export interface FeedRequest {
  page?: number;
  pageSize?: number;
  category?: string;
  retailerId?: string;
  fitTagIds?: number[];
  sortBy?: FeedSortBy;
}

export interface EngageRequest {
  type: EngagementType;
}

export interface CreateReportRequest {
  reason: ReportReason;
  additionalDetails?: string;
}

export interface SearchRequest {
  query: string;
  page?: number;
  pageSize?: number;
  type?: SearchType;
}

// Search responses
export interface SearchResponse {
  posts: FeedPostResponse[];
  products: ProductSearchResult[];
  creators: CreatorSearchResult[];
  totalResults: number;
}

export interface ProductSearchResult {
  id: string;
  name: string;
  imageUrl: string | null;
  price: number;
  currency: string;
  retailerName: string;
  postCount: number;
}

export interface CreatorSearchResult {
  id: string;
  displayName: string;
  isVerified: boolean;
  postCount: number;
}
