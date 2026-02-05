// Product
export interface ProductResponse {
  id: string;
  retailerId: string;
  retailerName: string;
  retailerLogoUrl: string | null;
  externalSku: string;
  name: string;
  description: string | null;
  category: string;
  subCategory: string | null;
  imageUrls: string[];
  productUrl: string;
  price: number;
  currency: string;
  isActive: boolean;
  postCount: number;
}

export interface ProductSearchRequest {
  query: string;
  retailerId?: string;
  category?: string;
  page?: number;
  pageSize?: number;
}

// Retailer
export interface RetailerResponse {
  id: string;
  name: string;
  logoUrl: string | null;
  websiteUrl: string | null;
  isActive: boolean;
  productCount: number;
  postCount: number;
}

export interface RetailerProductsResponse {
  retailerId: string;
  retailerName: string;
  products: ProductResponse[];
}

// Click tracking
export interface TrackClickRequest {
  postId: string;
  postProductId: string;
  sessionId?: string;
  source?: 'feed' | 'post_detail' | 'search' | 'similar_posts';
  feedPosition?: number;
  searchQuery?: string;
  platform?: 'ios' | 'android' | 'web';
  appVersion?: string;
}

export interface TrackClickResponse {
  clickId: string;
  affiliateUrl: string;
  retailerName: string;
  productName: string;
}

// Conversion webhook (for admin reference)
export interface ConversionWebhookRequest {
  clickId: string;
  orderId: string;
  orderValue: number;
  currency: string;
  commissionAmount: number;
  productSku?: string;
  transactionDate: string;
  signature: string;
}
