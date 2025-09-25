import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { StorageService } from '../services/storage.service';

/**
 * HTTP interceptor for automatic JWT token attachment
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storageService = inject(StorageService);
  const token = storageService.getToken();

  // Skip token attachment for login and refresh token requests
  const skipAuth = req.url.includes('/auth/login') || 
                   req.url.includes('/auth/refresh') ||
                   req.url.includes('/auth/register');

  if (token && !skipAuth) {
    // Clone the request and add the authorization header
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  }

  return next(req);
};