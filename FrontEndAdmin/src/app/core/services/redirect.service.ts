import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Service to handle redirects after authentication
 */
@Injectable({
  providedIn: 'root'
})
export class RedirectService {
  private readonly REDIRECT_URL_KEY = 'redirectUrl';

  constructor(private router: Router) {}

  /**
   * Store the URL to redirect to after login
   */
  setRedirectUrl(url: string): void {
    if (url && url !== '/login') {
      sessionStorage.setItem(this.REDIRECT_URL_KEY, url);
    }
  }

  /**
   * Get the stored redirect URL
   */
  getRedirectUrl(): string | null {
    return sessionStorage.getItem(this.REDIRECT_URL_KEY);
  }

  /**
   * Clear the stored redirect URL
   */
  clearRedirectUrl(): void {
    sessionStorage.removeItem(this.REDIRECT_URL_KEY);
  }

  /**
   * Redirect to the stored URL or default to home
   */
  redirectAfterLogin(): void {
    const redirectUrl = this.getRedirectUrl();
    this.clearRedirectUrl();
    
    if (redirectUrl) {
      this.router.navigateByUrl(redirectUrl);
    } else {
      this.router.navigate(['/']);
    }
  }
}