# Municípios Module

This module provides CRUD functionality for managing Municípios (Municipalities) with UF cascading selection and advanced filtering capabilities.

## Features

### Core Functionality
- **CRUD Operations**: Create, read, update, delete municipalities
- **UF Cascading Selection**: País → UF → Município hierarchical selection
- **Search Functionality**: Search municipalities by name with UF filtering
- **Status Management**: Activate/deactivate municipalities
- **Validation**: Unique IBGE code and unique name within UF

### Advanced Features
- **Cascading Dropdowns**: Automatic loading of UFs based on selected País
- **Real-time Search**: Debounced search with minimum 2 characters
- **Multiple Filters**: Filter by País, UF, and search term simultaneously
- **Responsive Design**: Mobile-first responsive layout
- **Async Validation**: Real-time validation of IBGE codes and names

## Components

### MunicipiosComponent
Main component extending `ReferenceCrudBaseComponent` with municipality-specific functionality.

**Key Features:**
- UF cascading selection in both filters and forms
- Search functionality with debouncing
- Custom validation for IBGE codes and municipality names
- Responsive table with custom columns for UF and País display

### MunicipioService
Service extending `ReferenceCrudService` with municipality-specific operations.

**Key Methods:**
- `obterPorUf(ufId)`: Get municipalities by UF
- `obterComUf()`: Get municipalities with UF information
- `buscarPorNome(nome, ufId?)`: Search municipalities by name with optional UF filter
- `validarCodigoIbgeUnico()`: Validate IBGE code uniqueness
- `validarNomeUnico()`: Validate municipality name uniqueness within UF

## Data Models

### MunicipioDto
```typescript
interface MunicipioDto extends BaseReferenceEntity {
  codigoIbge: string;
  ufId: number;
  uf?: UfDto;
}
```

### CriarMunicipioDto
```typescript
interface CriarMunicipioDto {
  nome: string;
  codigoIbge: string;
  ufId: number;
}
```

### AtualizarMunicipioDto
```typescript
interface AtualizarMunicipioDto {
  nome: string;
  ativo: boolean;
}
```

## Validation Rules

### IBGE Code
- **Required**: Must be provided
- **Format**: Exactly 7 numeric digits
- **Uniqueness**: Must be unique across all municipalities
- **Pattern**: `/^\d{7}$/`

### Municipality Name
- **Required**: Must be provided
- **Length**: 2-100 characters
- **Uniqueness**: Must be unique within the selected UF
- **Trimming**: Automatically trimmed of whitespace

### UF Selection
- **Required**: Must select a valid UF
- **Cascading**: Automatically loads based on selected País
- **Validation**: Ensures UF belongs to selected País

## Usage Examples

### Basic Usage
```typescript
// In a parent component or routing
import { MunicipiosComponent } from './municipios/municipios.component';

// Route configuration
{
  path: 'municipios',
  loadComponent: () => import('./municipios/municipios.component').then(m => m.MunicipiosComponent)
}
```

### Service Usage
```typescript
// Inject the service
constructor(private municipioService: MunicipioService) {}

// Get municipalities by UF
this.municipioService.obterPorUf(ufId).subscribe(municipios => {
  console.log('Municipalities:', municipios);
});

// Search municipalities
this.municipioService.buscarPorNome('São Paulo', ufId).subscribe(results => {
  console.log('Search results:', results);
});
```

## API Endpoints

The service communicates with the following API endpoints:

- `GET /api/referencias/municipios` - Get all municipalities
- `GET /api/referencias/municipios/ativos` - Get active municipalities
- `GET /api/referencias/municipios/uf/{ufId}` - Get municipalities by UF
- `GET /api/referencias/municipios/buscar?nome={nome}&ufId={ufId}` - Search municipalities
- `POST /api/referencias/municipios` - Create municipality
- `PUT /api/referencias/municipios/{id}` - Update municipality
- `DELETE /api/referencias/municipios/{id}` - Delete municipality
- `GET /api/referencias/municipios/validar-codigo-ibge` - Validate IBGE code
- `GET /api/referencias/municipios/validar-nome` - Validate municipality name

## Styling

The component uses PrimeNG components with custom SCSS styling:

- **Responsive Grid**: CSS Grid for filter layout
- **Mobile-First**: Responsive design starting from mobile
- **Custom Validation**: Visual feedback for form validation
- **Loading States**: Visual indicators for async operations
- **Accessibility**: High contrast and reduced motion support

## Dependencies

### Angular Dependencies
- `@angular/common`
- `@angular/forms` (ReactiveFormsModule)
- `rxjs` (operators: map, debounceTime, distinctUntilChanged, switchMap)

### PrimeNG Components
- `InputTextModule`
- `SelectModule`
- `TagModule`
- `TooltipModule`

### Internal Dependencies
- `ReferenceCrudBaseComponent`
- `ReferenceCrudService`
- `UfService`
- `PaisService`
- Reference models and interfaces

## Testing

### Unit Tests
- Component initialization and form creation
- Validation logic for IBGE codes and names
- Cascading dropdown behavior
- Search functionality with debouncing

### Integration Tests
- API service calls and error handling
- Form submission and validation
- Filter interactions and data loading

### E2E Tests
- Complete CRUD workflows
- Cascading selection scenarios
- Search and filter combinations

## Performance Considerations

### Optimization Strategies
- **Debounced Search**: 300ms debounce for search input
- **Lazy Loading**: UFs loaded only when País is selected
- **Caching**: Service-level caching for frequently accessed data
- **Virtual Scrolling**: For large municipality lists (if implemented)

### Memory Management
- Proper subscription cleanup in component destruction
- Signal-based reactive state management
- Efficient change detection with OnPush strategy (if implemented)

## Accessibility

### WCAG Compliance
- **Keyboard Navigation**: Full keyboard support for all interactions
- **Screen Reader Support**: Proper ARIA labels and descriptions
- **High Contrast**: Support for high contrast mode
- **Reduced Motion**: Respects user's motion preferences

### Form Accessibility
- **Required Field Indicators**: Visual and programmatic indication
- **Error Messages**: Associated with form controls via ARIA
- **Focus Management**: Proper focus handling in forms and modals

## Browser Support

- **Modern Browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Mobile Browsers**: iOS Safari 14+, Chrome Mobile 90+
- **Responsive Breakpoints**: 768px (mobile), 1024px (tablet), 1200px+ (desktop)

## Future Enhancements

### Planned Features
- **Bulk Operations**: Import/export municipalities
- **Advanced Search**: Multiple criteria search
- **Geolocation**: Integration with mapping services
- **Audit Trail**: Track changes and modifications
- **Data Validation**: Integration with official IBGE data

### Performance Improvements
- **Virtual Scrolling**: For large datasets
- **Infinite Scrolling**: Progressive data loading
- **Caching Strategy**: Enhanced client-side caching
- **Offline Support**: PWA capabilities for offline usage