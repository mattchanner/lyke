import { UserType } from '../enums';

// Requests
export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  userType?: UserType;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

// Responses
export interface AuthResponse {
  userId: string;
  email: string;
  userType: UserType;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
}
