# API Endpoint Implementation Plan: Get Locations

## 1. Endpoint Overview
This endpoint retrieves a paginated list of locations belonging to a specific warehouse. It provides location management functionality for the warehouse details view, allowing users to view all locations within a warehouse with proper pagination support.

## 2. Request Details
- HTTP Method: GET
- URL Structure: `/api/locations`
- Parameters:
  - Required: 
    - `warehouseId` (query parameter, Guid) - The ID of the warehouse to get locations for
    - `page` (query parameter, int) - Page number (default: 1)
    - `pageSize` (query parameter, int) - Number of items per page (default: 10, max: 100)
- Request Body: None

## 3. Used Types
- **Request DTOs**: `PaginationRequestDto` (existing)
- **Response DTOs**: `PaginatedResponseDto<LocationDto>`, `LocationDto`, `PaginationDto`
- **Database Models**: `Location`, `Warehouse`, `Organization`, `OrganizationMember`
- **Exceptions**: `InvalidUserIdException`, `InvalidPaginationException`, `UnauthorizedWarehouseAccessException`, `WarehouseNotFoundException`, `DatabaseOperationException`

## 4. Response Details
- **Success Response (200 OK)**: Returns paginated list of locations
  ```json
  {
    "data": [
      {
        "id": "guid",
        "name": "string",
        "description": "string",
        "warehouseId": "guid"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "total": 25
    }
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid pagination parameters or warehouse ID
  - 401 Unauthorized: Invalid or missing authentication token
  - 403 Forbidden: User doesn't have access to the warehouse's organization
  - 404 Not Found: Warehouse not found
  - 500 Internal Server Error: Database or server error

## 5. Data Flow
1. Extract user ID from JWT token claims
2. Validate pagination parameters (page ≥ 1, pageSize 1-100)
3. Parse and validate warehouse ID
4. Verify user has access to warehouse's organization via OrganizationMember table
5. Verify warehouse exists
6. Query locations table with pagination:
   - Filter by warehouseId
   - Order by name
   - Apply pagination (Skip/Take)
7. Count total locations for pagination metadata
8. Map database models to DTOs
9. Return paginated response

## 6. Security Considerations
- **Authentication**: Requires valid JWT token with user ID claim
- **Authorization**: User must be member of warehouse's organization
- **Input Validation**: 
  - Warehouse ID must be valid GUID
  - Page number must be ≥ 1
  - Page size must be between 1-100
- **Data Access**: Only returns locations for warehouses the user has access to
- **SQL Injection**: Uses Entity Framework parameterized queries

## 7. Error Handling
- **Invalid User ID**: Return 401 Unauthorized
- **Invalid Pagination**: Return 400 Bad Request with validation details
- **Warehouse Not Found**: Return 404 Not Found
- **Unauthorized Access**: Return 403 Forbidden
- **Database Errors**: Return 500 Internal Server Error with generic message
- **Unexpected Errors**: Log error and return 500 Internal Server Error

## 8. Performance Considerations
- **Database Indexing**: Ensure indexes on Location.WarehouseId and OrganizationMember tables
- **Pagination**: Limit page size to prevent large data transfers
- **Query Optimization**: Use efficient LINQ queries with proper includes
- **Caching**: Consider caching warehouse access permissions for frequently accessed warehouses
- **Connection Pooling**: Use Entity Framework connection pooling

## 9. Implementation Steps
1. Create `ILocationService` interface with `GetWarehouseLocationsAsync` method
2. Implement `LocationService` class with location retrieval logic
3. Add location DTOs to `WarehouseDtos.cs`:
   - `CreateLocationRequestDto`
   - `UpdateLocationRequestDto`
   - Extend existing `LocationDto` with `WarehouseId`
4. Create `LocationsController` with GET endpoint
5. Add location-specific exceptions to `LocationExceptions.cs`:
   - `LocationNotFoundException`
   - `DuplicateLocationNameException`
   - `UnauthorizedLocationAccessException`
6. Register `ILocationService` in dependency injection container
7. Add comprehensive error handling and logging
8. Implement unit tests for service and controller
9. Add integration tests for the endpoint
10. Update API documentation
