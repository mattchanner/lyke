import { FitPreference, UserType } from '../enums';

// Responses
export interface UserProfileResponse {
  id: string;
  email: string;
  displayName: string | null;
  userType: UserType;
  hasBodyProfile: boolean;
  profileCompleteness: number;
  createdAt: string;
  profileImageUrl: string | null;
  isEmailVerified: boolean;
}

export interface BodyProfileResponse {
  id: string;
  heightCm: number;
  heightDisplay: string;
  weightKg: number;
  weightDisplay: string;
  bodyTypeId: number;
  bodyTypeName: string;
  frameSizeId: number | null;
  frameSizeName: string | null;
  fitPreferences: FitPreference[];
  needsProfileUpdate: boolean;
  createdAt: string;
  updatedAt: string | null;
  stature?: string;
  build?: string;
}

export interface AnonymizedBodyProfileResponse {
  heightRange: string;
  weightRange: string;
  bodyTypeName: string;
  frameSizeName: string | null;
  fitPreferences: FitPreference[];
}

export interface BodyTypeResponse {
  id: number;
  name: string;
  description: string | null;
  displayOrder: number;
}

export interface FrameSizeResponse {
  id: number;
  name: string;
  description: string | null;
  displayOrder: number;
}

export interface FitPreferenceResponse {
  value: number;
  name: string;
  description: string;
}

// Requests
export interface UpdateProfileRequest {
  email?: string;
  displayName?: string | null;
}

export interface PublicUserProfileResponse {
  userId: string;
  displayName: string;
  bio: string | null;
  isVerified: boolean;
  userType: UserType;
  creatorId: string | null;
  bodyProfile: AnonymizedBodyProfileResponse | null;
  profileImageUrl: string | null;
  publishedPostCount: number;
  followerCount: number;
  joinedAt: string;
}

export interface CreateBodyProfileRequest {
  heightCm: number;
  weightKg: number;
  bodyTypeId: number;
  frameSizeId?: number | null;
  fitPreferences?: FitPreference[];
  stature?: string;
  build?: string;
}

export interface UpdateBodyProfileRequest {
  heightCm?: number;
  weightKg?: number;
  bodyTypeId?: number;
  frameSizeId?: number | null;
  fitPreferences?: FitPreference[];
  stature?: string;
  build?: string;
}
