# Atividades Agropecuárias Module

This module provides CRUD functionality for managing agricultural activities (Atividades Agropecuárias) in the Agriis system.

## Features

### Core CRUD Operations
- **Create**: Add new agricultural activities with code, description, and type
- **Read**: List and view agricultural activities with filtering and search
- **Update**: Edit existing activities (description, type, and status)
- **Delete**: Remove activities (with dependency validation)

### Type Filtering
- Filter activities by type: Agricultura, Pecuária, or Mista
- Type-based grouping in list view
- Visual icons for each activity type

### Advanced Features
- **Grouped View**: Activities organized by type with visual grouping
- **Search**: Search by code or description
- **Validation**: Comprehensive form validation with custom validators
- **Concurrency Control**: Optimistic concurrency with RowVersion
- **Caching**: Service-level caching for improved performance

## Components

### AtividadesAgropecuariasComponent
Main component extending `ReferenceCrudBaseComponent` with specialized features:

- **Type Filter Dropdown**: Filter activities by type
- **Grouped List View**: Visual grouping by activity type
- **Custom Form Fields**: Code, description, and type selection
- **Responsive Design**: Mobile-friendly layout

### AtividadeAgropecuariaService
Service extending `ReferenceCrudService` with additional methods:

- `obterPorTipo(tipo)`: Get activities by type
- `obterAgrupadasPorTipo()`: Get activities grouped by type
- `buscarPorCodigo(codigo)`: Search by code pattern
- `buscarPorDescricao(descricao, tipo?)`: Search by description with optional type filter
- `verificarCodigoUnico(codigo, idExcluir?)`: Check code uniqueness
- `verificarDescricaoUnica(descricao, idExcluir?)`: Check description uniqueness
- `obterParaDropdown(tipo?)`: Get minimal data for dropdowns
- `getTipoDescricao(tipo)`: Get type description for display
- `getTipoOptions()`: Get all type options for dropdowns

## Data Models

### AtividadeAgropecuariaDto
```typescript
interface AtividadeAgropecuariaDto {
  id: number;
  codigo: string;
  nome: string;
  descricao: string;
  tipo: TipoAtividadeAgropecuaria;
  tipoDescricao: string;
  ativo: boolean;
  dataCriacao: Date;
  dataAtualizacao?: Date;
  rowVersion: Uint8Array;
}
```

### TipoAtividadeAgropecuaria Enum
```typescript
enum TipoAtividadeAgropecuaria {
  Agricultura = 1,  // Agriculture (crop cultivation)
  Pecuaria = 2,     // Livestock (animal husbandry)
  Mista = 3         // Mixed (both agriculture and livestock)
}
```

## Validation Rules

### Code (Código)
- **Required**: Must be provided
- **Length**: 2-10 characters
- **Format**: Alphanumeric, uppercase
- **Uniqueness**: Must be unique across all activities
- **Immutable**: Cannot be changed after creation

### Description (Descrição)
- **Required**: Must be provided
- **Length**: 5-200 characters
- **Uniqueness**: Must be unique across all activities

### Type (Tipo)
- **Required**: Must be selected
- **Values**: Agricultura, Pecuária, or Mista

## Usage Examples

### Basic Usage
```typescript
// In a component
constructor(private atividadeService: AtividadeAgropecuariaService) {}

// Get all activities
this.atividadeService.obterAtivos().subscribe(atividades => {
  console.log('Active activities:', atividades);
});

// Filter by type
this.atividadeService.obterPorTipo(TipoAtividadeAgropecuaria.Agricultura)
  .subscribe(atividades => {
    console.log('Agriculture activities:', atividades);
  });
```

### Grouped View
```typescript
// Get activities grouped by type
this.atividadeService.obterAgrupadasPorTipo().subscribe(grouped => {
  console.log('Agriculture:', grouped[TipoAtividadeAgropecuaria.Agricultura]);
  console.log('Livestock:', grouped[TipoAtividadeAgropecuaria.Pecuaria]);
  console.log('Mixed:', grouped[TipoAtividadeAgropecuaria.Mista]);
});
```

### Form Validation
```typescript
// Create form with validation
const form = this.fb.group({
  codigo: ['', [
    Validators.required,
    Validators.minLength(2),
    Validators.maxLength(10),
    this.fieldValidators.alphaNumeric(),
    this.fieldValidators.upperCase()
  ]],
  descricao: ['', [
    Validators.required,
    Validators.minLength(5),
    Validators.maxLength(200)
  ]],
  tipo: [null, Validators.required]
});
```

## Styling

The component uses a comprehensive SCSS file with:

- **Responsive Design**: Mobile-first approach with breakpoints
- **Type-specific Colors**: Different colors for each activity type
- **Grouped Layout**: Visual grouping with headers and badges
- **Interactive Elements**: Hover effects and transitions
- **Form Styling**: Consistent form field styling with validation states

## Testing

Comprehensive test suite covering:

- **Component Creation**: Basic component initialization
- **Form Validation**: All validation rules and edge cases
- **Service Integration**: Service method calls and responses
- **Type Filtering**: Filter functionality and state management
- **Error Handling**: Service error scenarios
- **User Interactions**: Click events and form submissions

## Integration

### Routes
```typescript
// In app routing
{
  path: 'referencias/atividades-agropecuarias',
  loadChildren: () => import('./features/referencias/atividades-agropecuarias/atividades-agropecuarias.routes')
}
```

### Navigation
```typescript
// In navigation menu
{
  label: 'Atividades Agropecuárias',
  icon: 'pi pi-star',
  routerLink: '/referencias/atividades-agropecuarias'
}
```

## Performance Considerations

- **Caching**: 15-minute cache for activities (moderate frequency changes)
- **Lazy Loading**: Component and routes are lazy-loaded
- **Virtual Scrolling**: For large lists (if needed)
- **Debounced Search**: Search input with debounce for performance
- **Optimized Queries**: Separate endpoints for dropdowns and full data

## Accessibility

- **Keyboard Navigation**: Full keyboard support
- **Screen Readers**: Proper ARIA labels and descriptions
- **Color Contrast**: Meets WCAG guidelines
- **Focus Management**: Proper focus handling in forms and lists
- **Error Announcements**: Screen reader announcements for validation errors

## Future Enhancements

- **Bulk Operations**: Select multiple activities for bulk actions
- **Export/Import**: CSV/Excel export and import functionality
- **Activity History**: Track changes and modifications
- **Advanced Filtering**: Multiple filter criteria combination
- **Drag & Drop**: Reorder activities within types