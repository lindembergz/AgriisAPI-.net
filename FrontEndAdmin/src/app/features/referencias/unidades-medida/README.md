# Unidades de Medida Module

This module provides comprehensive management for Units of Measure (Unidades de Medida) in the Agriis system, including CRUD operations, type filtering, and conversion calculator functionality.

## Features

### Core CRUD Operations
- **Create**: Add new units of measure with symbol, name, type, and conversion factor
- **Read**: List all units with pagination, sorting, and filtering
- **Update**: Modify existing units (name, type, conversion factor, status)
- **Delete**: Remove units (with dependency checking)
- **Activate/Deactivate**: Toggle unit status

### Type Management
- **Type Filtering**: Filter units by type (Peso, Volume, Área, Unidade)
- **Type Validation**: Ensure proper categorization of units
- **Type-based Queries**: Optimized endpoints for type-specific operations

### Conversion Calculator
- **Real-time Conversion**: Convert quantities between units of the same type
- **Interactive Interface**: User-friendly calculator with dropdowns
- **Validation**: Ensures conversions only between compatible unit types
- **Error Handling**: Clear feedback for invalid conversion attempts

### Advanced Features
- **Symbol Uniqueness**: Prevents duplicate unit symbols
- **Name Uniqueness**: Prevents duplicate unit names
- **Conversion Factors**: Support for custom conversion multipliers
- **Responsive Design**: Mobile-friendly interface
- **Caching**: Optimized performance with intelligent caching

## Components

### UnidadesMedidaComponent
Main component extending `ReferenceCrudBaseComponent` with specialized functionality:

```typescript
@Component({
  selector: 'app-unidades-medida',
  // ... configuration
})
export class UnidadesMedidaComponent extends ReferenceCrudBaseComponent<
  UnidadeMedidaDto,
  CriarUnidadeMedidaDto,
  AtualizarUnidadeMedidaDto
>
```

**Key Features:**
- Type filtering dropdown
- Conversion calculator toggle
- Form validation with custom validators
- Responsive table with custom columns
- Real-time conversion calculations

### UnidadeMedidaService
Service extending `ReferenceCrudService` with unit-specific operations:

```typescript
@Injectable({ providedIn: 'root' })
export class UnidadeMedidaService extends ReferenceCrudService<
  UnidadeMedidaDto,
  CriarUnidadeMedidaDto,
  AtualizarUnidadeMedidaDto
>
```

**Additional Methods:**
- `obterPorTipo(tipo)`: Get units by type
- `converter(quantidade, origem, destino)`: Convert between units
- `verificarSimboloUnico(simbolo)`: Check symbol uniqueness
- `obterTipos()`: Get available unit types
- `obterDropdownPorTipo(tipo)`: Get units for dropdowns

## Data Models

### UnidadeMedidaDto
```typescript
interface UnidadeMedidaDto extends BaseReferenceEntity {
  simbolo: string;           // Unique symbol (kg, L, m², etc.)
  tipo: TipoUnidadeMedida;   // Type enum
  fatorConversao?: number;   // Conversion factor (default: 1.0)
}
```

### TipoUnidadeMedida Enum
```typescript
enum TipoUnidadeMedida {
  Peso = 1,     // Weight units (kg, g, t)
  Volume = 2,   // Volume units (L, mL, m³)
  Area = 3,     // Area units (m², ha, km²)
  Unidade = 4   // Count units (un, dz, cx)
}
```

### Create/Update DTOs
```typescript
interface CriarUnidadeMedidaDto {
  simbolo: string;
  nome: string;
  tipo: TipoUnidadeMedida;
  fatorConversao?: number;
}

interface AtualizarUnidadeMedidaDto {
  nome: string;
  tipo: TipoUnidadeMedida;
  fatorConversao?: number;
  ativo: boolean;
}
```

## API Endpoints

### Standard CRUD
- `GET /api/referencias/unidades-medida` - List all units
- `GET /api/referencias/unidades-medida/{id}` - Get unit by ID
- `POST /api/referencias/unidades-medida` - Create new unit
- `PUT /api/referencias/unidades-medida/{id}` - Update unit
- `DELETE /api/referencias/unidades-medida/{id}` - Delete unit
- `PATCH /api/referencias/unidades-medida/{id}/ativar` - Activate unit
- `PATCH /api/referencias/unidades-medida/{id}/desativar` - Deactivate unit

