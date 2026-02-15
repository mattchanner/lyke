import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserType } from '../../models';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for auth initialization
  if (authService.isLoading()) {
    return new Promise<boolean>((resolve) => {
      const checkAuth = setInterval(() => {
        if (!authService.isLoading()) {
          clearInterval(checkAuth);
          if (authService.isAuthenticated()) {
            resolve(true);
          } else {
            router.navigate(['/auth/login']);
            resolve(false);
          }
        }
      }, 50);
    });
  }

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/auth/login']);
  return false;
};

function getDefaultRoute(authService: AuthService): string {
  const userType = authService.userType();
  if (userType === UserType.Admin) return '/admin';
  if (userType === UserType.Creator) return '/creator';
  return '/feed';
}

// Guard for routes that should only be accessible when NOT authenticated
export const noAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoading()) {
    return new Promise<boolean>((resolve) => {
      const checkAuth = setInterval(() => {
        if (!authService.isLoading()) {
          clearInterval(checkAuth);
          if (!authService.isAuthenticated()) {
            resolve(true);
          } else {
            router.navigate([getDefaultRoute(authService)]);
            resolve(false);
          }
        }
      }, 50);
    });
  }

  if (!authService.isAuthenticated()) {
    return true;
  }

  router.navigate([getDefaultRoute(authService)]);
  return false;
};
