# API Endpoint Implementation Plan: Create Organization Invitation

## 1. Endpoint Overview
This endpoint allows organization owners to invite new users to join their organization by their user ID. The invitation creates a pending invitation record that can be accepted or declined by the invited user.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/api/organizations/{orgId}/invitations`
- Parameters:
  - Required: `orgId` (Guid) - The ID of the organization
- Request Body:
  ```json
  {
    "invitedUserId": "uuid",
    "role": "member" | "viewer | owner"
  }
  ```

## 3. Used Types
- **Request DTO**: `CreateInvitationRequestDto` (needs to be created)
- **Response DTO**: `InvitationDto` (existing)
- **Command Model**: `CreateInvitationCommand` (existing)
- **Database Models**: `Invitation`, `Organization` (existing)

## 4. Response Details
- **Success (201 Created)**:
  ```json
  {
    "id": "uuid",
    "invitedUserId": "uuid",
    "role": "member",
    "status": "pending"
  }
  ```
- **Error Responses**:
  - `400 Bad Request` - Validation errors
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized (not owner)
  - `404 Not Found` - Organization not found
  - `409 Conflict` - Invitation already exists for this user
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate request body and organization ID
3. Check if organization exists
4. Verify user has owner role in the organization
5. Check if invitation already exists for the user
6. Validate that invited user exists in Supabase
7. Create invitation record in database
8. Return created invitation DTO

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: Only organization owners can create invitations
- **Input Validation**: Invited user ID must be valid, role must be valid enum value
- **Data Isolation**: Users can only invite to organizations they own
- **Duplicate Prevention**: Check for existing invitations to prevent duplicates

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not an owner
- **Not Found**: Return 404 if organization doesn't exist
- **Conflict**: Return 409 if invitation already exists
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Single database query to check existing invitation
- Single insert operation for new invitation
- Validate user existence with Supabase client (consider caching)
- Use database transactions for data consistency

## 9. Implementation Steps
1. Create `CreateInvitationRequestDto` in `OrganizationDtos.cs`
2. Add `CreateInvitationAsync` method to `IOrganizationService`
3. Implement `CreateInvitationAsync` in `OrganizationService`
4. Add `POST /api/organizations/{orgId}/invitations` endpoint to `OrganizationsController`
5. Add Supabase client integration to validate invited user existence
6. Add proper error handling and logging