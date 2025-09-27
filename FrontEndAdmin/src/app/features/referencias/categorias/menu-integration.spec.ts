import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { LayoutComponent } from '../../layout/layout.component';
import { AuthService } from '../../../core/auth/auth.service';
import { signal } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('Categorias Menu Integration', () => {
  let component: LayoutComponent;
  let fixture: ComponentFixture<LayoutComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['logout'], {
      currentUser: signal({ nome: 'Test User', id: 1 })
    });

    await TestBed.configureTestingModule({
      imports: [LayoutComponent, NoopAnimationsModule],
      providers: [
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Menu Structure', () => {
    it('should have Referencias section in main menu', () => {
      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      expect(referenciasMenu).toBeTruthy();
      expect(referenciasMenu?.icon).toBe('pi pi-database');
      expect(referenciasMenu?.items).toBeTruthy();
    });

    it('should have Categorias item in Referencias menu with correct properties', () => {
      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      const categoriasItem = referenciasMenu?.items?.find(item => item.label === 'Categorias');
      
      expect(categoriasItem).toBeTruthy();
      expect(categoriasItem?.routerLink).toEqual(['/referencias/categorias']);
      expect(categoriasItem?.icon).toBe('pi pi-sitemap');
    });

    it('should include all expected referencias modules', () => {
      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      const expectedModules = [
        { label: 'Categorias', route: '/referencias/categorias', icon: 'pi pi-sitemap' },
        { label: 'Unidades de Medida', route: '/referencias/unidades-medida', icon: 'pi pi-calculator' },
        { label: 'Moedas', route: '/referencias/moedas', icon: 'pi pi-dollar' },
        { label: 'Países', route: '/referencias/paises', icon: 'pi pi-globe' },
        { label: 'Estados', route: '/referencias/ufs', icon: 'pi pi-map' },
        { label: 'Municípios', route: '/referencias/municipios', icon: 'pi pi-map-marker' },
        { label: 'Atividades Agropecuárias', route: '/referencias/atividades-agropecuarias', icon: 'pi pi-briefcase' },
        { label: 'Embalagens', route: '/referencias/embalagens', icon: 'pi pi-box' }
      ];

      expectedModules.forEach(expected => {
        const menuItem = referenciasMenu?.items?.find(item => item.label === expected.label);
        expect(menuItem).toBeTruthy(`${expected.label} should be in Referencias menu`);
        expect(menuItem?.routerLink).toEqual([expected.route]);
        expect(menuItem?.icon).toBe(expected.icon);
      });
    });
  });

  describe('Menu Highlighting', () => {
    it('should highlight Categorias when on categorias page', () => {
      // Mock router URL
      Object.defineProperty(component['router'], 'url', {
        value: '/referencias/categorias',
        writable: true
      });

      component['updateMenuState']();

      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      const categoriasItem = referenciasMenu?.items?.find(item => item.label === 'Categorias');
      
      expect(categoriasItem?.styleClass).toBe('active-menu-item');
      expect(referenciasMenu?.expanded).toBe(true);
      expect(referenciasMenu?.styleClass).toBe('active-parent-menu-item');
    });

    it('should expand Referencias menu when any referencias route is active', () => {
      const testRoutes = [
        '/referencias/categorias',
        '/referencias/unidades-medida',
        '/referencias/moedas'
      ];

      testRoutes.forEach(route => {
        // Reset menu state
        component.mainMenuItems.forEach(item => {
          item.expanded = false;
          item.styleClass = '';
          if (item.items) {
            item.items.forEach(subItem => {
              subItem.styleClass = '';
            });
          }
        });

        Object.defineProperty(component['router'], 'url', {
          value: route,
          writable: true
        });

        component['updateMenuState']();

        const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
        expect(referenciasMenu?.expanded).toBe(true, `Referencias should be expanded for route: ${route}`);
        expect(referenciasMenu?.styleClass).toBe('active-parent-menu-item');
      });
    });

    it('should not highlight Referencias when on non-referencias page', () => {
      Object.defineProperty(component['router'], 'url', {
        value: '/produtos',
        writable: true
      });

      component['updateMenuState']();

      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      expect(referenciasMenu?.expanded).toBe(false);
      expect(referenciasMenu?.styleClass).toBe('');
    });
  });

  describe('User Permissions', () => {
    it('should show menu items when user is authenticated', () => {
      expect(component.user()).toBeTruthy();
      expect(component.mainMenuItems.length).toBeGreaterThan(0);
      
      const referenciasMenu = component.mainMenuItems.find(item => item.label === 'Referências');
      expect(referenciasMenu).toBeTruthy();
    });
  });
});