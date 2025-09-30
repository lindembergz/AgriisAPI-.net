# End-to-End Tests for Reference Components

This directory contains comprehensive end-to-end tests for all reference components as specified in **Task 10.3: Create end-to-end tests**.

## Test Coverage

### 📋 Complete CRUD Workflows
- **Create**: Test form validation, data submission, and success feedback
- **Read**: Test data loading, display, and pagination
- **Update**: Test editing forms, validation, and data persistence
- **Delete**: Test deletion confirmation, dependency checks, and cleanup
- **Activate/Deactivate**: Test status toggle operations and UI updates

### 🔍 Search and Filtering
- **Text Search**: Real-time search across relevant fields
- **Status Filtering**: Active/Inactive status filters
- **Type-Specific Filters**: Category, type, and custom filters
- **Filter Combinations**: Multiple filters working together
- **Clear Filters**: Reset functionality

### 📱 Responsive Design
- **Mobile Viewport** (375px): Touch-friendly interactions, collapsed layouts
- **Tablet Viewport** (768px): Optimized for medium screens
- **Desktop Viewport** (1200px+): Full feature display
- **Layout Adaptation**: Dynamic column hiding, responsive forms
- **Touch Interactions**: Adequate touch targets, gesture support

### 🔄 Navigation Between Components
- **Route Navigation**: Seamless transitions between reference modules
- **State Preservation**: Maintain filters and selections during navigation
- **Deep Linking**: Direct access to specific components and states
- **Browser History**: Proper back/forward navigation support

## Test Files

### `referencias-e2e-workflow.spec.ts`
Comprehensive workflow tests covering:
- **UnidadesMedida Component**: Full CRUD operations, type filtering, conversion calculator
- **UFs Component**: País relationship handling, geographic filtering
- **Moedas Component**: Currency management, validation
- **Países Component**: Country management, dependency checks
- **Embalagens Component**: Package management, unit relationships
- **Cross-Component Integration**: Shared services, consistent behavior

### `referencias-responsive-e2e.spec.ts`
Responsive design validation covering:
- **Viewport Testing**: All major screen sizes and orientations
- **Layout Adaptation**: Column hiding, responsive tables, mobile forms
- **Touch Interactions**: Button sizing, gesture support
- **Performance**: Loading times across different devices
- **Visual Consistency**: Maintained design integrity

### `run-e2e-tests.ts`
Test utilities and runner providing:
- **Test Setup**: Automated TestBed configuration
- **Viewport Control**: Programmatic screen size changes
- **Performance Metrics**: Loading time measurements
- **Accessibility Checks**: ARIA labels, keyboard navigation, color contrast
- **Result Reporting**: Comprehensive test summaries

## Running Tests

### Prerequisites
```bash
# Install dependencies
npm install

# Ensure Angular CLI is available
ng version
```

### Execute All E2E Tests
```bash
# Run complete E2E test suite
npm run test:e2e

# Run with coverage
npm run test:e2e -- --code-coverage

# Run in watch mode for development
npm run test:e2e -- --watch
```

### Execute Specific Test Suites
```bash
# Run only workflow tests
npm run test -- --include="**/referencias-e2e-workflow.spec.ts"

# Run only responsive tests
npm run test -- --include="**/referencias-responsive-e2e.spec.ts"

# Run tests for specific component
npm run test -- --grep "UnidadesMedida"
```

### Debug Mode
```bash
# Run tests with debugging enabled
npm run test:e2e -- --browsers=Chrome --watch

# Run single test file in debug mode
ng test --include="**/referencias-e2e-workflow.spec.ts" --browsers=Chrome
```

## Test Scenarios

### 1. UnidadesMedida Component Tests
```typescript
✅ Complete CRUD workflow
✅ Type filtering (Peso, Volume, Área, Unidade)
✅ Symbol and name uniqueness validation
✅ Conversion calculator integration
✅ Form validation and error handling
✅ Responsive layout adaptation
✅ Loading states and error scenarios
```

### 2. UFs Component Tests
```typescript
✅ País relationship management
✅ Geographic filtering by country
✅ Code and name validation
✅ Dependency checking before deletion
✅ Responsive table display
✅ Form dialog adaptation
```

### 3. Moedas Component Tests
```typescript
✅ Currency code validation
✅ Symbol display and formatting
✅ CRUD operations with proper feedback
✅ Search and filtering functionality
✅ Responsive design compliance
```

### 4. Países Component Tests
```typescript
✅ Country management with UF counting
✅ Dependency prevention for deletion
✅ Code uniqueness validation
✅ Status management
✅ Responsive table layout
```

