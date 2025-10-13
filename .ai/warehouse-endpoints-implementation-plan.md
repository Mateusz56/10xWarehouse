# Warehouse Endpoints Implementation Plan

## Overview
This document outlines the implementation plan for warehouse management endpoints in the 10xWarehouse application. The implementation follows the established patterns from the OrganizationsController and OrganizationService.

## Endpoint Specifications

### 1. Get Warehouses by Organization
**HTTP Method:** `GET`  
**Route:** `/api/warehouses`  
**Query Parameters:**
- `organizationId` (required): Guid of the organization
- `page` (optional): Page number for pagination (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 100)

**Response:** `PaginatedResponseDto<WarehouseDto>`

**Business Logic:**
1. Validate user authentication and extract user ID from claims
2. Verify user has access to the specified organization
3. Validate pagination parameters
4. Query warehouses belonging to the organization
5. Apply pagination
6. Return paginated list of warehouses

**Error Handling:**
- 400: Invalid pagination parameters
- 401: User not authenticated or invalid user ID
- 403: User doesn't have access to organization
- 404: Organization not found
- 500: Database or server errors

### 2. Get Single Warehouse
**HTTP Method:** `GET`  
**Route:** `/api/warehouses/{warehouseId}`  
**Path Parameters:**
- `warehouseId` (required): Guid of the warehouse

**Response:** `WarehouseWithLocationsDto`

**Business Logic:**
1. Validate user authentication and extract user ID from claims
2. Verify user has access to the warehouse's organization
3. Query warehouse with its locations
4. Return warehouse details with locations

**Error Handling:**
- 401: User not authenticated or invalid user ID
- 403: User doesn't have access to warehouse
- 404: Warehouse not found
- 500: Database or server errors

### 3. Create Warehouse
**HTTP Method:** `POST`  
**Route:** `/api/warehouses`  
**Request Body:** `CreateWarehouseRequestDto`

**Response:** `WarehouseDto`

**Business Logic:**
1. Validate user authentication and extract user ID from claims
2. Verify user has access to the specified organization
3. Validate warehouse name (required, max 100 characters)
4. Check for duplicate warehouse names within organization
5. Create new warehouse entity
6. Save to database
7. Return created warehouse

**Error Handling:**
- 400: Invalid request data or duplicate warehouse name
- 401: User not authenticated or invalid user ID
- 403: User doesn't have access to organization
- 404: Organization not found
- 500: Database or server errors

### 4. Update Warehouse
**HTTP Method:** `PUT`  
**Route:** `/api/warehouses/{warehouseId}`  
**Path Parameters:**
- `warehouseId` (required): Guid of the warehouse
**Request Body:** `UpdateWarehouseRequestDto`

**Response:** `WarehouseDto`

**Business Logic:**
1. Validate user authentication and extract user ID from claims
2. Verify user has access to the warehouse's organization
3. Validate warehouse name (required, max 100 characters)
4. Check for duplicate warehouse names within organization (excluding current warehouse)
5. Update warehouse entity
6. Save changes to database
7. Return updated warehouse

**Error Handling:**
- 400: Invalid request data or duplicate warehouse name
- 401: User not authenticated or invalid user ID
- 403: User doesn't have access to warehouse
- 404: Warehouse not found
- 500: Database or server errors

### 5. Delete Warehouse
**HTTP Method:** `DELETE`  
**Route:** `/api/warehouses/{warehouseId}`  
**Path Parameters:**
- `warehouseId` (required): Guid of the warehouse

**Response:** `204 No Content`

**Business Logic:**
1. Validate user authentication and extract user ID from claims
2. Verify user has access to the warehouse's organization
3. Check if warehouse has any locations
4. If locations exist, prevent deletion and return error
5. Delete warehouse from database
6. Return success response

**Error Handling:**
- 401: User not authenticated or invalid user ID
- 403: User doesn't have access to warehouse
- 404: Warehouse not found
- 409: Warehouse has locations and cannot be deleted
- 500: Database or server errors

## Data Transfer Objects (DTOs)

### Request DTOs
```csharp
public record CreateWarehouseRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
}

public record UpdateWarehouseRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}
```

### Response DTOs
```csharp
public record WarehouseDto(Guid Id, string Name, Guid OrganizationId);

public record WarehouseWithLocationsDto(Guid Id, string Name, Guid OrganizationId, List<LocationDto> Locations);

public record LocationDto(Guid Id, string Name, string? Description);
```

## Service Layer Design

### IWarehouseService Interface
```csharp
public interface IWarehouseService
{
    Task<(IEnumerable<Warehouse> warehouses, int totalCount)> GetOrganizationWarehousesAsync(
        Guid organizationId, string userId, int page, int pageSize);
    
    Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId, string userId);
    
    Task<Warehouse> CreateWarehouseAsync(CreateWarehouseRequestDto request, string userId);
    
    Task<Warehouse> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, string userId);
    
    Task DeleteWarehouseAsync(Guid warehouseId, string userId);
    
    Task<bool> UserHasAccessToOrganizationAsync(Guid organizationId, string userId);
    
    Task<bool> UserHasAccessToWarehouseAsync(Guid warehouseId, string userId);
}
```

