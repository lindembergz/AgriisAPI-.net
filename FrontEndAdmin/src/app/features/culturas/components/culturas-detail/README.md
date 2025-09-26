# CulturasDetailComponent

## Overview

The `CulturasDetailComponent` is a standalone Angular component that provides a form interface for creating and editing agricultural cultures (culturas). It implements reactive forms with validation and integrates with the CulturaService for API communication.

## Features

- **Dual Mode Operation**: Supports both create and edit modes based on route parameters
- **Reactive Forms**: Uses Angular Reactive Forms with comprehensive validation
- **Real-time Validation**: Provides immediate feedback for form validation errors
- **Loading States**: Shows loading indicators during API operations
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Responsive Design**: Mobile-friendly layout with responsive breakpoints
- **Accessibility**: Follows accessibility best practices

## Usage

### Routes

The component is accessible through the following routes:

- `/culturas/nova` - Create new cultura
- `/culturas/:id` - Edit existing cultura (where :id is the cultura ID)

### Navigation

Users can navigate to this component from:
- CulturasListComponent "Nova Cultura" button
- CulturasListComponent "Editar" action buttons

## Form Fields

### Nome (Required)
- **Type**: Text input
- **Validation**: Required
- **Description**: Name of the agricultural culture

### Descrição (Optional)
- **Type**: Textarea
- **Validation**: None (optional field)
- **Description**: Description of the agricultural culture

### Ativo (Edit Mode Only)
- **Type**: Checkbox
- **Default**: true (for new cultures)
- **Description**: Whether the culture is active in the system

## API Integration

### Create Mode
- **Endpoint**: `POST /api/culturas`
- **Payload**: `CriarCulturaDto`
- **Success**: Shows success toast and navigates to list
- **Error**: Shows error toast with API message

### Edit Mode
- **Load**: `GET /api/culturas/:id`
- **Update**: `PUT /api/culturas/:id`
- **Payload**: `AtualizarCulturaDto`
- **Success**: Shows success toast and navigates to list
- **Error**: Shows error toast with API message

## Validation Rules

### Client-side Validation
- **Nome**: Required field
- Form submission is prevented if validation fails
- Visual indicators for invalid fields
- Error messages in Portuguese

### Server-side Validation
- API validation errors are displayed to the user
- Handles various HTTP error codes (400, 404, 409, 422, 500)

## Component State

The component uses Angular signals for reactive state management:

- `loading`: Indicates when data is being loaded
- `saving`: Indicates when form is being submitted
- `isEditMode`: Determines if component is in edit or create mode
- `culturaId`: Stores the ID of the cultura being edited

## Error Handling

### Client Errors
- Form validation errors with specific messages
- Invalid route parameters handling
- Navigation on load errors

### Server Errors
- HTTP error responses with user-friendly messages
- Toast notifications for all error scenarios
- Graceful degradation for network issues

## Styling

### CSS Classes
- `.culturas-detail-container`: Main container
- `.loading-container`: Loading state layout
- `.page-header`: Title and subtitle section
- `.cultura-form`: Form styling
- `.form-field`: Individual field styling
- `.form-actions`: Button container

### Responsive Breakpoints
- Mobile: `max-width: 768px`
- Tablet and desktop: Default styles

### Theme Support
- Light/dark theme compatibility
- High contrast mode support
- Print-friendly styles

## Testing

### Unit Tests
- Form validation testing
- Component state management
- Navigation behavior
- Error handling scenarios

### Integration Tests
- API communication testing
- HTTP error handling
- Form submission workflows
- Route parameter handling

## Dependencies

### Angular
- `@angular/common`
- `@angular/forms` (ReactiveFormsModule)
- `@angular/router`

### PrimeNG
- `primeng/button`
- `primeng/inputtext`
- `primeng/textarea`
- `primeng/checkbox`
- `primeng/card`
- `primeng/progressspinner`
- `primeng/api` (MessageService)

### Services
- `CulturaService` - API communication
- `MessageService` - Toast notifications

## Performance Considerations

- Lazy loading through route configuration
- Minimal bundle size with standalone component
- Efficient change detection with signals
- Optimized form validation

## Accessibility Features

- Semantic HTML structure
- ARIA labels and descriptions
- Keyboard navigation support
- Screen reader compatibility
- High contrast mode support
- Focus management

## Future Enhancements

- Form auto-save functionality
- Bulk operations support
- Advanced validation rules
- File upload capabilities
- Audit trail display