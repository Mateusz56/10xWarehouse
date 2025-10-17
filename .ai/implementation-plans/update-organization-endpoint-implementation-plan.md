# API Endpoint Implementation Plan: Update Organization

## 1. Endpoint Overview
This endpoint allows organization owners to update the name of their organization. The endpoint requires owner-level permissions and validates that the requesting user has the necessary authorization to modify the organization.

## 2. Request Details
- HTTP Method: PUT
- URL Structure: `/api/organizations/{orgId}`
- Parameters:
  - Required: `orgId` (Guid) - The ID of the organization to update
- Request Body:
  ```json
  {
    "name": "string"
  }
  ```

## 3. Used Types
- **Request DTO**: `UpdateOrganizationRequestDto` (needs to be created)
- **Response DTO**: `OrganizationDto` (existing)
- **Command Model**: `UpdateOrganizationCommand` (needs to be created)
- **Database Model**: `Organization` (existing)

## 4. Response Details
- **Success (200 OK)**:
  ```json
  {
    "id": "uuid",
    "name": "string"
  }
  ```
- **Error Responses**:
  - `400 Bad Request` - Validation errors
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not owner)
  - `404 Not Found` - Organization not found
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate request body and organization ID
3. Check if organization exists
4. Verify user has owner role in the organization
5. Update organization name in database
6. Return updated organization DTO

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only organization owners can update organization details
- **Input Validation**: Organization name must be non-empty and within length limits
- **Data Isolation**: Users can only update organizations they own

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not an owner
- **Not Found**: Return 404 if organization doesn't exist
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Single database query to fetch organization and verify ownership
- Single update operation
- No complex joins or aggregations required

## 9. Implementation Steps
1. Create `UpdateOrganizationRequestDto` in `OrganizationDtos.cs`
2. Create `UpdateOrganizationCommand` in `OrganizationDtos.cs`
3. Add `UpdateOrganizationAsync` method to `IOrganizationService`
4. Implement `UpdateOrganizationAsync` in `OrganizationService`
5. Add `PUT /api/organizations/{orgId}` endpoint to `OrganizationsController`
6. Add proper error handling and logging
