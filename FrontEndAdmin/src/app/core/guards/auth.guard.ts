import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { RedirectService } from '../services/redirect.service';
import { map, take } from 'rxjs/operators';

/**
 * Authentication guard for route protection
 * Checks if user is authenticated and redirects to login if not
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const redirectService = inject(RedirectService);

  // Check authentication status using the reactive approach
  return authService.getAuthState().pipe(
    take(1),
    map(authState => {
      if (authState.isAuthenticated && authService.isUserAuthenticated()) {
        return true;
      }
      
      // Store the attempted URL for redirect after login
      const currentUrl = router.url;
      redirectService.setRedirectUrl(currentUrl);
      
      // Redirect to login page
      return router.parseUrl('/login');
    })
  );
};

/**
 * Guest guard for routes that should only be accessible to non-authenticated users
 * Redirects authenticated users to the dashboard
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.getAuthState().pipe(
    take(1),
    map(authState => {
      if (authState.isAuthenticated && authService.isUserAuthenticated()) {
        // User is authenticated, redirect to dashboard
        return router.parseUrl('/');
      }
      
      // User is not authenticated, allow access to guest routes
      return true;
    })
  );
};
