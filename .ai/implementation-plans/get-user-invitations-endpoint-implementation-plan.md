# API Endpoint Implementation Plan: Get User Invitations

## 1. Endpoint Overview
This endpoint retrieves all pending invitations for the currently authenticated user. It shows invitations from all organizations where the user has been invited but hasn't yet accepted or declined.

## 2. Request Details
- HTTP Method: GET
- URL Structure: `/api/invitations`
- Parameters: None
- Request Body: None

## 3. Used Types
- **Response DTO**: `UserInvitationDto` (needs to be created)
- **Database Models**: `Invitation`, `Organization` (existing)

## 4. Response Details
- **Success (200 OK)**:
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "organizationName": "Example Corp",
        "role": "member",
        "invitedAt": "datetime"
      }
    ]
  }
  ```
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Query all pending invitations for the user
3. Join with organization data to get organization names
4. Format response with invitation details
5. Return list of invitations

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Data Isolation**: Users can only see their own invitations
- **Authorization**: No additional authorization needed (user-specific data)
- **Data Privacy**: Only show pending invitations, not accepted/declined ones

## 7. Error Handling
- **Authentication Errors**: Return 401 if user not authenticated
- **Database Errors**: Log and return 500 for unexpected database issues
- **Validation Errors**: Minimal validation needed (user ID from token)

## 8. Performance Considerations
- Single database query with join to organization table
- No pagination needed (typically small number of pending invitations)
- Consider caching if invitations are accessed frequently
- Use database indexes on user ID and invitation status

## 9. Implementation Steps
1. Create `UserInvitationDto` in `OrganizationDtos.cs`
2. Add `GetUserInvitationsAsync` method to `IOrganizationService`
3. Implement `GetUserInvitationsAsync` in `OrganizationService`
4. Create new `InvitationsController` for user-specific invitation endpoints
5. Add `GET /api/invitations` endpoint to `InvitationsController`
6. Add proper error handling and logging
