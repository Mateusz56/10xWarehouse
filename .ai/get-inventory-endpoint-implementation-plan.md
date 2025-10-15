# API Endpoint Implementation Plan: GET /api/inventory

## 1. Endpoint Overview
This endpoint retrieves a paginated summary of inventory levels across all warehouses and locations within the active organization. It provides aggregated inventory data with optional filtering capabilities for location, product template, and low stock alerts.

## 2. Request Details
- **HTTP Method**: GET
- **URL Structure**: `/api/inventory`
- **Parameters**:
  - **Required**: None
  - **Optional**: 
    - `page` (number, default: 1) - Page number for pagination
    - `pageSize` (number, default: 50) - Number of items per page
    - `locationId` (uuid) - Filter by specific location
    - `productTemplateId` (uuid) - Filter by specific product template
    - `lowStock` (boolean) - Filter for items with quantity <= lowStockThreshold
- **Request Body**: None

## 3. Used Types
- **Request DTOs**: 
  - `PaginationRequestDto` (from existing OrganizationDtos)
- **Response DTOs**: 
  - `InventorySummaryDto` (from existing InventoryDtos)
  - `ProductSummaryDto` (from existing InventoryDtos)
  - `LocationSummaryDto` (from existing InventoryDtos)
  - `PaginatedResponseDto<InventorySummaryDto>` (from existing Common)
- **Database Models**: 
  - `Inventory` (from existing Db/Models)
  - `ProductTemplate` (from existing Db/Models)
  - `Location` (from existing Db/Models)

## 4. Response Details
- **Success Response (200 OK)**:
  ```json
  {
    "data": [
      {
        "product": { "id": "uuid", "name": "string" },
        "location": { "id": "uuid", "name": "string" },
        "quantity": 100
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 50,
      "total": 25
    }
  }
  ```
- **Error Responses**:
  - `400 Bad Request` - Invalid query parameters
  - `401 Unauthorized` - Missing or invalid JWT token
  - `500 Internal Server Error` - Database or server errors

## 5. Data Flow
1. **Authentication**: Validate JWT token and extract `active_org_id` claim
2. **Authorization**: Verify user has access to the organization (any role)
3. **Parameter Validation**: Validate pagination and filter parameters
4. **Database Query**: 
   - Query `Inventory` table joined with `ProductTemplate` and `Location`
   - Filter by `OrganizationId` from JWT claim
   - Apply optional filters (locationId, productTemplateId, lowStock)
   - Apply pagination
5. **Response Mapping**: Map database entities to DTOs
6. **Return Response**: Return paginated inventory summary

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: User must be a member of the organization (any role)
- **Data Isolation**: All queries scoped to `active_org_id` from JWT
- **Input Validation**: Validate UUID parameters and pagination bounds
- **SQL Injection**: Use Entity Framework parameterized queries

## 7. Error Handling
- **400 Bad Request**: 
  - Invalid page/pageSize values
  - Invalid UUID format for locationId/productTemplateId
- **401 Unauthorized**: 
  - Missing Authorization header
  - Invalid JWT token
  - Expired JWT token
- **500 Internal Server Error**: 
  - Database connection issues
  - Unexpected server errors

## 8. Performance Considerations
- **Database Indexing**: Ensure indexes on `OrganizationId`, `LocationId`, `ProductTemplateId`
- **Query Optimization**: Use efficient JOINs and WHERE clauses
- **Pagination**: Limit pageSize to prevent large result sets
- **Caching**: Consider caching for frequently accessed inventory data

## 9. Implementation Steps
1. **Create InventoryController**: 
   - Add `[ApiController]` and `[Route("api/[controller]")]` attributes
   - Inject `IInventoryService` dependency
2. **Create IInventoryService Interface**: 
   - Define `GetInventorySummaryAsync` method signature
3. **Implement InventoryService**: 
   - Create service class implementing `IInventoryService`
   - Implement pagination and filtering logic
   - Handle database queries with Entity Framework
4. **Add Controller Action**: 
   - Create `GetInventory` action method
   - Add parameter validation attributes
   - Implement error handling and response mapping
5. **Register Service**: 
   - Add service registration in `Program.cs`
6. **Add Authorization**: 
   - Apply authorization policies for organization access
7. **Testing**: 
   - Unit tests for service logic
   - Integration tests for controller endpoints
