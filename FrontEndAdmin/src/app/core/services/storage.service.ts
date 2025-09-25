import { Injectable } from '@angular/core';
import { Usuario } from '../../shared/models/user.model';

/**
 * Service for secure token storage and management
 */
@Injectable({
  providedIn: 'root'
})
export class StorageService {
  private readonly TOKEN_KEY = 'agriis_token';
  private readonly REFRESH_TOKEN_KEY = 'agriis_refresh_token';
  private readonly USER_KEY = 'agriis_user';
  private readonly TOKEN_EXPIRY_KEY = 'agriis_token_expiry';

  /**
   * Store JWT token securely with expiration tracking
   */
  setToken(token: string): void {
    try {
      localStorage.setItem(this.TOKEN_KEY, token);
      
      // Extract and store expiration time
      const expiration = this.extractTokenExpiration(token);
      if (expiration) {
        localStorage.setItem(this.TOKEN_EXPIRY_KEY, expiration.toString());
      }
    } catch (error) {
      console.error('Error storing token:', error);
      this.clearToken();
    }
  }

  /**
   * Retrieve JWT token
   */
  getToken(): string | null {
    try {
      return localStorage.getItem(this.TOKEN_KEY);
    } catch (error) {
      console.error('Error retrieving token:', error);
      return null;
    }
  }

  /**
   * Store refresh token securely
   */
  setRefreshToken(refreshToken: string): void {
    try {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    } catch (error) {
      console.error('Error storing refresh token:', error);
    }
  }

  /**
   * Retrieve refresh token
   */
  getRefreshToken(): string | null {
    try {
      return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    } catch (error) {
      console.error('Error retrieving refresh token:', error);
      return null;
    }
  }

  /**
   * Store user data securely
   */
  setUser(user: Usuario): void {
    try {
      // Don't store sensitive data like password
      const safeUser = {
        ...user,
        senha: undefined // Remove password from storage
      };
      localStorage.setItem(this.USER_KEY, JSON.stringify(safeUser));
    } catch (error) {
      console.error('Error storing user data:', error);
      this.clearUser();
    }
  }

  /**
   * Retrieve user data
   */
  getUser(): Usuario | null {
    try {
      const userData = localStorage.getItem(this.USER_KEY);
      return userData ? JSON.parse(userData) : null;
    } catch (error) {
      console.error('Error retrieving user data:', error);
      this.clearUser();
      return null;
    }
  }

  /**
   * Clear all stored authentication data
   */
  clearAll(): void {
    try {
      this.clearToken();
      this.clearRefreshToken();
      this.clearUser();
      this.clearTokenExpiry();
      // Also clear legacy key for backward compatibility
      localStorage.removeItem('ifarmer_user');
    } catch (error) {
      console.error('Error clearing storage:', error);
    }
  }

  /**
   * Clear only token data
   */
  clearToken(): void {
    try {
      localStorage.removeItem(this.TOKEN_KEY);
    } catch (error) {
      console.error('Error clearing token:', error);
    }
  }

  /**
   * Clear only refresh token data
   */
  clearRefreshToken(): void {
    try {
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    } catch (error) {
      console.error('Error clearing refresh token:', error);
    }
  }

  /**
   * Clear only user data
   */
  clearUser(): void {
    try {
      localStorage.removeItem(this.USER_KEY);
    } catch (error) {
      console.error('Error clearing user data:', error);
    }
  }

  /**
   * Clear token expiry data
   */
  private clearTokenExpiry(): void {
    try {
      localStorage.removeItem(this.TOKEN_EXPIRY_KEY);
    } catch (error) {
      console.error('Error clearing token expiry:', error);
    }
  }

  /**
   * Check if token exists and is not expired
   */
  isTokenValid(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    try {
      const expiration = this.getTokenExpiration();
      if (!expiration) {
        return false;
      }

      const currentTime = Date.now();
      const isValid = expiration > currentTime;
      
      // If token is expired, clean it up
      if (!isValid) {
        this.handleTokenExpiration();
      }
      
      return isValid;
    } catch (error) {
      console.error('Error validating token:', error);
      this.handleTokenExpiration();
      return false;
    }
  }

  /**
   * Get token expiration time in milliseconds
   */
  getTokenExpiration(): number | null {
    try {
      // First try to get from stored expiry
      const storedExpiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
      if (storedExpiry) {
        return parseInt(storedExpiry, 10);
      }

      // Fallback to extracting from token
      const token = this.getToken();
      if (!token) {
        return null;
      }

      return this.extractTokenExpiration(token);
    } catch (error) {
      console.error('Error getting token expiration:', error);
      return null;
    }
  }

  /**
   * Extract expiration time from JWT token
   */
  private extractTokenExpiration(token: string): number | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }

      const payload = JSON.parse(atob(parts[1]));
      return payload.exp ? payload.exp * 1000 : null; // Convert to milliseconds
    } catch (error) {
      console.error('Error extracting token expiration:', error);
      return null;
    }
  }

  /**
   * Handle token expiration scenarios
   */
  private handleTokenExpiration(): void {
    console.warn('Token has expired, clearing authentication data');
    this.clearAll();
  }

  /**
   * Check if token will expire soon (within 5 minutes)
   */
  isTokenExpiringSoon(): boolean {
    const expiration = this.getTokenExpiration();
    if (!expiration) {
      return true;
    }

    const currentTime = Date.now();
    const fiveMinutes = 5 * 60 * 1000; // 5 minutes in milliseconds
    
    return (expiration - currentTime) <= fiveMinutes;
  }

  /**
   * Get time until token expires in milliseconds
   */
  getTimeUntilExpiration(): number {
    const expiration = this.getTokenExpiration();
    if (!expiration) {
      return 0;
    }

    const currentTime = Date.now();
    return Math.max(0, expiration - currentTime);
  }

  /**
   * Check if storage is available
   */
  isStorageAvailable(): boolean {
    try {
      const test = '__storage_test__';
      localStorage.setItem(test, test);
      localStorage.removeItem(test);
      return true;
    } catch (error) {
      console.warn('localStorage is not available:', error);
      return false;
    }
  }
}