### Specialized Endpoints
- `GET /api/referencias/unidades-medida/tipos` - Get available types
- `GET /api/referencias/unidades-medida/tipo/{tipo}` - Get units by type
- `GET /api/referencias/unidades-medida/tipo/{tipo}/dropdown` - Get units for dropdown
- `GET /api/referencias/unidades-medida/simbolo/{simbolo}` - Get unit by symbol
- `GET /api/referencias/unidades-medida/existe-simbolo/{simbolo}` - Check symbol exists
- `GET /api/referencias/unidades-medida/existe-nome/{nome}` - Check name exists
- `GET /api/referencias/unidades-medida/converter` - Convert between units

## Form Validation

### Symbol Field
- Required
- 1-10 characters
- No whitespace
- Must be unique
- Immutable after creation

### Name Field
- Required
- 2-100 characters
- No special characters
- Must be unique

### Type Field
- Required
- Must be valid enum value
- Determines conversion compatibility

### Conversion Factor
- Optional (defaults to 1.0)
- Must be > 0.000001
- Used for unit conversions

## Conversion Calculator

### Features
- **Interactive Interface**: Dropdown selectors for origin and destination units
- **Real-time Validation**: Ensures units are of the same type
- **Quantity Input**: Numeric input with decimal support
- **Result Display**: Clear presentation of conversion results
- **Error Handling**: User-friendly error messages

### Usage Flow
1. Enter quantity to convert
2. Select origin unit from dropdown
3. Select destination unit from dropdown (filtered by type)
4. Click "Converter" button
5. View conversion result

### Validation Rules
- Quantity must be > 0
- Origin and destination units must be selected
- Units must be of the same type
- Both units must be active

## Styling

### CSS Classes
- `.custom-toolbar` - Toolbar with filters and actions
- `.filter-group` - Type filter controls
- `.conversion-calculator` - Calculator container
- `.conversion-form` - Calculator form layout
- `.conversion-result` - Result display area
- `.tipo-badge` - Type indicator badges

### Responsive Design
- Mobile-first approach
- Collapsible toolbar on small screens
- Stacked form fields on mobile
- Optimized table columns for different screen sizes

## Testing

### Unit Tests
- Component initialization
- Form validation
- CRUD operations
- Type filtering
- Conversion calculations
- Error handling

### Test Coverage
- Form creation and validation
- DTO mapping
- Service method calls
- Error scenarios
- User interactions

## Usage Examples

### Basic CRUD
```typescript
// Create new unit
const novaUnidade: CriarUnidadeMedidaDto = {
  simbolo: 'kg',
  nome: 'Quilograma',
  tipo: TipoUnidadeMedida.Peso,
  fatorConversao: 1
};

// Update existing unit
const atualizarUnidade: AtualizarUnidadeMedidaDto = {
  nome: 'Quilograma Atualizado',
  tipo: TipoUnidadeMedida.Peso,
  fatorConversao: 1.5,
  ativo: true
};
```

### Type Filtering
```typescript
// Filter by weight units
this.service.obterPorTipo(TipoUnidadeMedida.Peso).subscribe(units => {
  console.log('Weight units:', units);
});
```

### Unit Conversion
```typescript
// Convert 10 kg to grams
this.service.converter(10, kgId, gId).subscribe(result => {
  console.log(`${result.quantidadeOriginal} kg = ${result.quantidadeConvertida} g`);
});
```

## Integration

### With Produto Module
- Units used for product measurements
- Dropdown integration for unit selection
- Validation of unit compatibility

### With Embalagem Module
- Package units reference measurement units
- Type-based filtering for appropriate units
- Conversion support for package calculations

### With Base Components
- Extends `ReferenceCrudBaseComponent`
- Uses `ReferenceCrudService` base functionality
- Integrates with shared validation utilities

## Performance Considerations

### Caching Strategy
- 30-minute cache for unit types (rarely change)
- Shared cache for dropdown data
- Invalidation on CRUD operations

### Optimization
- Lazy loading of conversion calculator
- Debounced search inputs
- Minimal data transfer for dropdowns
- Efficient type-based filtering

## Error Handling

### Validation Errors
- Duplicate symbol detection
- Duplicate name detection
- Invalid conversion factor
- Type mismatch in conversions

### Network Errors
- Connection failures
- Server errors
- Timeout handling
- Retry mechanisms

### User Feedback
- Toast notifications for operations
- Inline form validation
- Loading states
- Error message display

## Future Enhancements

### Planned Features
- Bulk import/export of units
- Unit conversion history
- Advanced conversion formulas
- Unit relationship mapping
- Integration with external unit databases

### Scalability
- Support for custom unit types
- Multi-language unit names
- Regional unit preferences
- Advanced conversion algorithms