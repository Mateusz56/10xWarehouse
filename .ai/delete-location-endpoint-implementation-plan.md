# API Endpoint Implementation Plan: Delete Location

## 1. Endpoint Overview
This endpoint deletes an existing location from a warehouse. It enables users to remove storage locations that are no longer needed, supporting the warehouse details view's location management functionality.

## 2. Request Details
- HTTP Method: DELETE
- URL Structure: `/api/locations/{locationId}`
- Parameters:
  - Required: `locationId` (route parameter, Guid) - The ID of the location to delete
- Request Body: None

## 3. Used Types
- **Request DTOs**: None
- **Response DTOs**: None (204 No Content on success)
- **Database Models**: `Location`, `Warehouse`, `Organization`, `OrganizationMember`, `Inventory`
- **Exceptions**: `InvalidUserIdException`, `LocationNotFoundException`, `UnauthorizedLocationAccessException`, `LocationHasInventoryException`, `DatabaseOperationException`

## 4. Response Details
- **Success Response (204 No Content)**: Location successfully deleted
- **Error Responses**:
  - 401 Unauthorized: Invalid or missing authentication token
  - 403 Forbidden: User doesn't have access to the location's warehouse
  - 404 Not Found: Location not found
  - 409 Conflict: Cannot delete location with existing inventory
  - 500 Internal Server Error: Database or server error

## 5. Data Flow
1. Extract user ID from JWT token claims
2. Parse and validate location ID from route
3. Retrieve existing location from database
4. Verify location exists
5. Verify user has access to location's warehouse via OrganizationMember table
6. Check if location has any associated inventory records
7. If inventory exists, prevent deletion and return conflict error
8. Delete location entity from database within transaction
9. Return 204 No Content on successful deletion

## 6. Security Considerations
- **Authentication**: Requires valid JWT token with user ID claim
- **Authorization**: User must be member of location's warehouse organization (Member or Owner role)
- **Data Integrity**: Prevents deletion of locations with existing inventory
- **SQL Injection**: Uses Entity Framework parameterized queries
- **Transaction Safety**: Uses database transactions for consistency
- **Access Control**: Ensures user can only delete locations they have access to
- **Cascade Protection**: Prevents accidental data loss by checking for dependent records

## 7. Error Handling
- **Invalid User ID**: Return 401 Unauthorized
- **Location Not Found**: Return 404 Not Found
- **Unauthorized Access**: Return 403 Forbidden
- **Location Has Inventory**: Return 409 Conflict with descriptive message
- **Database Errors**: Return 500 Internal Server Error with generic message
- **Transaction Failures**: Rollback and return 500 Internal Server Error

## 8. Performance Considerations
- **Database Indexing**: Ensure indexes on Location.Id, Location.WarehouseId, and Inventory.LocationId
- **Transaction Scope**: Keep transaction scope minimal
- **Constraint Checking**: Efficiently check for inventory dependencies
- **Connection Pooling**: Use Entity Framework connection pooling
- **Logging**: Log successful deletions for audit purposes
- **Soft Delete**: Consider implementing soft delete for audit trails

## 9. Implementation Steps
1. Add `DeleteLocationAsync` method to `ILocationService` interface
2. Implement location deletion logic in `LocationService`
3. Add DELETE endpoint to `LocationsController`
4. Add location-specific exceptions:
   - `LocationNotFoundException`
   - `UnauthorizedLocationAccessException`
   - `LocationHasInventoryException`
5. Implement comprehensive error handling
6. Add inventory dependency checking logic
7. Implement unit tests for service and controller
8. Add integration tests for the endpoint
9. Test constraint scenarios (deletion with inventory)
10. Update API documentation
