import { FitRating, MediaType, PostStatus } from '../enums';

export interface AuthorPostResponse {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: MediaType;
  mediaUrls: string[];
  thumbnailUrls: string[];
  status: PostStatus;
  moderationNotes: string | null;
  publishedAt: string | null;
  products: AuthorPostProductResponse[];
  engagements: AuthorPostEngagementResponse;
  createdAt: string;
  updatedAt: string | null;
}

export interface AuthorPostProductResponse {
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

export interface AuthorPostEngagementResponse {
  views: number;
  likes: number;
  saves: number;
  shares: number;
  clicks: number;
}

export interface AuthorPostsRequest {
  status?: PostStatus;
  page?: number;
  pageSize?: number;
}
