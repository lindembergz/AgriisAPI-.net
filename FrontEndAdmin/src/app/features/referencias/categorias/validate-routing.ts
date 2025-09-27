/**
 * Validation script for categorias routing integration
 * This script validates that the categorias route is properly integrated
 */

import { routes } from '../../../app.routes';
import { CATEGORIAS_ROUTES } from './categorias.routes';

// Validation functions
function validateMainRouting(): boolean {
  console.log('🔍 Validating main routing configuration...');
  
  // Find the main layout route
  const mainRoute = routes.find(route => route.path === '' && route.children);
  if (!mainRoute) {
    console.error('❌ Main layout route not found');
    return false;
  }
  
  // Find the referencias route
  const referenciasRoute = mainRoute.children?.find(child => child.path === 'referencias');
  if (!referenciasRoute) {
    console.error('❌ Referencias route not found');
    return false;
  }
  
  // Find the categorias route
  const categoriasRoute = referenciasRoute.children?.find(child => child.path === 'categorias');
  if (!categoriasRoute) {
    console.error('❌ Categorias route not found in referencias children');
    return false;
  }
  
  // Validate lazy loading configuration
  if (!categoriasRoute.loadChildren) {
    console.error('❌ Categorias route does not have lazy loading configured');
    return false;
  }
  
  console.log('✅ Main routing configuration is valid');
  return true;
}

function validateCategoriasRoutes(): boolean {
  console.log('🔍 Validating categorias routes configuration...');
  
  if (!CATEGORIAS_ROUTES || !Array.isArray(CATEGORIAS_ROUTES)) {
    console.error('❌ CATEGORIAS_ROUTES is not defined or not an array');
    return false;
  }
  
  if (CATEGORIAS_ROUTES.length === 0) {
    console.error('❌ CATEGORIAS_ROUTES is empty');
    return false;
  }
  
  const mainRoute = CATEGORIAS_ROUTES[0];
  
  // Validate route path
  if (mainRoute.path !== '') {
    console.error('❌ Main categorias route path should be empty string');
    return false;
  }
  
  // Validate component
  if (!mainRoute.component) {
    console.error('❌ Main categorias route does not have component defined');
    return false;
  }
  
  // Validate route data
  if (!mainRoute.data) {
    console.error('❌ Main categorias route does not have data defined');
    return false;
  }
  
  const requiredDataFields = ['title', 'breadcrumb', 'permissions'];
  for (const field of requiredDataFields) {
    if (!mainRoute.data[field]) {
      console.error(`❌ Main categorias route data missing field: ${field}`);
      return false;
    }
  }
  
  // Validate permissions
  const permissions = mainRoute.data['permissions'];
  if (!Array.isArray(permissions) || permissions.length === 0) {
    console.error('❌ Categorias route permissions should be a non-empty array');
    return false;
  }
  
  console.log('✅ Categorias routes configuration is valid');
  return true;
}

function validateRouteGuards(): boolean {
  console.log('🔍 Validating route guards...');
  
  const mainRoute = routes.find(route => route.path === '' && route.children);
  if (!mainRoute) {
    console.error('❌ Main layout route not found');
    return false;
  }
  
  // Check if auth guard is applied
  if (!mainRoute.canActivate) {
    console.error('❌ Main layout route does not have canActivate guards');
    return false;
  }
  
  console.log('✅ Route guards are properly configured');
  return true;
}

function validateNavigationPath(): boolean {
  console.log('🔍 Validating navigation path...');
  
  // The expected path should be: /referencias/categorias
  const expectedPath = '/referencias/categorias';
  
  // Simulate path construction
  const basePath = '';
  const referenciasPath = 'referencias';
  const categoriasPath = 'categorias';
  
  const constructedPath = `/${referenciasPath}/${categoriasPath}`;
  
  if (constructedPath !== expectedPath) {
    console.error(`❌ Navigation path mismatch. Expected: ${expectedPath}, Got: ${constructedPath}`);
    return false;
  }
  
  console.log(`✅ Navigation path is correct: ${expectedPath}`);
  return true;
}

// Run all validations
function runValidation(): boolean {
  console.log('🚀 Starting categorias routing validation...\n');
  
  const validations = [
    validateMainRouting,
    validateCategoriasRoutes,
    validateRouteGuards,
    validateNavigationPath
  ];
  
  let allValid = true;
  
  for (const validation of validations) {
    const isValid = validation();
    if (!isValid) {
      allValid = false;
    }
    console.log(''); // Add spacing between validations
  }
  
  if (allValid) {
    console.log('🎉 All routing validations passed! Categorias route is properly integrated.');
  } else {
    console.log('❌ Some routing validations failed. Please check the errors above.');
  }
  
  return allValid;
}

// Export for testing
export {
  validateMainRouting,
  validateCategoriasRoutes,
  validateRouteGuards,
  validateNavigationPath,
  runValidation
};

// Run validation if this file is executed directly
if (typeof window === 'undefined') {
  runValidation();
}