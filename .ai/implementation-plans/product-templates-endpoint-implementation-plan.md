# API Endpoint Implementation Plan: Product Templates

## 1. Endpoint Overview
The product templates API provides CRUD operations for managing product master records within an organization. Product templates serve as the foundation for inventory management, defining products that can be tracked across different warehouse locations.

## 2. Request Details

### GET /api/product-templates
- **HTTP Method**: GET
- **URL Structure**: `/api/product-templates`
- **Parameters**:
  - Required: `organizationId` (Guid)
  - Optional: `page` (int, default: 1), `pageSize` (int, default: 50), `search` (string)
- **Request Body**: None

### POST /api/product-templates
- **HTTP Method**: POST
- **URL Structure**: `/api/product-templates`
- **Parameters**: None
- **Request Body**: 
  ```json
  {
    "name": "string",
    "barcode": "string",
    "description": "string",
    "lowStockThreshold": 0.0
  }
  ```
- **Authorization**: Requires 'Owner' role only

### DELETE /api/product-templates/{productTemplateId}
- **HTTP Method**: DELETE
- **URL Structure**: `/api/product-templates/{productTemplateId}`
- **Parameters**:
  - Required: `productTemplateId` (Guid)
- **Request Body**: None
- **Authorization**: Requires 'Owner' role only

## 3. Used Types

### DTOs
- `ProductTemplateDto` (needs update for decimal LowStockThreshold)
- `CreateProductTemplateRequestDto` (new, with validation attributes)
- `PaginationDto` (existing)
- `PaginatedResponseDto<T>` (existing)

### Command Models
- `CreateProductTemplateCommand` (needs update for decimal LowStockThreshold)

### Database Models
- `ProductTemplate` (existing)
- `Organization` (existing)
- `OrganizationMember` (existing)

## 4. Response Details

### GET Response (200 OK)
```json
{
  "data": [
    {
      "id": "uuid",
      "name": "string",
      "barcode": "string",
      "description": "string",
      "lowStockThreshold": 0.0
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "total": 100
  }
}
```

### POST Response (201 Created)
```json
{
  "id": "uuid",
  "name": "string",
  "barcode": "string",
  "description": "string",
  "lowStockThreshold": 0.0
}
```

### DELETE Response (204 No Content)
- **Description**: Product template successfully deleted
- **Response Body**: None

## 5. Data Flow

1. **GET Flow**:
   - Extract user ID from JWT claims
   - Validate pagination parameters
   - Verify user has access to organization
   - Query product templates with optional search filter
   - Apply pagination
   - Return paginated results

2. **POST Flow**:
   - Extract user ID from JWT claims
   - Validate request body
   - Verify user has Owner role in organization
   - Check for duplicate barcode within organization
   - Create new product template
   - Return created entity

3. **DELETE Flow**:
   - Extract user ID from JWT claims
   - Verify user has Owner role in organization
   - Check if product template exists
   - Verify product template belongs to user's organization
   - Check if product template has associated inventory
   - Delete product template (cascade delete inventory)
   - Return 204 No Content

## 6. Security Considerations

- **Authentication**: JWT token required for all endpoints
- **Authorization**: 
  - GET: User must be member of the organization
  - POST/DELETE: User must be Owner of the organization
- **Input Validation**: All input fields validated with appropriate constraints
- **Data Isolation**: Product templates scoped to organization
- **Barcode Uniqueness**: Enforced at database level with unique constraint
- **Role-based Access**: POST and DELETE operations restricted to organization owners only

## 7. Error Handling

- **400 Bad Request**: Invalid input parameters or validation failures
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: User not member of organization (GET) or not Owner (POST/DELETE)
- **404 Not Found**: Product template not found (DELETE)
- **409 Conflict**: Duplicate barcode within organization (POST)
- **422 Unprocessable Entity**: Cannot delete product template with existing inventory
- **500 Internal Server Error**: Database or server errors

## 8. Performance Considerations

- **Pagination**: Implemented to handle large datasets efficiently
- **Search**: Case-insensitive search on name and description fields
- **Indexing**: Database has unique index on (organization_id, barcode)
- **Caching**: Consider caching frequently accessed product templates

## 9. Implementation Steps

1. **Update DTOs**:
   - Fix `ProductTemplateDto` to use `decimal` for `LowStockThreshold`
   - Create `CreateProductTemplateRequestDto` with validation attributes
   - Update `CreateProductTemplateCommand` to use `decimal`

2. **Create Exceptions**:
   - `ProductTemplateNotFoundException`
   - `DuplicateProductTemplateBarcodeException`
   - `UnauthorizedProductTemplateAccessException`
   - `ProductTemplateHasInventoryException`

3. **Create Service Interface**:
   - `IProductTemplateService` with methods for CRUD operations
   - Include organization access validation methods
   - Include role-based authorization methods

4. **Implement Service**:
   - `ProductTemplateService` implementing the interface
   - Follow existing patterns from `WarehouseService` and `LocationService`
   - Implement search functionality with case-insensitive filtering
   - Implement role-based authorization for delete operations
   - Check for inventory dependencies before deletion

5. **Create Controller**:
   - `ProductTemplatesController` with GET, POST, and DELETE endpoints
   - Follow existing authentication and authorization patterns
   - Implement proper error handling and logging
   - Add role-based authorization for POST and DELETE endpoints

6. **Register Dependencies**:
   - Register `IProductTemplateService` and `ProductTemplateService` in DI container

7. **Add Validation**:
   - Request body validation using data annotations
   - Custom validation for barcode uniqueness
   - Pagination parameter validation

8. **Testing**:
   - Unit tests for service layer
   - Integration tests for controller endpoints
   - Test error scenarios and edge cases

The implementation should follow the established patterns in the codebase, ensuring consistency with existing warehouse and location management functionality.