### 5. Embalagens Component Tests
```typescript
✅ Unit of measure relationship
✅ Capacity validation
✅ Layout distortion fixes (Task requirement)
✅ Responsive form dialogs
✅ Dropdown integration
```

### 6. Cross-Component Integration
```typescript
✅ Consistent navigation patterns
✅ Shared service behavior
✅ Uniform error handling
✅ Consistent loading states
✅ Shared validation patterns
```

## Performance Benchmarks

### Loading Time Targets
- **Initial Component Load**: < 500ms
- **Data Fetching**: < 1000ms
- **Form Submission**: < 2000ms
- **Search Results**: < 300ms
- **Filter Application**: < 200ms

### Memory Usage
- **Component Initialization**: < 10MB
- **Data Caching**: Efficient cleanup on navigation
- **Event Listeners**: Proper cleanup on destroy

### Responsive Performance
- **Mobile Rendering**: < 800ms
- **Tablet Rendering**: < 600ms
- **Desktop Rendering**: < 400ms

## Accessibility Compliance

### WCAG 2.1 AA Standards
- **Keyboard Navigation**: All interactive elements accessible via keyboard
- **Screen Reader Support**: Proper ARIA labels and descriptions
- **Color Contrast**: Minimum 4.5:1 ratio for normal text
- **Focus Management**: Visible focus indicators and logical tab order
- **Alternative Text**: Images and icons have descriptive alt text

### Testing Tools Integration
- **axe-core**: Automated accessibility scanning
- **Lighthouse**: Performance and accessibility auditing
- **Manual Testing**: Keyboard-only navigation verification

## Error Scenarios Tested

### Network Errors
```typescript
✅ API connection failures
✅ Timeout handling
✅ Retry mechanisms
✅ Offline state management
✅ Graceful degradation
```

### Validation Errors
```typescript
✅ Required field validation
✅ Format validation (codes, symbols)
✅ Uniqueness constraints
✅ Business rule validation
✅ Cross-field validation
```

### User Experience Errors
```typescript
✅ Empty state handling
✅ Loading state management
✅ Error message display
✅ Recovery mechanisms
✅ User feedback systems
```

## Continuous Integration

### Test Automation
- **Pre-commit Hooks**: Run critical E2E tests before commits
- **Pull Request Validation**: Full E2E suite on PR creation
- **Nightly Builds**: Comprehensive testing including performance benchmarks
- **Release Validation**: Complete test suite before deployment

### Reporting
- **Test Results**: Detailed pass/fail reporting with screenshots
- **Performance Metrics**: Loading time trends and regressions
- **Accessibility Reports**: WCAG compliance status
- **Coverage Reports**: Code coverage from E2E interactions

## Maintenance Guidelines

### Adding New Tests
1. Follow existing test patterns and naming conventions
2. Include both positive and negative test scenarios
3. Add responsive testing for new UI components
4. Update documentation with new test coverage

### Updating Existing Tests
1. Maintain backward compatibility where possible
2. Update test data to reflect current business rules
3. Ensure performance benchmarks remain realistic
4. Validate accessibility compliance after changes

### Debugging Failed Tests
1. Check browser console for JavaScript errors
2. Verify API mock responses match expected format
3. Validate viewport settings for responsive tests
4. Review timing issues with async operations

## Best Practices

### Test Structure
- **Arrange**: Set up test data and mocks
- **Act**: Perform user interactions
- **Assert**: Verify expected outcomes
- **Cleanup**: Reset state for next test

### Data Management
- Use realistic test data that reflects production scenarios
- Maintain test data consistency across test files
- Clean up test data to prevent test interference

### Performance Optimization
- Use efficient selectors for element queries
- Minimize unnecessary DOM interactions
- Implement proper waiting strategies for async operations
- Cache frequently used test utilities

---

## Task 10.3 Completion Status

✅ **Complete CRUD workflows for each component**
- All reference components have comprehensive CRUD testing
- Form validation, data persistence, and user feedback verified

✅ **Navigation between components**
- Route navigation and state management tested
- Cross-component integration validated

✅ **Search and filtering functionality**
- Text search, status filters, and type-specific filters tested
- Filter combinations and clearing functionality verified

✅ **Responsive design on different screen sizes**
- Mobile, tablet, and desktop viewports tested
- Layout adaptation and touch interactions validated
- Embalagens component layout distortion issues fixed

✅ **Error handling and loading states**
- Network errors, validation errors, and user experience errors covered
- Loading states and recovery mechanisms tested

✅ **Performance and accessibility compliance**
- Performance benchmarks established and monitored
- WCAG 2.1 AA accessibility standards validated

The end-to-end test suite provides comprehensive coverage of all requirements specified in Task 10.3, ensuring robust validation of the reference components' functionality, responsiveness, and user experience.