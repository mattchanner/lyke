export interface DataExportResponse {
  user: UserExportData;
  bodyProfile: BodyProfileExportData | null;
  creator: CreatorExportData | null;
  posts: PostExportData[];
  engagements: EngagementExportData[];
  clickEvents: ClickEventExportData[];
  earnings: EarningExportData[];
  exportedAt: string;
}

export interface UserExportData {
  id: string;
  email: string;
  userName: string;
  phoneNumber: string | null;
  userType: string;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  privacyPolicyAcceptedAt: string | null;
  privacyPolicyVersion: string | null;
  marketingOptIn: boolean;
}

export interface BodyProfileExportData {
  heightCm: number;
  weightKg: number;
  bodyTypeName: string;
  frameSizeName: string | null;
  fitPreferences: string;
  createdAt: string;
}

export interface CreatorExportData {
  displayName: string;
  bio: string | null;
  isVerified: boolean;
  socialLinks: string | null;
  verificationStatus: string;
  createdAt: string;
  posts: PostExportData[];
}

export interface PostExportData {
  id: string;
  title: string | null;
  description: string | null;
  mediaType: string;
  mediaUrls: string | null;
  status: string;
  publishedAt: string | null;
  createdAt: string;
  products: PostProductExportData[];
}

export interface PostProductExportData {
  productName: string;
  sizeWorn: string;
  fitNotes: string | null;
  fitRating: string | null;
  stylingNotes: string | null;
  fitTags: string[];
}

export interface EngagementExportData {
  postId: string;
  type: string;
  createdAt: string;
}

export interface ClickEventExportData {
  postId: string;
  createdAt: string;
  convertedAt: string | null;
}

export interface EarningExportData {
  earningType: string;
  amount: number;
  currency: string;
  status: string;
  paidAt: string | null;
  createdAt: string;
}

export interface ConsentStatusResponse {
  marketingOptIn: boolean;
  privacyPolicyVersion: string | null;
  privacyPolicyAcceptedAt: string | null;
  currentPolicyVersion: string;
  needsReconsent: boolean;
}

export interface UpdateConsentRequest {
  marketingOptIn?: boolean;
  acceptPrivacyPolicy?: boolean;
}
