import { inject } from '@angular/core';
import { HttpContext } from '@angular/common/http';
import { Router, CanActivateFn } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ApiService } from '../services/api.service';
import { SUPPRESS_ERROR_TOAST } from '../interceptors/error.interceptor';
import { UserProfileResponse } from '../../models';

export const onboardingGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const apiService = inject(ApiService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }

  // Admins and retailers don't need body profile onboarding
  if (authService.isAdmin() || authService.isRetailer()) {
    return true;
  }

  // Check if user has completed onboarding (has body profile)
  const suppressToast = { context: new HttpContext().set(SUPPRESS_ERROR_TOAST, true) };

  return apiService.get<UserProfileResponse>('profile', 'me', undefined, suppressToast).pipe(
    map((response) => {
      if (response.success && response.data) {
        if (response.data.hasBodyProfile) {
          return true;
        }
        // Redirect to onboarding if no body profile
        router.navigate(['/onboarding']);
        return false;
      }
      return true;
    }),
    catchError(() => {
      // On error, allow access (fail open for better UX)
      return of(true);
    })
  );
};

// Guard for onboarding page - only accessible if body profile NOT completed
export const needsOnboardingGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const apiService = inject(ApiService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }

  const suppressToast = { context: new HttpContext().set(SUPPRESS_ERROR_TOAST, true) };

  return apiService.get<UserProfileResponse>('profile', 'me', undefined, suppressToast).pipe(
    map((response) => {
      if (response.success && response.data) {
        if (!response.data.hasBodyProfile) {
          return true;
        }
        // Already onboarded, redirect to feed
        router.navigate(['/feed']);
        return false;
      }
      return true;
    }),
    catchError(() => {
      return of(true);
    })
  );
};
