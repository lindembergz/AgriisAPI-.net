# Categorias Module

This module provides category management functionality for the FrontEndAdmin application.

## CategoriaService

The `CategoriaService` extends `ReferenceCrudService` and provides specialized methods for managing product categories with hierarchical structure.

### Key Features

- **Hierarchy Management**: Methods to work with parent-child category relationships
- **Caching Strategy**: Optimized caching for hierarchical data with 15-minute TTL
- **Tree Structure Support**: Integration with PrimeNG TreeTable component
- **Validation**: Circular reference prevention and uniqueness checking
- **Filtering**: Support for filtering by product type and status

### Available Methods

#### Hierarchy Methods
- `obterComHierarquia()`: Get all categories with complete hierarchy
- `obterCategoriasRaiz()`: Get only root categories (no parent)
- `obterSubCategorias(categoriaPaiId)`: Get subcategories of a specific parent

#### Filtering Methods
- `obterPorTipo(tipo)`: Get categories filtered by CategoriaProduto type
- `obterOrdenadas()`: Get categories ordered by the 'ordem' field
- `obterPorNome(nome)`: Get category by exact name

#### Validation Methods
- `existeComNome(nome, idExcluir?)`: Check if category name already exists
- `podeRemover(id)`: Check if category can be removed (no products/subcategories)
- `validarReferenciaCircular(categoriaId, categoriaPaiId)`: Validate circular reference

#### Utility Methods
- `transformToTreeNodes(categorias)`: Transform categories to PrimeNG TreeNode format
- `obterParaDropdown()`: Get categories formatted for dropdown with hierarchy indication
- `obterTiposDisponiveis()`: Get available CategoriaProduto enum options
- `obterLabelTipo(tipo)`: Get human-readable label for product type

### API Endpoints

The service integrates with the following API endpoints:

- `GET /api/referencias/categorias/hierarquia` - Get categories with hierarchy
- `GET /api/referencias/categorias/raiz` - Get root categories
- `GET /api/referencias/categorias/{id}/subcategorias` - Get subcategories
- `GET /api/referencias/categorias/tipo/{tipo}` - Get categories by type
- `GET /api/referencias/categorias/ordenadas` - Get ordered categories
- `GET /api/referencias/categorias/nome/{nome}` - Get category by name
- `GET /api/referencias/categorias/existe-nome` - Check name uniqueness
- `GET /api/referencias/categorias/{id}/pode-remover` - Check if can remove
- `GET /api/referencias/categorias/{id}/validar-pai/{paiId}` - Validate circular reference

### Caching Strategy

The service implements an optimized caching strategy for hierarchical data:

- **TTL**: 15 minutes (longer than base service due to hierarchical complexity)
- **Cache Keys**: Specific keys for hierarchy, root categories, subcategories, and type filters
- **Invalidation**: Automatic cache invalidation on all CUD operations
- **Max Size**: 50 entries to accommodate various filter combinations

### Usage Example

```typescript
import { CategoriaService } from './services/categoria.service';

@Component({...})
export class CategoriasComponent {
  constructor(private categoriaService: CategoriaService) {}

  loadCategoriesTree() {
    this.categoriaService.obterComHierarquia().subscribe(categorias => {
      this.treeNodes = this.categoriaService.transformToTreeNodes(categorias);
    });
  }

  loadDropdownOptions() {
    this.categoriaService.obterParaDropdown().subscribe(options => {
      this.categoriaOptions = options;
    });
  }
}
```

## CategoriasComponent

The `CategoriasComponent` extends `ReferenceCrudBaseComponent` and provides a hierarchical tree interface for category management using PrimeNG TreeTable.

### Key Features

- **TreeTable Display**: Hierarchical visualization with expand/collapse functionality
- **Responsive Design**: Column hiding on mobile and tablet devices
- **CRUD Operations**: Create, read, update, delete with form validation
- **Status Management**: Activate/deactivate categories with visual indicators
- **Filtering**: Status-based filtering (all, active, inactive)
- **Loading States**: Progress indicators and empty state handling

### TreeTable Configuration

The component uses PrimeNG TreeTable with the following columns:

| Column | Header | Width | Mobile | Tablet | Sortable |
|--------|--------|-------|--------|--------|----------|
| nome | Nome | 40% | ✓ | ✓ | ✓ |
| tipo | Tipo | 20% | ✗ | ✓ | ✓ |
| ativo | Status | 15% | ✓ | ✓ | ✓ |
| ordem | Ordem | 10% | ✗ | ✗ | ✓ |
| acoes | Ações | 15% | ✓ | ✓ | ✗ |

### Responsive Behavior

- **Mobile (< 768px)**: Hides Tipo and Ordem columns, stacks action buttons vertically
- **Tablet (768px - 1024px)**: Hides Ordem column only
- **Desktop (> 1024px)**: Shows all columns

### Tree Node Structure

Categories are transformed into TreeNode format for PrimeNG TreeTable:

```typescript
interface TreeNode<CategoriaDto> {
  data: CategoriaDto;           // Category data
  children?: TreeNode[];        // Subcategories
  expanded?: boolean;           // Expansion state
  leaf?: boolean;              // Has no children
  parent?: TreeNode;           // Parent reference
}
```

### Requirements Covered

This component implementation covers the following requirements:

- **1.1**: Hierarchical tree display with TreeTable
- **1.2**: Nome, Tipo, Status, Ordem, Ações columns
- **1.3**: Expand/collapse functionality
- **6.1**: PrimeNG TreeTable component usage
- **6.6**: Responsive column hiding for mobile and tablet

### Requirements Covered (Service)

This service implementation covers the following requirements:

- **7.1**: API integration for category operations
- **7.2**: Create, update, activate/deactivate operations
- **7.3**: Delete operations with validation
- **7.4**: Error handling for HTTP requests
- **8.6**: Caching strategy for performance optimization