# API Endpoint Implementation Plan: Accept Invitation

## 1. Endpoint Overview
This endpoint allows a user to accept a pending invitation to join an organization. When accepted, it creates an organization member record and updates the invitation status to accepted.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/api/invitations/{invitationId}/accept`
- Parameters:
  - Required: `invitationId` (Guid) - The ID of the invitation to accept
- Request Body: None

## 3. Used Types
- **Database Models**: `Invitation`, `OrganizationMember`, `Organization` (existing)
- **No DTOs required** - This is a state change operation with no response body

## 4. Response Details
- **Success (204 No Content)** - Invitation successfully accepted
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not the invited user)
  - `404 Not Found` - Invitation not found
  - `400 Bad Request` - Invitation already accepted or expired
  - `409 Conflict` - User already a member of the organization
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate invitation ID parameter
3. Check if invitation exists and is still pending
4. Verify the authenticated user is the invited user
5. Check if user is already a member of the organization
6. Create organization member record
7. Update invitation status to accepted
8. Use database transaction to ensure consistency
9. Return 204 No Content

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only the invited user can accept their invitation
- **Business Logic**: Only pending invitations can be accepted
- **Data Integrity**: Prevent duplicate memberships
- **Transaction Safety**: Use database transactions for atomicity

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not the invited user
- **Not Found**: Return 404 if invitation doesn't exist
- **Business Logic Errors**: Return 400 if invitation already accepted
- **Conflict Errors**: Return 409 if user already a member
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Use database transaction for atomicity
- Single query to check invitation and user status
- Single insert for organization member
- Single update for invitation status
- Consider adding invitation expiration logic

## 9. Implementation Steps
1. Add `AcceptInvitationAsync` method to `IOrganizationService`
2. Implement `AcceptInvitationAsync` in `OrganizationService`
3. Add `POST /api/invitations/{invitationId}/accept` endpoint to `InvitationsController`
4. Add business logic to prevent duplicate memberships
5. Add proper error handling and logging