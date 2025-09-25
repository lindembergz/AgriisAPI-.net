import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from '../services/error-handler.service';
import { AuthService } from '../auth/auth.service';
import { StorageService } from '../services/storage.service';

/**
 * Global HTTP error interceptor
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandler = inject(ErrorHandlerService);
  const authService = inject(AuthService);
  const storageService = inject(StorageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle authentication errors
      if (error.status === 401) {
        // Token expired or invalid, try to refresh or logout
        const refreshToken = storageService.getRefreshToken();
        
        if (refreshToken && !req.url.includes('/auth/refresh')) {
          // Try to refresh token
          authService.refreshToken().subscribe({
            error: () => {
              // Refresh failed, logout user
              authService.logout();
            }
          });
        } else {
          // No refresh token or refresh request failed, logout
          authService.logout();
        }
      }

      // Handle other HTTP errors (but don't show toast for 401 as it's handled above)
      if (error.status !== 401) {
        errorHandler.handleHttpError(error);
      }

      // Re-throw the error for component-level handling if needed
      return throwError(() => error);
    })
  );
};