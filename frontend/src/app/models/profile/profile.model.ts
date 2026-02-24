import { FitPreference, UserType } from '../enums';

// Responses
export interface UserProfileResponse {
  id: string;
  email: string;
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
}

export interface CreateBodyProfileRequest {
  heightCm: number;
  weightKg: number;
  bodyTypeId: number;
  frameSizeId?: number | null;
  fitPreferences?: FitPreference[];
}

export interface UpdateBodyProfileRequest {
  heightCm?: number;
  weightKg?: number;
  bodyTypeId?: number;
  frameSizeId?: number | null;
  fitPreferences?: FitPreference[];
}
