# API Endpoint Implementation Plan: Delete Organization Member

## 1. Endpoint Overview
This endpoint allows organization owners to remove a member from their organization. This is a permanent action that removes the user's access to the organization and all its resources.

## 2. Request Details
- HTTP Method: DELETE
- URL Structure: `/api/organizations/{orgId}/members/{userId}`
- Parameters:
  - Required: `orgId` (Guid) - The ID of the organization
  - Required: `userId` (Guid) - The ID of the user to remove
- Request Body: None

## 3. Used Types
- **Database Models**: `OrganizationMember`, `Organization` (existing)
- **No DTOs required** - This is a delete operation with no response body

## 4. Response Details
- **Success (204 No Content)** - Member successfully removed
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not owner)
  - `404 Not Found` - Organization or member not found
  - `400 Bad Request` - Cannot remove the last owner
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate organization ID and user ID parameters
3. Check if organization exists
4. Verify requesting user has owner role in the organization
5. Check if target user is a member of the organization
6. Verify that removing the user won't leave the organization without owners
7. Remove the organization member record
8. Return 204 No Content

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only organization owners can remove members
- **Business Logic**: Prevent removal of the last owner to maintain organization integrity
- **Data Isolation**: Users can only remove members from organizations they own
- **Self-Protection**: Prevent users from removing themselves (optional business rule)

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not an owner
- **Not Found**: Return 404 if organization or member doesn't exist
- **Business Logic Errors**: Return 400 if trying to remove the last owner
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Single database query to verify membership and ownership
- Single delete operation
- Consider cascading effects on related data (inventory, warehouses, etc.)
- Use database transactions for data consistency

## 9. Implementation Steps
1. Add `RemoveOrganizationMemberAsync` method to `IOrganizationService`
2. Implement `RemoveOrganizationMemberAsync` in `OrganizationService`
3. Add `DELETE /api/organizations/{orgId}/members/{userId}` endpoint to `OrganizationsController`
4. Add business logic to prevent removal of the last owner
5. Add proper error handling and logging
