import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { StorageService } from '../services/storage.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const storage = inject(StorageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = error.error.message;
      } else {
        // Server-side error
        switch (error.status) {
          case 0:
            errorMessage = 'Unable to connect to server. Please check your internet connection.';
            break;
          case 400:
            errorMessage = error.error?.error?.message || 'Invalid request';
            break;
          case 401:
            errorMessage = 'Your session has expired. Please log in again.';
            storage.clearAuthData().then(() => {
              router.navigate(['/auth/login']);
            });
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = error.error?.error?.message || 'The requested resource was not found.';
            break;
          case 409:
            errorMessage = error.error?.error?.message || 'A conflict occurred with the current state.';
            break;
          case 422:
            errorMessage = error.error?.error?.message || 'Validation failed';
            break;
          case 429:
            errorMessage = 'Too many requests. Please wait a moment and try again.';
            break;
          case 500:
          case 502:
          case 503:
          case 504:
            errorMessage = 'Server error. Please try again later.';
            break;
          default:
            errorMessage = error.error?.error?.message || `Error: ${error.status}`;
        }
      }

      // Don't show toast for 401 (handled with redirect) or suppressed errors
      if (error.status !== 401) {
        toast.error(errorMessage);
      }

      return throwError(() => ({
        status: error.status,
        message: errorMessage,
        details: error.error?.error?.details,
      }));
    })
  );
};
