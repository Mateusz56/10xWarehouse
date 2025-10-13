# API Endpoint Implementation Plan: Update Location

## 1. Endpoint Overview
This endpoint updates an existing location within a warehouse. It allows users to modify location details such as name and description, supporting the warehouse details view's location management functionality.

## 2. Request Details
- HTTP Method: PUT
- URL Structure: `/api/locations/{locationId}`
- Parameters:
  - Required: `locationId` (route parameter, Guid) - The ID of the location to update
- Request Body: JSON object with updated location details
  ```json
  {
    "name": "string (required, max 100 chars)",
    "description": "string (optional, max 500 chars)"
  }
  ```

## 3. Used Types
- **Request DTOs**: `UpdateLocationRequestDto`
- **Response DTOs**: `LocationDto`
- **Database Models**: `Location`, `Warehouse`, `Organization`, `OrganizationMember`
- **Exceptions**: `InvalidUserIdException`, `LocationNotFoundException`, `UnauthorizedLocationAccessException`, `DuplicateLocationNameException`, `DatabaseOperationException`

## 4. Response Details
- **Success Response (200 OK)**: Returns updated location
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
  - 403 Forbidden: User doesn't have access to the location's warehouse
  - 404 Not Found: Location not found
  - 409 Conflict: Location name already exists in warehouse
  - 500 Internal Server Error: Database or server error

## 5. Data Flow
1. Extract user ID from JWT token claims
2. Validate request model and data annotations
3. Parse and validate location ID from route
4. Retrieve existing location from database
5. Verify location exists
6. Verify user has access to location's warehouse via OrganizationMember table
7. Check for duplicate location name within the same warehouse (excluding current location)
8. Update location entity with provided data
9. Save changes to database within transaction
10. Map updated entity to DTO
11. Return 200 OK with updated location data

## 6. Security Considerations
- **Authentication**: Requires valid JWT token with user ID claim
- **Authorization**: User must be member of location's warehouse organization (Member or Owner role)
- **Input Validation**: 
  - Name: Required, max 100 characters
  - Description: Optional, max 500 characters
- **Data Integrity**: Prevents duplicate location names within same warehouse
- **SQL Injection**: Uses Entity Framework parameterized queries
- **Transaction Safety**: Uses database transactions for consistency
- **Access Control**: Ensures user can only update locations they have access to

## 7. Error Handling
- **Invalid User ID**: Return 401 Unauthorized
- **Invalid Request Data**: Return 400 Bad Request with validation details
- **Location Not Found**: Return 404 Not Found
- **Unauthorized Access**: Return 403 Forbidden
- **Duplicate Name**: Return 409 Conflict with descriptive message
- **Database Errors**: Return 500 Internal Server Error with generic message
- **Transaction Failures**: Rollback and return 500 Internal Server Error

## 8. Performance Considerations
- **Database Indexing**: Ensure indexes on Location.Id, Location.WarehouseId, and Location.Name
- **Transaction Scope**: Keep transaction scope minimal
- **Validation**: Perform validation before database operations
- **Connection Pooling**: Use Entity Framework connection pooling
- **Logging**: Log successful updates for audit purposes
- **Optimistic Concurrency**: Consider adding concurrency tokens for concurrent updates

## 9. Implementation Steps
1. Add `UpdateLocationAsync` method to `ILocationService` interface
2. Implement location update logic in `LocationService`
3. Add `UpdateLocationRequestDto` to `WarehouseDtos.cs`:
   ```csharp
   public record UpdateLocationRequestDto
   {
       [Required]
       [StringLength(100)]
       public string Name { get; set; } = string.Empty;
       
       [StringLength(500)]
       public string? Description { get; set; }
   }
   ```
4. Add PUT endpoint to `LocationsController`
5. Add location-specific exceptions:
   - `LocationNotFoundException`
   - `DuplicateLocationNameException`
   - `UnauthorizedLocationAccessException`
6. Implement comprehensive error handling
7. Add input validation attributes
8. Implement unit tests for service and controller
9. Add integration tests for the endpoint
10. Update API documentation
