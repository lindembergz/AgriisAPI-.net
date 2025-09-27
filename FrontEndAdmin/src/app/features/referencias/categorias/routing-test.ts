/**
 * Simple test to verify routing imports work correctly
 */

// Test imports
try {
  console.log('Testing imports...');
  
  // Test main routes import
  import('../../../app.routes').then(appRoutes => {
    console.log('✅ Main app routes imported successfully');
    console.log('Routes count:', appRoutes.routes.length);
    
    // Find referencias route
    const mainRoute = appRoutes.routes.find(route => route.path === '' && route.children);
    if (mainRoute) {
      const referenciasRoute = mainRoute.children?.find(child => child.path === 'referencias');
      if (referenciasRoute) {
        const categoriasRoute = referenciasRoute.children?.find(child => child.path === 'categorias');
        if (categoriasRoute) {
          console.log('✅ Categorias route found in main routing');
          console.log('Categorias route config:', {
            path: categoriasRoute.path,
            hasLoadChildren: !!categoriasRoute.loadChildren
          });
        } else {
          console.error('❌ Categorias route not found in referencias children');
        }
      } else {
        console.error('❌ Referencias route not found');
      }
    } else {
      console.error('❌ Main layout route not found');
    }
  }).catch(error => {
    console.error('❌ Failed to import main app routes:', error);
  });
  
  // Test categorias routes import
  import('./categorias.routes').then(categoriasRoutes => {
    console.log('✅ Categorias routes imported successfully');
    console.log('Categorias routes count:', categoriasRoutes.CATEGORIAS_ROUTES.length);
    
    const mainRoute = categoriasRoutes.CATEGORIAS_ROUTES[0];
    console.log('Main categorias route config:', {
      path: mainRoute.path,
      hasComponent: !!mainRoute.component,
      hasData: !!mainRoute.data,
      title: mainRoute.data?.title,
      permissions: mainRoute.data?.permissions
    });
  }).catch(error => {
    console.error('❌ Failed to import categorias routes:', error);
  });
  
  // Test component import
  import('./categorias.component').then(component => {
    console.log('✅ Categorias component imported successfully');
    console.log('Component name:', component.CategoriasComponent.name);
  }).catch(error => {
    console.error('❌ Failed to import categorias component:', error);
  });
  
} catch (error) {
  console.error('❌ Import test failed:', error);
}

export {}; // Make this a module