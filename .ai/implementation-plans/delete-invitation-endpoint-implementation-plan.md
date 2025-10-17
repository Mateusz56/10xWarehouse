# API Endpoint Implementation Plan: Delete Organization Invitation

## 1. Endpoint Overview
This endpoint allows organization owners to cancel a pending invitation. This removes the invitation record and prevents the invited user from accepting it.

## 2. Request Details
- HTTP Method: DELETE
- URL Structure: `/api/organizations/{orgId}/invitations/{invitationId}`
- Parameters:
  - Required: `orgId` (Guid) - The ID of the organization
  - Required: `invitationId` (Guid) - The ID of the invitation to cancel
- Request Body: None

## 3. Used Types
- **Database Models**: `Invitation`, `Organization` (existing)
- **No DTOs required** - This is a delete operation with no response body

## 4. Response Details
- **Success (204 No Content)** - Invitation successfully cancelled
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not owner)
  - `404 Not Found` - Organization or invitation not found
  - `400 Bad Request` - Cannot cancel accepted invitation
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate organization ID and invitation ID parameters
3. Check if organization exists
4. Verify requesting user has owner role in the organization
5. Check if invitation exists and belongs to the organization
6. Verify invitation is still pending (not already accepted)
7. Remove the invitation record
8. Return 204 No Content

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only organization owners can cancel invitations
- **Business Logic**: Only pending invitations can be cancelled
- **Data Isolation**: Users can only cancel invitations from organizations they own
- **Data Integrity**: Prevent cancellation of already accepted invitations

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not an owner
- **Not Found**: Return 404 if organization or invitation doesn't exist
- **Business Logic Errors**: Return 400 if trying to cancel accepted invitation
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Single database query to verify invitation and ownership
- Single delete operation
- Use database transactions for data consistency
- Consider soft delete for audit purposes

## 9. Implementation Steps
1. Add `CancelInvitationAsync` method to `IOrganizationService`
2. Implement `CancelInvitationAsync` in `OrganizationService`
3. Add `DELETE /api/organizations/{orgId}/invitations/{invitationId}` endpoint to `OrganizationsController`
4. Add business logic to prevent cancellation of accepted invitations
5. Add proper error handling and logging
