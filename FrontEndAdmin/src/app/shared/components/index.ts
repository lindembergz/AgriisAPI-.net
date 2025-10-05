/**
 * Shared Components Index
 * Exports all shared components for easy importing
 */

// Base Components
export * from './reference-crud-base/reference-crud-base.component';
export * from './empty-state/empty-state.component';

// New Unified Components
export * from './field-error/field-error.component';
export * from './loading-spinner/loading-spinner.component';
export * from './filter-summary/filter-summary.component';
export * from './responsive-table/responsive-table.component';

// Map and Address Components
export { CoordenadasMapComponent } from './coordenadas-map.component';
export { EnderecoFormComponent } from './endereco-form.component';
export { EnderecoMapComponent } from './endereco-map.component';
export { EnderecoMapExampleComponent } from './endereco-map-example.component';

// Interfaces and Types
export * from '../interfaces/unified-component.interfaces';