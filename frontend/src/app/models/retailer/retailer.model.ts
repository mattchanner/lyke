// Retailer profile
export interface RetailerProfileResponse {
  id: string;
  name: string;
  logoUrl: string | null;
  websiteUrl: string | null;
  contactEmail: string | null;
  isActive: boolean;
  totalProducts: number;
  activeProducts: number;
  totalCampaigns: number;
  activeCampaigns: number;
  createdAt: string;
  affiliateConfig: AffiliateConfigResponse | null;
}

export interface AffiliateConfigResponse {
  baseUrl: string | null;
  affiliateId: string | null;
  commissionRate: number | null;
}

export interface RegisterRetailerRequest {
  name: string;
  logoUrl?: string;
  websiteUrl?: string;
  contactEmail?: string;
}

export interface UpdateRetailerProfileRequest {
  name?: string;
  logoUrl?: string;
  websiteUrl?: string;
  contactEmail?: string;
  affiliateConfig?: AffiliateConfigRequest;
}

export interface AffiliateConfigRequest {
  baseUrl?: string;
  affiliateId?: string;
  commissionRate?: number;
}

// Products
export interface RetailerProductResponse {
  id: string;
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
  clickCount: number;
  conversionCount: number;
  lastSyncedAt: string | null;
  createdAt: string;
}

export interface RetailerProductsRequest {
  category?: string;
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface UpdateRetailerProductRequest {
  name?: string;
  description?: string;
  category?: string;
  subCategory?: string;
  productUrl?: string;
  price?: number;
  currency?: string;
  isActive?: boolean;
}

export interface ImportProductsResponse {
  totalRows: number;
  imported: number;
  updated: number;
  skipped: number;
  failed: number;
  errors: ImportError[];
}

export interface ImportError {
  row: number;
  externalSku: string | null;
  errorMessage: string;
}

// Campaigns
export interface CampaignResponse {
  id: string;
  productId: string | null;
  productName: string | null;
  budgetAmount: number;
  spentAmount: number;
  status: string;
  targetBodyTypes: string[] | null;
  targetCategories: string[] | null;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCampaignRequest {
  productId?: string;
  budgetAmount: number;
  targetBodyTypes?: string[];
  targetCategories?: string[];
  startDate: string;
  endDate: string;
}

export interface UpdateCampaignRequest {
  budgetAmount?: number;
  targetBodyTypes?: string[];
  targetCategories?: string[];
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

export interface CampaignListRequest {
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

// Analytics
export interface RetailerAnalyticsResponse {
  summary: RetailerAnalyticsSummary;
  dailyMetrics: RetailerDailyMetrics[];
  topProducts: TopProductAnalytics[];
  categoryBreakdown: CategoryBreakdown[];
}

export interface RetailerAnalyticsSummary {
  totalViews: number;
  totalClicks: number;
  totalConversions: number;
  totalRevenue: number;
  totalPosts: number;
  currency: string;
}

export interface RetailerDailyMetrics {
  date: string;
  views: number;
  clicks: number;
  conversions: number;
  revenue: number;
}

export interface TopProductAnalytics {
  productId: string;
  productName: string;
  category: string;
  views: number;
  clicks: number;
  conversions: number;
  revenue: number;
}

export interface CategoryBreakdown {
  category: string;
  productCount: number;
  postCount: number;
  clicks: number;
  conversions: number;
}

export interface RetailerAnalyticsRequest {
  startDate?: string;
  endDate?: string;
}

// Insights
export interface FitInsightsResponse {
  products: ProductFitSummary[];
}

export interface ProductFitSummary {
  productId: string;
  productName: string;
  category: string;
  totalReviews: number;
  fitRecommendation: string;
  fitDistribution: FitDistribution[];
  sizeBreakdown: SizeFitBreakdown[];
}

export interface FitDistribution {
  rating: string;
  count: number;
  percentage: number;
}

export interface SizeFitBreakdown {
  size: string;
  reviewCount: number;
  averageFit: string;
}

export interface BodyProfileInsightsResponse {
  heightBands: InsightBand[];
  weightBands: InsightBand[];
  bodyTypes: InsightBand[];
  fitPreferences: InsightBand[];
  minimumGroupSize: number;
  totalEngagedUsers: number;
}

export interface InsightBand {
  label: string;
  count: number;
  percentage: number;
}
