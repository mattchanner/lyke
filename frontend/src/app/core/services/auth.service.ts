import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpContext } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, from, of, throwError } from 'rxjs';
import { map, switchMap, tap, catchError } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';
import { ApiService } from './api.service';
import { AppInsightsService } from './app-insights.service';
import { StorageService } from './storage.service';
import { ToastService } from './toast.service';
import {
  SUPPRESS_ERROR_TOAST,
  SUPPRESS_AUTH_REDIRECT,
} from '../interceptors/error.interceptor';

// Auth-page requests handle errors inline (no toast, no auto-redirect on 401).
const authPageContext = () =>
  new HttpContext()
    .set(SUPPRESS_ERROR_TOAST, true)
    .set(SUPPRESS_AUTH_REDIRECT, true);
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  SocialLoginRequest,
  UserType,
  UserProfileResponse,
} from '../../models';

interface JwtPayload {
  sub: string;
  email: string;
  user_type: string;
  exp: number;
  iat: number;
}

interface AuthState {
  userId: string | null;
  email: string | null;
  userType: UserType | null;
  profileImageUrl: string | null;
  isEmailVerified: boolean;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const INITIAL_STATE: AuthState = {
  userId: null,
  email: null,
  userType: null,
  profileImageUrl: null,
  isEmailVerified: true,
  isAuthenticated: false,
  isLoading: true,
};

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly appInsights = inject(AppInsightsService);
  private readonly storage = inject(StorageService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  // State using Angular Signals
  private readonly state = signal<AuthState>(INITIAL_STATE);

  // Public computed signals
  readonly userId = computed(() => this.state().userId);
  readonly email = computed(() => this.state().email);
  readonly userType = computed(() => this.state().userType);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly isLoading = computed(() => this.state().isLoading);

  readonly profileImageUrl = computed(() => this.state().profileImageUrl);
  readonly isEmailVerified = computed(() => this.state().isEmailVerified);
  readonly isCreator = computed(() => this.state().userType === UserType.Creator);
  readonly isRetailer = computed(() => this.state().userType === UserType.Retailer);
  readonly isAdmin = computed(() => this.state().userType === UserType.Admin);
  readonly isShopper = computed(() => this.state().userType === UserType.Shopper);

  constructor() {
    this.initializeAuth();
  }

  private async initializeAuth(): Promise<void> {
    try {
      const accessToken = await this.storage.getAccessToken();
      const expiry = await this.storage.getTokenExpiry();

      if (accessToken && expiry) {
        const expiryDate = new Date(expiry);
        if (expiryDate > new Date()) {
          const decoded = this.decodeToken(accessToken);
          if (decoded) {
            this.state.set({
              userId: decoded.sub,
              email: decoded.email,
              userType: decoded.user_type as UserType,
              profileImageUrl: null,
              isEmailVerified: true,
              isAuthenticated: true,
              isLoading: false,
            });
            this.appInsights.setAuthenticatedUser(decoded.sub);
            this.loadProfileImage();
            return;
          }
        } else {
          // Token expired, try refresh
          await this.tryRefreshToken();
          return;
        }
      }
    } catch (error) {
      console.error('Auth initialization error:', error);
    }

    this.state.set({ ...INITIAL_STATE, isLoading: false });
  }

  private loadProfileImage(): void {
    this.api.get<UserProfileResponse>('profile', 'me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.state.update((s) => ({
            ...s,
            profileImageUrl: response.data!.profileImageUrl,
            isEmailVerified: response.data!.isEmailVerified,
          }));
        }
      },
    });
  }

  setProfileImageUrl(url: string | null): void {
    this.state.update((s) => ({ ...s, profileImageUrl: url }));
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  }

  private async tryRefreshToken(): Promise<void> {
    const refreshToken = await this.storage.getRefreshToken();
    if (!refreshToken) {
      this.state.set({ ...INITIAL_STATE, isLoading: false });
      return;
    }

    this.refreshToken(refreshToken).subscribe({
      error: () => {
        this.state.set({ ...INITIAL_STATE, isLoading: false });
      },
    });
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api
      .post<AuthResponse>('auth', 'login', request, { context: authPageContext() })
      .pipe(
        switchMap((response) => {
          if (!response.success || !response.data) {
            return throwError(() => new Error(response.error?.message || 'Login failed'));
          }
          return from(this.handleAuthResponse(response.data)).pipe(
            map(() => response.data!)
          );
        })
      );
  }

  socialLogin(request: SocialLoginRequest): Observable<AuthResponse> {
    return this.api
      .post<AuthResponse>('auth', 'social-login', request, { context: authPageContext() })
      .pipe(
        switchMap((response) => {
          if (!response.success || !response.data) {
            return throwError(() => new Error(response.error?.message || 'Social login failed'));
          }
          return from(this.handleAuthResponse(response.data)).pipe(
            map(() => response.data!)
          );
        })
      );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.api
      .post<AuthResponse>('auth', 'register', request, { context: authPageContext() })
      .pipe(
        switchMap((response) => {
          if (!response.success || !response.data) {
            return throwError(() => new Error(response.error?.message || 'Registration failed'));
          }
          return from(this.handleAuthResponse(response.data)).pipe(
            map(() => response.data!)
          );
        })
      );
  }

  refreshToken(refreshToken: string): Observable<AuthResponse> {
    const request: RefreshTokenRequest = { refreshToken };
    return this.api.post<AuthResponse>('auth', 'refresh', request).pipe(
      switchMap((response) => {
        if (!response.success || !response.data) {
          return throwError(() => new Error(response.error?.message || 'Token refresh failed'));
        }
        return from(this.handleAuthResponse(response.data)).pipe(
          map(() => response.data!)
        );
      }),
      catchError((error) => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<void> {
    return this.api.post<void>('auth', 'forgot-password', request).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.error?.message || 'Request failed');
        }
      })
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.api.post<void>('auth', 'reset-password', request).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.error?.message || 'Password reset failed');
        }
      })
    );
  }

  deleteAccount(): Observable<void> {
    return this.api.delete<void>('auth', 'account').pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.error?.message || 'Account deletion failed');
        }
      })
    );
  }

  resendVerification(): Observable<void> {
    return this.api.post<void>('auth', 'resend-verification', {}).pipe(
      map((response) => {
        if (!response.success) {
          throw new Error(response.error?.message || 'Failed to resend verification');
        }
      })
    );
  }

  async logout(): Promise<void> {
    this.appInsights.clearAuthenticatedUser();
    await this.storage.clearAuthData();
    this.state.set({ ...INITIAL_STATE, isLoading: false });
    await this.router.navigate(['/auth/login']);
    this.toast.info('You have been logged out');
  }

  private async handleAuthResponse(auth: AuthResponse): Promise<void> {
    await Promise.all([
      this.storage.setAccessToken(auth.accessToken),
      this.storage.setRefreshToken(auth.refreshToken),
      this.storage.setTokenExpiry(auth.accessTokenExpiry),
      this.storage.setUserId(auth.userId),
    ]);

    this.state.set({
      userId: auth.userId,
      email: auth.email,
      userType: auth.userType,
      profileImageUrl: null,
      isEmailVerified: true,
      isAuthenticated: true,
      isLoading: false,
    });

    this.appInsights.setAuthenticatedUser(auth.userId);
    this.loadProfileImage();
  }

  refreshSession(): Observable<AuthResponse> {
    return from(this.storage.getRefreshToken()).pipe(
      switchMap((token) => {
        if (!token) {
          return throwError(() => new Error('No refresh token available'));
        }
        return this.refreshToken(token);
      })
    );
  }

  async getAccessToken(): Promise<string | null> {
    const token = await this.storage.getAccessToken();
    const expiry = await this.storage.getTokenExpiry();

    if (!token || !expiry) {
      return null;
    }

    const expiryDate = new Date(expiry);
    const now = new Date();

    // Refresh if token expires in less than 5 minutes
    if (expiryDate.getTime() - now.getTime() < 5 * 60 * 1000) {
      const refreshToken = await this.storage.getRefreshToken();
      if (refreshToken) {
        return new Promise((resolve) => {
          this.refreshToken(refreshToken).subscribe({
            next: (auth) => resolve(auth.accessToken),
            error: () => resolve(null),
          });
        });
      }
      return null;
    }

    return token;
  }
}
