import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserType } from '../../models';

export function roleGuard(...allowedRoles: UserType[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isLoading()) {
      return new Promise<boolean>((resolve) => {
        const checkAuth = setInterval(() => {
          if (!authService.isLoading()) {
            clearInterval(checkAuth);
            resolve(checkRole());
          }
        }, 50);
      });
    }

    function checkRole(): boolean {
      if (!authService.isAuthenticated()) {
        router.navigate(['/auth/login']);
        return false;
      }

      const userType = authService.userType();
      if (userType !== null && allowedRoles.includes(userType)) {
        return true;
      }

      // Redirect to appropriate page based on user type
      router.navigate(['/feed']);
      return false;
    }

    return checkRole();
  };
}

// Convenience guards for common roles
export const creatorGuard: CanActivateFn = roleGuard(UserType.Creator);
export const adminGuard: CanActivateFn = roleGuard(UserType.Admin);
export const creatorOrAdminGuard: CanActivateFn = roleGuard(UserType.Creator, UserType.Admin);
