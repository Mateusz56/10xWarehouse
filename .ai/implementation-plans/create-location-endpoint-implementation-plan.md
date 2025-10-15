# API Endpoint Implementation Plan: Create Location

## 1. Endpoint Overview
This endpoint creates a new location within a specific warehouse. It enables users to add new storage locations to existing warehouses, supporting the warehouse details view's location management functionality.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/api/locations`
- Parameters: None
- Request Body: JSON object with location details
  ```json
  {
    "name": "string (required, max 100 chars)",
    "description": "string (optional, max 500 chars)",
    "warehouseId": "guid (required)"
  }
  ```

## 3. Used Types
- **Request DTOs**: `CreateLocationRequestDto`
- **Response DTOs**: `LocationDto`
- **Database Models**: `Location`, `Warehouse`, `Organization`, `OrganizationMember`
- **Exceptions**: `InvalidUserIdException`, `UnauthorizedWarehouseAccessException`, `WarehouseNotFoundException`, `DuplicateLocationNameException`, `DatabaseOperationException`

## 4. Response Details
- **Success Response (201 Created)**: Returns created location
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "warehouseId": "guid"
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid request data or validation errors
  - 401 Unauthorized: Invalid or missing authentication token
  - 403 Forbidden: User doesn't have access to the warehouse's organization
  - 404 Not Found: Warehouse not found
  - 409 Conflict: Location name already exists in warehouse
  - 500 Internal Server Error: Database or server error

## 5. Data Flow
1. Extract user ID from JWT token claims
2. Validate request model and data annotations
3. Parse and validate warehouse ID
4. Verify warehouse exists
5. Verify user has access to warehouse's organization via OrganizationMember table
6. Check for duplicate location name within the same warehouse
7. Create new Location entity with provided data
8. Save to database within transaction
9. Map created entity to DTO
10. Return 201 Created with location data

## 6. Security Considerations
- **Authentication**: Requires valid JWT token with user ID claim
- **Authorization**: User must be member of warehouse's organization (Member or Owner role)
- **Input Validation**: 
  - Name: Required, max 100 characters
  - Description: Optional, max 500 characters
  - WarehouseId: Required, valid GUID
- **Data Integrity**: Prevents duplicate location names within same warehouse
- **SQL Injection**: Uses Entity Framework parameterized queries
- **Transaction Safety**: Uses database transactions for consistency

## 7. Error Handling
- **Invalid User ID**: Return 401 Unauthorized
- **Invalid Request Data**: Return 400 Bad Request with validation details
- **Warehouse Not Found**: Return 404 Not Found
- **Unauthorized Access**: Return 403 Forbidden
- **Duplicate Name**: Return 409 Conflict with descriptive message
- **Database Errors**: Return 500 Internal Server Error with generic message
- **Transaction Failures**: Rollback and return 500 Internal Server Error

## 8. Performance Considerations
- **Database Indexing**: Ensure indexes on Location.WarehouseId and Location.Name for duplicate checking
- **Transaction Scope**: Keep transaction scope minimal
- **Validation**: Perform validation before database operations
- **Connection Pooling**: Use Entity Framework connection pooling
- **Logging**: Log successful creations for audit purposes

## 9. Implementation Steps
1. Add `CreateLocationAsync` method to `ILocationService` interface
2. Implement location creation logic in `LocationService`
3. Add `CreateLocationRequestDto` to `WarehouseDtos.cs`:
   ```csharp
   public record CreateLocationRequestDto
   {
       [Required]
       [StringLength(100)]
       public string Name { get; set; } = string.Empty;
       
       [StringLength(500)]
       public string? Description { get; set; }
       
       [Required]
       public Guid WarehouseId { get; set; }
   }
   ```
4. Add POST endpoint to `LocationsController`
5. Add location-specific exceptions:
   - `DuplicateLocationNameException`
   - `LocationNotFoundException`
6. Implement comprehensive error handling
7. Add input validation attributes
8. Implement unit tests for service and controller
9. Add integration tests for the endpoint
10. Update API documentation
