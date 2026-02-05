import { FitPreference, UserType } from '../enums';

// Responses
export interface UserProfileResponse {
  id: string;
  email: string;
  userType: UserType;
  hasBodyProfile: boolean;
  profileCompleteness: number;
  createdAt: string;
}

export interface BodyProfileResponse {
  id: string;
  heightCm: number;
  heightDisplay: string;
  weightKg: number;
  weightDisplay: string;
  bodyTypeId: number;
  bodyTypeName: string;
  fitPreference: FitPreference | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface AnonymizedBodyProfileResponse {
  heightRange: string;
  weightRange: string;
  bodyTypeName: string;
  fitPreference: FitPreference | null;
}

export interface BodyTypeResponse {
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
  fitPreference?: FitPreference | null;
}

export interface UpdateBodyProfileRequest {
  heightCm?: number;
  weightKg?: number;
  bodyTypeId?: number;
  fitPreference?: FitPreference | null;
}
