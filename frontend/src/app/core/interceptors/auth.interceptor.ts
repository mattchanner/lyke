import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import { StorageService } from '../services/storage.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(StorageService);

  // Only add auth header for our API
  if (!req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  // Skip auth header for public endpoints
  const publicEndpoints = [
    '/auth/v1/login',
    '/auth/v1/register',
    '/auth/v1/forgot-password',
    '/auth/v1/reset-password',
    '/auth/v1/refresh',
  ];

  if (publicEndpoints.some((endpoint) => req.url.includes(endpoint))) {
    return next(req);
  }

  return from(storage.getAccessToken()).pipe(
    switchMap((token) => {
      if (token) {
        const authReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        });
        return next(authReq);
      }
      return next(req);
    })
  );
};