### WarehouseService Implementation
The service will handle:
- Database operations using Entity Framework
- User access validation through organization membership
- Business rule validation (duplicate names, deletion constraints)
- Proper error handling and logging
- Transaction management for data consistency

## Exception Handling

### New Warehouse-Specific Exceptions
```csharp
public class WarehouseNotFoundException : Exception
{
    public WarehouseNotFoundException(Guid warehouseId) 
        : base($"Warehouse with ID {warehouseId} was not found.") { }
}

public class DuplicateWarehouseNameException : Exception
{
    public DuplicateWarehouseNameException(string warehouseName, Guid organizationId) 
        : base($"Warehouse with name '{warehouseName}' already exists in organization {organizationId}.") { }
}

public class WarehouseHasLocationsException : Exception
{
    public WarehouseHasLocationsException(Guid warehouseId) 
        : base($"Cannot delete warehouse {warehouseId} because it contains locations.") { }
}

public class UnauthorizedWarehouseAccessException : Exception
{
    public UnauthorizedWarehouseAccessException(Guid warehouseId, string userId) 
        : base($"User {userId} does not have access to warehouse {warehouseId}.") { }
}
```

## Database Considerations

### Required Database Operations
1. **Query warehouses by organization** with pagination
2. **Query single warehouse** with locations included
3. **Insert new warehouse** with transaction support
4. **Update warehouse** with duplicate name checking
5. **Delete warehouse** with constraint checking
6. **Check user organization membership** for access control

### Entity Framework Queries
- Use `Include()` for loading related locations
- Implement proper pagination with `Skip()` and `Take()`
- Use transactions for data consistency
- Apply proper indexing for performance

## Security Considerations

### Access Control
- All endpoints require authentication (`[Authorize]` attribute)
- User access validation through organization membership
- Role-based access control (future enhancement)

### Input Validation
- Required field validation using data annotations
- String length limits for warehouse names
- GUID format validation for IDs
- Pagination parameter validation

## Implementation Steps

### Phase 1: Core Infrastructure
1. Create `WarehouseController` with basic structure
2. Define `IWarehouseService` interface
3. Implement `WarehouseService` with database operations
4. Create warehouse-specific exceptions

### Phase 2: CRUD Operations
1. Implement GET warehouses endpoint with pagination
2. Implement GET single warehouse endpoint
3. Implement POST create warehouse endpoint
4. Implement PUT update warehouse endpoint
5. Implement DELETE warehouse endpoint

### Phase 3: Testing and Validation
1. Add comprehensive error handling
2. Implement input validation
3. Add logging for all operations
4. Test edge cases and error scenarios

## Testing Considerations

### Unit Tests
- Service layer business logic
- Exception handling scenarios
- Access control validation
- Database operation mocking

### Integration Tests
- End-to-end API testing
- Database integration testing
- Authentication and authorization testing
- Pagination functionality testing

### Edge Cases to Test
- Invalid pagination parameters
- Non-existent warehouse IDs
- Duplicate warehouse names
- Warehouse deletion with existing locations
- Unauthorized access attempts
- Database connection failures

## Future Enhancements

### Potential Additions
1. **Bulk operations** for multiple warehouses
2. **Warehouse statistics** (location count, inventory summary)
3. **Warehouse templates** for quick creation
4. **Audit logging** for warehouse changes
5. **Soft delete** instead of hard delete
6. **Warehouse archiving** functionality

### Performance Optimizations
1. **Caching** for frequently accessed warehouses
2. **Database indexing** optimization
3. **Query optimization** for large datasets
4. **Response compression** for large payloads

## Dependencies

### Required NuGet Packages
- `Microsoft.EntityFrameworkCore` (already included)
- `Microsoft.AspNetCore.Mvc` (already included)
- `Microsoft.AspNetCore.Authorization` (already included)

### Existing Dependencies
- `WarehouseDbContext` for database operations
- `OrganizationService` for access control validation
- Existing exception handling patterns
- Authentication middleware

## File Structure

### New Files to Create
```
10xWarehouseNet/
├── Controllers/
│   └── WarehousesController.cs
├── Services/
│   ├── IWarehouseService.cs
│   └── WarehouseService.cs
├── Exceptions/
│   └── WarehouseExceptions.cs
└── Dtos/
    └── WarehouseDtos.cs (extend existing)
```

### Files to Modify
```
10xWarehouseNet/
├── Program.cs (register new service)
└── Dtos/WarehouseDtos.cs (add new DTOs)
```

This implementation plan provides a comprehensive foundation for warehouse management endpoints that follows the established patterns in the codebase while ensuring proper security, validation, and error handling.
