# Filtering and Search Implementation

This document describes the filtering and search capabilities implemented for the Categorias component.

## Features Implemented

### 1. Real-time Search Filter
- **Location**: Search input in the filters section
- **Functionality**: Filters categories by name or description in real-time
- **Implementation**: `onSearchTextChange()` method
- **Behavior**: 
  - Case-insensitive search
  - Searches both name and description fields
  - Maintains hierarchy by including parent/child categories when matches are found

### 2. Tipo Filter
- **Location**: Dropdown in the filters section
- **Functionality**: Filters categories by product type (CategoriaProduto enum)
- **Implementation**: `onTipoFilterChange()` method
- **Options**: 
  - "Todos os tipos" (shows all)
  - Sementes
  - Fertilizantes
  - Defensivos
  - Inoculantes
  - Adjuvantes
  - Micronutrientes
  - Outros

### 3. Status Filter
- **Location**: Dropdown in the filters section
- **Functionality**: Filters categories by active/inactive status
- **Implementation**: `onStatusFilterChange()` method (overridden from base component)
- **Options**:
  - "Todas" (shows all)
  - "Ativas" (shows only active categories)
  - "Inativas" (shows only inactive categories)

### 4. Combined Filtering
- **Functionality**: All filters work together using AND logic
- **Implementation**: `aplicarFiltros()` method
- **Behavior**: Categories must match ALL active filter criteria

### 5. Clear Filters
- **Location**: "Limpar Filtros" button in the filters section
- **Functionality**: Resets all filters to default state
- **Implementation**: `limparFiltros()` method
- **Behavior**: 
  - Clears search text
  - Resets tipo filter to null
  - Resets status filter to "todas"

### 6. Active Filters Summary
- **Location**: Below the filter controls
- **Functionality**: Shows which filters are currently active
- **Implementation**: `temFiltrosAtivos()` method and template logic
- **Display**: Shows tags for each active filter with their values

## Technical Implementation

### Key Methods

#### `aplicarFiltros()`
- Central method that applies all active filters
- Processes filters in sequence: status → tipo → search
- Maintains hierarchy relationships
- Updates the tree display

#### `aplicarFiltroTexto()`
- Handles text-based search filtering
- Maintains parent-child relationships in results
- Uses helper methods to include necessary hierarchy nodes

#### `buildHierarchyWithFilters()`
- Builds the hierarchical tree structure from filtered data
- Handles cases where parents might be filtered out
- Ensures proper tree structure for TreeTable display

### Filter State Management
- Uses Angular signals for reactive state management
- `searchText` signal for search input
- `selectedTipoFilter` signal for tipo selection
- `selectedStatusFilter` signal inherited from base component

### Data Flow
1. User interacts with filter controls
2. Event handlers update filter signals
3. `aplicarFiltros()` is called
4. Filtered data is processed and hierarchy is rebuilt
5. TreeTable is updated with new data

## User Experience Features

### Empty States
- Different messages based on whether filters are active
- "Clear Filters" button when no results found with active filters
- "Create New Category" button when no filters are active

### Responsive Design
- Filter controls stack vertically on mobile devices
- Filter tags wrap appropriately on smaller screens
- Maintains usability across all device sizes

### Visual Feedback
- Active filter tags show current filter values
- Clear filters button is disabled when no filters are active
- Loading states during filter operations

## Requirements Compliance

This implementation satisfies all requirements from Requirement 10:

- ✅ 10.1: Real-time search filtering by category name
- ✅ 10.2: Tipo filter dropdown with CategoriaProduto options  
- ✅ 10.3: Status filter (ativo/inativo) functionality
- ✅ 10.4: Combined filter logic with AND operations
- ✅ 10.5: Clear filters functionality to restore full tree view
- ✅ 10.6: Hierarchy maintained when filters are applied
- ✅ 10.7: "Nenhuma categoria encontrada" message when no results

## Testing

The filtering functionality includes comprehensive unit tests covering:
- Individual filter operations
- Combined filter scenarios
- Edge cases and empty states
- Filter state management
- Hierarchy preservation

See `categorias.component.spec.ts` for detailed test cases.