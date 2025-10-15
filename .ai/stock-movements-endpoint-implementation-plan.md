# API Endpoint Implementation Plan: Stock Movements (GET and POST)

## 1. Endpoint Overview
This implementation plan covers both GET and POST endpoints for stock movements. The GET endpoint retrieves a paginated immutable log of all inventory changes, while the POST endpoint creates new stock movements and updates inventory levels in a transactional manner. Stock movements are the core business logic for all inventory operations including add, withdraw, move, and reconcile operations.

## 2. Request Details

### GET /api/stock-movements
- **HTTP Method**: GET
- **URL Structure**: `/api/stock-movements`
- **Parameters**:
  - **Required**: None
  - **Optional**: 
    - `page` (number, default: 1) - Page number for pagination
    - `pageSize` (number, default: 50) - Number of items per page
    - `productTemplateId` (uuid) - Filter by specific product template
    - `locationId` (uuid) - Filter by specific location (from or to)
- **Request Body**: None

### POST /api/stock-movements
- **HTTP Method**: POST
- **URL Structure**: `/api/stock-movements`
- **Parameters**: None
- **Request Body**: 
  ```json
  // For 'add', 'withdraw', 'reconcile'
  {
    "productTemplateId": "uuid",
    "movementType": "add" | "withdraw" | "reconcile",
    "locationId": "uuid",
    "delta": 10
  }
  
  // For 'move'
  {
    "productTemplateId": "uuid",
    "movementType": "move",
    "fromLocationId": "uuid",
    "toLocationId": "uuid",
    "delta": 5
  }
  ```

## 3. Used Types
- **Request DTOs**: 
  - `CreateStockMovementCommand` (from existing InventoryDtos)
  - `PaginationRequestDto` (from existing OrganizationDtos)
- **Response DTOs**: 
  - `StockMovementDto` (from existing InventoryDtos)
  - `PaginatedResponseDto<StockMovementDto>` (from existing Common)
- **Database Models**: 
  - `StockMovement` (from existing Db/Models)
  - `Inventory` (from existing Db/Models)
  - `ProductTemplate` (from existing Db/Models)
  - `Location` (from existing Db/Models)
- **Enums**: 
  - `MovementType` (from existing Db/Enums)

## 4. Response Details

### GET Response (200 OK)
```json
{
  "data": [
    {
      "id": "uuid",
      "productTemplateId": "uuid",
      "movementType": "add",
      "delta": 10,
      "fromLocationId": null,
      "toLocationId": "uuid",
      "createdAt": "2024-01-01T00:00:00Z"
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
  "productTemplateId": "uuid",
  "movementType": "add",
  "delta": 10,
  "fromLocationId": null,
  "toLocationId": "uuid",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Error Responses
- `400 Bad Request` - Invalid request body or parameters
- `401 Unauthorized` - Missing or invalid JWT token
- `403 Forbidden` - Insufficient permissions (POST requires Owner/Member role)
- `404 Not Found` - Product template or location not found
- `409 Conflict` - Insufficient inventory for withdraw/move operations
- `500 Internal Server Error` - Database or server errors

## 5. Data Flow

### GET Flow
1. **Authentication**: Validate JWT token and extract `active_org_id` claim
2. **Authorization**: Verify user has access to the organization (any role)
3. **Parameter Validation**: Validate pagination and filter parameters
4. **Database Query**: Query `StockMovements` table with optional filters
5. **Response Mapping**: Map database entities to DTOs
6. **Return Response**: Return paginated stock movements

### POST Flow
1. **Authentication**: Validate JWT token and extract `active_org_id` claim
2. **Authorization**: Verify user has Owner or Member role
3. **Request Validation**: Validate request body and business rules
4. **Business Logic Validation**:
   - Verify product template exists in organization
   - Verify location(s) exist in organization
   - For withdraw/move: verify sufficient inventory
   - For reconcile: validate delta represents new total quantity
5. **Database Transaction**:
   - Create new `StockMovement` record
   - Update `Inventory` table based on movement type
   - For move operations: update both source and destination inventory
6. **Response Mapping**: Map created entity to DTO
7. **Return Response**: Return created stock movement

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: 
  - GET: Any organization member
  - POST: Owner or Member role required
- **Data Isolation**: All operations scoped to `active_org_id` from JWT
- **Input Validation**: Comprehensive validation of request body
- **Business Logic Security**: Prevent negative inventory and unauthorized movements
- **SQL Injection**: Use Entity Framework parameterized queries
- **Transaction Safety**: Use database transactions for data consistency

## 7. Error Handling
- **400 Bad Request**: 
  - Invalid request body structure
  - Invalid movement type
  - Missing required fields for specific movement types
  - Invalid delta values (negative for add/withdraw)
- **401 Unauthorized**: 
  - Missing Authorization header
  - Invalid JWT token
- **403 Forbidden**: 
  - User lacks Owner/Member role for POST operations
- **404 Not Found**: 
  - Product template not found in organization
  - Location not found in organization
- **409 Conflict**: 
  - Insufficient inventory for withdraw operation
  - Insufficient inventory for move operation
- **500 Internal Server Error**: 
  - Database transaction failures
  - Unexpected server errors

## 8. Performance Considerations
- **Database Indexing**: 
  - Indexes on `OrganizationId`, `ProductTemplateId`, `LocationId`
  - Composite indexes for common query patterns
- **Transaction Management**: 
  - Use appropriate isolation levels
  - Minimize transaction scope
- **Query Optimization**: 
  - Efficient JOINs for related data
  - Proper WHERE clause ordering
- **Pagination**: Limit pageSize to prevent large result sets
- **Concurrent Access**: Handle concurrent inventory updates appropriately

## 9. Implementation Steps

### Phase 1: Service Layer
1. **Create IStockMovementService Interface**: 
   - Define `GetStockMovementsAsync` method
   - Define `CreateStockMovementAsync` method
2. **Implement StockMovementService**: 
   - Implement pagination and filtering logic for GET
   - Implement complex business logic for POST
   - Handle database transactions and inventory updates
3. **Add Business Logic Validation**: 
   - Inventory sufficiency checks
   - Movement type validation
   - Location and product template validation

### Phase 2: Controller Layer
4. **Create StockMovementsController**: 
   - Add `[ApiController]` and `[Route("api/[controller]")]` attributes
   - Inject `IStockMovementService` dependency
5. **Add GET Action**: 
   - Create `GetStockMovements` action method
   - Add parameter validation attributes
   - Implement error handling and response mapping
6. **Add POST Action**: 
   - Create `CreateStockMovement` action method
   - Add request body validation
   - Implement comprehensive error handling

### Phase 3: Integration and Testing
7. **Register Service**: 
   - Add service registration in `Program.cs`
8. **Add Authorization**: 
   - Apply role-based authorization policies
9. **Database Transaction Testing**: 
   - Test concurrent operations
   - Test rollback scenarios
10. **Business Logic Testing**: 
    - Unit tests for all movement types
    - Integration tests for inventory updates
    - Edge case testing for insufficient inventory
