import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from '../../../app.routes';

// Mock components for testing
@Component({
  template: '<div>Mock Layout Component</div>'
})
class MockLayoutComponent { }

@Component({
  template: '<div>Mock Categorias Component</div>'
})
class MockCategoriasComponent { }

@Component({
  template: '<div>Mock Login Component</div>'
})
class MockLoginComponent { }

@Component({
  template: '<div>Mock Dashboard Component</div>'
})
class MockDashboardComponent { }

describe('Categorias Navigation Integration', () => {
  let router: Router;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MockLayoutComponent,
        MockCategoriasComponent,
        MockLoginComponent,
        MockDashboardComponent
      ],
      providers: [
        provideRouter(routes),
        // Mock auth guard to always return true for testing
        {
          provide: 'authGuard',
          useValue: () => true
        },
        {
          provide: 'guestGuard',
          useValue: () => true
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
  });

  it('should navigate to /referencias/categorias', async () => {
    await router.navigate(['/referencias/categorias']);
    expect(location.path()).toBe('/referencias/categorias');
  });

  it('should have categorias route configured with lazy loading', () => {
    const referenciasRoute = routes.find(route => 
      route.path === '' && route.children
    )?.children?.find(child => child.path === 'referencias');

    expect(referenciasRoute).toBeDefined();
    expect(referenciasRoute?.children).toBeDefined();

    const categoriasRoute = referenciasRoute?.children?.find(child => 
      child.path === 'categorias'
    );

    expect(categoriasRoute).toBeDefined();
    expect(categoriasRoute?.loadChildren).toBeDefined();
  });

  it('should have proper route guards applied', () => {
    const mainRoute = routes.find(route => 
      route.path === '' && route.component
    );

    expect(mainRoute).toBeDefined();
    expect(mainRoute?.canActivate).toBeDefined();
  });

  it('should redirect to unidades-medida when accessing /referencias', async () => {
    await router.navigate(['/referencias']);
    expect(location.path()).toBe('/referencias/unidades-medida');
  });
});