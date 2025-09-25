import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';

import { LoginComponent } from '../features/autenticacao/login.component';
import { DashboardComponent } from '../features/dashboard/dashboard.component';
import { AuthService } from '../core/auth/auth.service';
import { authGuard } from '../core/guards/auth.guard';
import { StorageService } from '../core/services/storage.service';

// Mock components for testing
@Component({
  template: '<div>Protected Route</div>',
  standalone: true
})
class MockProtectedComponent { }

@Component({
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: []
})
class MockAppComponent { }

describe('Authentication Workflow Integration Tests', () => {
  let fixture: ComponentFixture<MockAppComponent>;
  let router: Router;
  let location: Location;
  let httpMock: HttpTestingController;
  let authService: AuthService;
  let storageService: StorageService;

  const mockAuthResponse = {
    token: 'mock-jwt-token',
    refreshToken: 'mock-refresh-token',
    user: {
      id: 1,
      nome: 'Test User',
      email: 'test@example.com'
    },
    expiresIn: 3600
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MockAppComponent,
        NoopAnimationsModule
      ],
      providers: [
        provideRouter([
          { path: 'login', component: LoginComponent },
          { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
          { path: 'protected', component: MockProtectedComponent, canActivate: [authGuard] },
          { path: '', redirectTo: '/dashboard', pathMatch: 'full' }
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        AuthService,
        StorageService,
        authGuard
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockAppComponent);
    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    storageService = TestBed.inject(StorageService);

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
    // Clear storage after each test
    storageService.clearAll();
  });

  describe('Complete Authentication Flow', () => {
    it('should redirect unauthenticated user to login', async () => {
      // Try to navigate to protected route
      await router.navigate(['/dashboard']);
      fixture.detectChanges();

      // Should be redirected to login
      expect(location.path()).toBe('/login');
    });

    it('should authenticate user and allow access to protected routes', async () => {
      // Navigate to login
      await router.navigate(['/login']);
      fixture.detectChanges();

      // Simulate successful login
      const loginPromise = authService.login('test@example.com', 'password');
      
      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        username: 'test@example.com',
        password: 'password'
      });
      
      req.flush(mockAuthResponse);
      
      await loginPromise;
      fixture.detectChanges();

      // Verify token is stored
      expect(storageService.getToken()).toBe('mock-jwt-token');
      expect(authService.isUserAuthenticated()).toBe(true);

      // Navigate to protected route
      await router.navigate(['/dashboard']);
      fixture.detectChanges();

      // Should be able to access protected route
      expect(location.path()).toBe('/dashboard');
    });

    it('should handle login failure correctly', async () => {
      // Navigate to login
      await router.navigate(['/login']);
      fixture.detectChanges();

      // Simulate failed login
      const loginPromise = authService.login('invalid@example.com', 'wrongpassword');
      
      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
      
      try {
        await loginPromise;
      } catch (error) {
        // Expected to fail
      }

      fixture.detectChanges();

      // Should remain unauthenticated
      expect(authService.isUserAuthenticated()).toBe(false);
      expect(storageService.getToken()).toBeNull();

      // Should still be on login page
      expect(location.path()).toBe('/login');
    });

    it('should logout user and redirect to login', async () => {
      // First authenticate user
      await authService.login('test@example.com', 'password');
      const req = httpMock.expectOne('/api/auth/login');
      req.flush(mockAuthResponse);

      // Navigate to protected route
      await router.navigate(['/dashboard']);
      fixture.detectChanges();
      expect(location.path()).toBe('/dashboard');

      // Logout
      authService.logout();
      fixture.detectChanges();

      // Should be redirected to login
      expect(location.path()).toBe('/login');
      expect(authService.isUserAuthenticated()).toBe(false);
      expect(storageService.getToken()).toBeNull();
    });

    it('should handle token refresh automatically', async () => {
      // Set up expired token
      const expiredToken = 'expired-token';
      storageService.setToken(expiredToken);
      storageService.setRefreshToken('valid-refresh-token');

      // Try to access protected route
      await router.navigate(['/dashboard']);
      fixture.detectChanges();

      // Should attempt token refresh
      const refreshReq = httpMock.expectOne('/api/auth/refresh');
      expect(refreshReq.request.method).toBe('POST');
      
      refreshReq.flush({
        token: 'new-jwt-token',
        refreshToken: 'new-refresh-token',
        expiresIn: 3600
      });

      fixture.detectChanges();

      // Should have new token and access to protected route
      expect(storageService.getToken()).toBe('new-jwt-token');
      expect(location.path()).toBe('/dashboard');
    });

    it('should redirect to login when refresh token is invalid', async () => {
      // Set up expired tokens
      storageService.setToken('expired-token');
      storageService.setRefreshToken('invalid-refresh-token');

      // Try to access protected route
      await router.navigate(['/dashboard']);
      fixture.detectChanges();

      // Refresh should fail
      const refreshReq = httpMock.expectOne('/api/auth/refresh');
      refreshReq.flush({ message: 'Invalid refresh token' }, { status: 401, statusText: 'Unauthorized' });

      fixture.detectChanges();

      // Should be redirected to login
      expect(location.path()).toBe('/login');
      expect(authService.isUserAuthenticated()).toBe(false);
    });
  });

  describe('Route Protection', () => {
    it('should protect all authenticated routes', async () => {
      const protectedRoutes = ['/dashboard', '/protected'];

      for (const route of protectedRoutes) {
        // Clear authentication
        authService.logout();
        
        // Try to access protected route
        await router.navigate([route]);
        fixture.detectChanges();

        // Should be redirected to login
        expect(location.path()).toBe('/login');
      }
    });

    it('should allow access to protected routes when authenticated', async () => {
      // Authenticate user
      await authService.login('test@example.com', 'password');
      const req = httpMock.expectOne('/api/auth/login');
      req.flush(mockAuthResponse);

      const protectedRoutes = [
        { path: '/dashboard', expected: '/dashboard' },
        { path: '/protected', expected: '/protected' }
      ];

      for (const route of protectedRoutes) {
        await router.navigate([route.path]);
        fixture.detectChanges();

        expect(location.path()).toBe(route.expected);
      }
    });
  });

  describe('Session Management', () => {
    it('should persist authentication across page reloads', () => {
      // Simulate stored token from previous session
      storageService.setToken('valid-token');
      storageService.setUser(mockAuthResponse.user);

      // Create new auth service instance (simulating page reload)
      const newAuthService = new AuthService(
        TestBed.inject(HttpTestingController) as any,
        storageService,
        TestBed.inject(MessageService)
      );

      // Should be authenticated
      expect(newAuthService.isUserAuthenticated()).toBe(true);
      expect(newAuthService.currentUser()).toEqual(mockAuthResponse.user);
    });

    it('should handle concurrent requests with token refresh', async () => {
      // Set up expired token
      storageService.setToken('expired-token');
      storageService.setRefreshToken('valid-refresh-token');

      // Make multiple concurrent requests (simulate with refresh token)
      const request1 = authService.refreshToken();
      const request2 = authService.refreshToken();

      // Should only make one refresh request
      const refreshReq = httpMock.expectOne('/api/auth/refresh');
      refreshReq.flush({
        token: 'new-jwt-token',
        refreshToken: 'new-refresh-token',
        expiresIn: 3600
      });

      // Both requests should get the same new token
      const [result1, result2] = await Promise.all([request1, request2]);
      expect(result1.token).toBe('new-jwt-token');
      expect(result2.token).toBe('new-jwt-token');
    });
  });
});