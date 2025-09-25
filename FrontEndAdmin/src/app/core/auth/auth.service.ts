import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError, timer } from 'rxjs';
import { map, catchError, tap, switchMap } from 'rxjs/operators';
import { Usuario } from '../../shared/models/user.model';
import { AuthResponse, LoginForm, RefreshTokenRequest, AuthState } from '../../shared/models/auth.model';
import { StorageService } from '../services/storage.service';
import { RedirectService } from '../services/redirect.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private router = inject(Router);
  private http = inject(HttpClient);
  private storageService = inject(StorageService);
  private redirectService = inject(RedirectService);

  // Signals for reactive state management
  currentUser = signal<Usuario | null>(null);
  isAuthenticated = signal<boolean>(false);
  
  // BehaviorSubject for authentication state
  private authState$ = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: undefined,
    token: undefined,
    refreshToken: undefined
  });

  // Timer for automatic token refresh
  private refreshTimer?: any;

  constructor() {
    this.initializeAuth();
  }

  /**
   * Initialize authentication state from storage
   */
  private initializeAuth(): void {
    const token = this.storageService.getToken();
    const user = this.storageService.getUser();
    
    if (token && this.storageService.isTokenValid() && user) {
      this.setAuthState(user, token, this.storageService.getRefreshToken());
      this.scheduleTokenRefresh();
    } else {
      this.clearAuthState();
    }
  }

  /**
   * Login with username and password
   */
  login(username: string, password: string): Observable<AuthResponse> {
    const loginData: LoginForm = {
      username,
      password,
      rememberMe: false
    };

    // For development, use mock login if no API endpoint is available
    if (!environment.production) {
      return this.mockLogin(loginData);
    }

    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, loginData)
      .pipe(
        tap(response => this.handleAuthResponse(response)),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Mock login for development
   */
  private mockLogin(loginData: LoginForm): Observable<AuthResponse> {
    // Simulate API delay
    return timer(1000).pipe(
      switchMap(() => {
        if (loginData.username && loginData.password) {
          const mockResponse: AuthResponse = {
            token: this.generateMockJWT(),
            refreshToken: 'mock-refresh-token-' + Date.now(),
            user: {
              id: 1,
              nome: 'Lindemberg Silva',
              email: loginData.username,
              senha: '',
              telefone: '(11) 99999-9999',
              dataCriacao: new Date(),
              ativo: true
            },
            expiresIn: 3600 // 1 hour
          };
          
          this.handleAuthResponse(mockResponse);
          return [mockResponse];
        } else {
          return throwError(() => new Error('Credenciais inválidas'));
        }
      })
    );
  }

  /**
   * Generate mock JWT token for development
   */
  private generateMockJWT(): string {
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({
      sub: '1',
      email: 'lindemberg@ifarmer.com.br',
      name: 'Lindemberg Silva',
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + 3600 // 1 hour
    }));
    const signature = btoa('mock-signature');
    
    return `${header}.${payload}.${signature}`;
  }

  /**
   * Handle authentication response
   */
  private handleAuthResponse(response: AuthResponse): void {
    this.storageService.setToken(response.token);
    this.storageService.setRefreshToken(response.refreshToken);
    this.storageService.setUser(response.user);
    
    this.setAuthState(response.user, response.token, response.refreshToken);
    this.scheduleTokenRefresh();
    
    // Use redirect service to handle post-login navigation
    this.redirectService.redirectAfterLogin();
  }

  /**
   * Logout user
   */
  logout(): void {
    // Clear refresh timer
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }

    // Call logout API if available
    if (environment.production) {
      this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({
        error: (error) => console.error('Logout API error:', error)
      });
    }

    this.clearAuthState();
    this.router.navigate(['/login']);
  }

  /**
   * Refresh authentication token
   */
  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.storageService.getRefreshToken();
    
    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshData: RefreshTokenRequest = { refreshToken };

    // For development, use mock refresh
    if (!environment.production) {
      return this.mockRefreshToken();
    }

    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/refresh`, refreshData)
      .pipe(
        tap(response => this.handleAuthResponse(response)),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.logout();
          return throwError(() => error);
        })
      );
  }

  /**
   * Mock token refresh for development
   */
  private mockRefreshToken(): Observable<AuthResponse> {
    return timer(500).pipe(
      switchMap(() => {
        const currentUser = this.currentUser();
        if (currentUser) {
          const mockResponse: AuthResponse = {
            token: this.generateMockJWT(),
            refreshToken: 'mock-refresh-token-' + Date.now(),
            user: currentUser,
            expiresIn: 3600
          };
          
          this.handleAuthResponse(mockResponse);
          return [mockResponse];
        } else {
          return throwError(() => new Error('No current user for refresh'));
        }
      })
    );
  }

  /**
   * Check if user is authenticated
   */
  isUserAuthenticated(): boolean {
    return this.isAuthenticated() && this.storageService.isTokenValid();
  }

  /**
   * Get current authentication state
   */
  getAuthState(): Observable<AuthState> {
    return this.authState$.asObservable();
  }

  /**
   * Set authentication state
   */
  private setAuthState(user: Usuario, token: string, refreshToken?: string | null): void {
    this.currentUser.set(user);
    this.isAuthenticated.set(true);
    
    this.authState$.next({
      isAuthenticated: true,
      user,
      token,
      refreshToken: refreshToken || undefined
    });
  }

  /**
   * Clear authentication state
   */
  private clearAuthState(): void {
    this.storageService.clearAll();
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
    
    this.authState$.next({
      isAuthenticated: false,
      user: undefined,
      token: undefined,
      refreshToken: undefined
    });
  }

  /**
   * Schedule automatic token refresh
   */
  private scheduleTokenRefresh(): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }

    const expiration = this.storageService.getTokenExpiration();
    if (expiration) {
      // Refresh token 5 minutes before expiration
      const refreshTime = expiration - Date.now() - (5 * 60 * 1000);
      
      if (refreshTime > 0) {
        this.refreshTimer = setTimeout(() => {
          this.refreshToken().subscribe({
            error: (error) => console.error('Automatic token refresh failed:', error)
          });
        }, refreshTime);
      }
    }
  }
}
