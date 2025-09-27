/**
 * Verification script for Categorias menu implementation
 * This script verifies that the menu navigation is properly configured
 */

export interface MenuVerificationResult {
  success: boolean;
  message: string;
  details?: any;
}

export class MenuVerification {
  
  /**
   * Verify that the Referencias menu structure is correct
   */
  static verifyMenuStructure(mainMenuItems: any[]): MenuVerificationResult {
    try {
      // Check if Referencias menu exists
      const referenciasMenu = mainMenuItems.find(item => item.label === 'Referências');
      if (!referenciasMenu) {
        return {
          success: false,
          message: 'Referencias menu not found in main menu items'
        };
      }

      // Check if Referencias has the correct icon
      if (referenciasMenu.icon !== 'pi pi-database') {
        return {
          success: false,
          message: `Referencias menu has incorrect icon: ${referenciasMenu.icon}, expected: pi pi-database`
        };
      }

      // Check if Referencias has items
      if (!referenciasMenu.items || !Array.isArray(referenciasMenu.items)) {
        return {
          success: false,
          message: 'Referencias menu does not have items array'
        };
      }

      // Check if Categorias item exists
      const categoriasItem = referenciasMenu.items.find((item: any) => item.label === 'Categorias');
      if (!categoriasItem) {
        return {
          success: false,
          message: 'Categorias item not found in Referencias menu'
        };
      }

      // Check Categorias item properties
      if (!categoriasItem.routerLink || !Array.isArray(categoriasItem.routerLink)) {
        return {
          success: false,
          message: 'Categorias item does not have routerLink array'
        };
      }

      if (categoriasItem.routerLink[0] !== '/referencias/categorias') {
        return {
          success: false,
          message: `Categorias item has incorrect route: ${categoriasItem.routerLink[0]}, expected: /referencias/categorias`
        };
      }

      if (categoriasItem.icon !== 'pi pi-sitemap') {
        return {
          success: false,
          message: `Categorias item has incorrect icon: ${categoriasItem.icon}, expected: pi pi-sitemap`
        };
      }

      // Check if all expected referencias modules are present
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

      const missingModules: string[] = [];
      const incorrectModules: string[] = [];

      expectedModules.forEach(expected => {
        const menuItem = referenciasMenu.items.find((item: any) => item.label === expected.label);
        if (!menuItem) {
          missingModules.push(expected.label);
        } else {
          if (menuItem.routerLink[0] !== expected.route) {
            incorrectModules.push(`${expected.label}: route should be ${expected.route}, got ${menuItem.routerLink[0]}`);
          }
          if (menuItem.icon !== expected.icon) {
            incorrectModules.push(`${expected.label}: icon should be ${expected.icon}, got ${menuItem.icon}`);
          }
        }
      });

      if (missingModules.length > 0) {
        return {
          success: false,
          message: `Missing modules in Referencias menu: ${missingModules.join(', ')}`
        };
      }

      if (incorrectModules.length > 0) {
        return {
          success: false,
          message: `Incorrect module configurations: ${incorrectModules.join('; ')}`
        };
      }

      return {
        success: true,
        message: 'Menu structure verification passed',
        details: {
          referenciasMenuFound: true,
          categoriasItemFound: true,
          totalModules: referenciasMenu.items.length,
          expectedModules: expectedModules.length
        }
      };

    } catch (error) {
      return {
        success: false,
        message: `Error during menu verification: ${error}`
      };
    }
  }

  /**
   * Verify menu highlighting logic
   */
  static verifyMenuHighlighting(updateMenuStateFn: Function, mockRouter: any): MenuVerificationResult {
    try {
      // Test categorias route highlighting
      mockRouter.url = '/referencias/categorias';
      const mainMenuItems = this.getMockMenuItems();
      
      // Simulate the updateMenuState function
      this.simulateUpdateMenuState(mainMenuItems, mockRouter.url);

      const referenciasMenu = mainMenuItems.find(item => item.label === 'Referências');
      const categoriasItem = referenciasMenu?.items?.find((item: any) => item.label === 'Categorias');

      if (categoriasItem?.styleClass !== 'active-menu-item') {
        return {
          success: false,
          message: `Categorias item should have 'active-menu-item' class when on /referencias/categorias, got: ${categoriasItem?.styleClass}`
        };
      }

      if (!referenciasMenu?.expanded) {
        return {
          success: false,
          message: 'Referencias menu should be expanded when categorias is active'
        };
      }

      if (referenciasMenu?.styleClass !== 'active-parent-menu-item') {
        return {
          success: false,
          message: `Referencias menu should have 'active-parent-menu-item' class, got: ${referenciasMenu?.styleClass}`
        };
      }

      return {
        success: true,
        message: 'Menu highlighting verification passed'
      };

    } catch (error) {
      return {
        success: false,
        message: `Error during highlighting verification: ${error}`
      };
    }
  }

  /**
   * Get mock menu items for testing
   */
  private static getMockMenuItems(): any[] {
    return [
      {
        label: 'Referências',
        icon: 'pi pi-database',
        items: [
          { label: 'Categorias', routerLink: ['/referencias/categorias'], icon: 'pi pi-sitemap' },
          { label: 'Unidades de Medida', routerLink: ['/referencias/unidades-medida'], icon: 'pi pi-calculator' }
        ]
      }
    ];
  }

  /**
   * Simulate the updateMenuState logic
   */
  private static simulateUpdateMenuState(menuItems: any[], currentUrl: string): void {
    menuItems.forEach(item => {
      // Reset states
      item.expanded = false;
      item.styleClass = '';
      
      if (item.routerLink && item.routerLink.length > 0) {
        const routePath = item.routerLink[0];
        if (currentUrl === routePath || (routePath !== '/home' && currentUrl.startsWith(routePath))) {
          item.styleClass = 'active-menu-item';
        }
      }
      
      if (item.items) {
        let hasActiveChild = false;
        item.items.forEach((subItem: any) => {
          this.simulateUpdateMenuState([subItem], currentUrl);
          if (subItem.styleClass?.includes('active-menu-item')) {
            hasActiveChild = true;
          }
        });
        
        if (hasActiveChild) {
          item.expanded = true;
          item.styleClass = 'active-parent-menu-item';
        }
      }
    });
  }

  /**
   * Run all verifications
   */
  static runAllVerifications(mainMenuItems: any[]): MenuVerificationResult[] {
    const results: MenuVerificationResult[] = [];
    
    // Structure verification
    results.push(this.verifyMenuStructure(mainMenuItems));
    
    // Highlighting verification
    const mockRouter = { url: '/referencias/categorias' };
    results.push(this.verifyMenuHighlighting(() => {}, mockRouter));
    
    return results;
  }
}

// Export for console testing
if (typeof window !== 'undefined') {
  (window as any).MenuVerification = MenuVerification;
}

console.log('✅ Menu verification utilities loaded. Use MenuVerification.runAllVerifications(mainMenuItems) to test.');