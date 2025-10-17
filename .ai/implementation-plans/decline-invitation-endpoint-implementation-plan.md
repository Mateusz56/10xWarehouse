# API Endpoint Implementation Plan: Decline Invitation

## 1. Endpoint Overview
This endpoint allows a user to decline a pending invitation to join an organization. When declined, it updates the invitation status to declined, preventing future acceptance.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/api/invitations/{invitationId}/decline`
- Parameters:
  - Required: `invitationId` (Guid) - The ID of the invitation to decline
- Request Body: None

## 3. Used Types
- **Database Models**: `Invitation` (existing)
- **No DTOs required** - This is a state change operation with no response body

## 4. Response Details
- **Success (204 No Content)** - Invitation successfully declined
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not the invited user)
  - `404 Not Found` - Invitation not found
  - `400 Bad Request` - Invitation already accepted or declined
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate invitation ID parameter
3. Check if invitation exists and is still pending
4. Verify the authenticated user is the invited user
5. Update invitation status to declined
6. Return 204 No Content

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only the invited user can decline their invitation
- **Business Logic**: Only pending invitations can be declined
- **Data Integrity**: Prevent declining already processed invitations
- **Idempotency**: Allow multiple decline attempts for the same invitation

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not the invited user
- **Not Found**: Return 404 if invitation doesn't exist
- **Business Logic Errors**: Return 400 if invitation already processed
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Single database query to check invitation status
- Single update operation for invitation status
- No complex joins or transactions required
- Consider soft delete for audit purposes

## 9. Implementation Steps
1. Add `DeclineInvitationAsync` method to `IOrganizationService`
2. Implement `DeclineInvitationAsync` in `OrganizationService`
3. Add `POST /api/invitations/{invitationId}/decline` endpoint to `InvitationsController`
4. Add business logic to prevent declining processed invitations
5. Add proper error handling and logging