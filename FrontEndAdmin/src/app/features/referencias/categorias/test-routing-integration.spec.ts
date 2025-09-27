import { 
  validateMainRouting, 
  validateCategoriasRoutes, 
  validateRouteGuards, 
  validateNavigationPath 
} from './validate-routing';

describe('Categorias Routing Integration', () => {
  describe('Main Routing Configuration', () => {
    it('should have categorias route properly integrated in main routing', () => {
      expect(validateMainRouting()).toBe(true);
    });
  });

  describe('Categorias Routes Configuration', () => {
    it('should have valid categorias routes configuration', () => {
      expect(validateCategoriasRoutes()).toBe(true);
    });
  });

  describe('Route Guards', () => {
    it('should have proper route guards applied', () => {
      expect(validateRouteGuards()).toBe(true);
    });
  });

  describe('Navigation Path', () => {
    it('should have correct navigation path structure', () => {
      expect(validateNavigationPath()).toBe(true);
    });
  });
});