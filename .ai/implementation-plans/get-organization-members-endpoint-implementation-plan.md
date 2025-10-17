# API Endpoint Implementation Plan: Get Organization Members

## 1. Endpoint Overview
This endpoint retrieves all members and pending invitations for a specific organization. It provides a paginated list showing both current members and users who have been invited but haven't yet accepted.

## 2. Request Details
- HTTP Method: GET
- URL Structure: `/api/organizations/{orgId}/members`
- Parameters:
  - Required: `orgId` (Guid) - The ID of the organization
  - Optional: `page` (int, default: 1) - Page number for pagination
  - Optional: `pageSize` (int, default: 10) - Number of items per page

## 3. Used Types
- **Response DTO**: `OrganizationMemberDto` (existing)
- **Pagination DTO**: `PaginationDto` (existing)
- **Paginated Response DTO**: `PaginatedResponseDto<OrganizationMemberDto>` (existing)
- **Database Models**: `OrganizationMember`, `Invitation` (existing)

## 4. Response Details
- **Success (200 OK)**:
  ```json
  {
    "data": [
      {
        "userId": "uuid",
        "email": "user@example.com",
        "role": "owner" | "member" | "viewer",
        "status": "accepted" | "pending"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "total": 1
    }
  }
  ```
- **Error Responses**:
  - `401 Unauthorized` - User not authenticated
  - `403 Forbidden` - User not authorized to view organization members
  - `404 Not Found` - Organization not found
  - `500 Internal Server Error` - Server error

## 5. Data Flow
1. Validate JWT token and extract user ID
2. Validate organization ID and pagination parameters
3. Check if organization exists
4. Verify user has access to the organization (any role)
5. Query organization members and invitations
6. Join with user data to get email addresses
7. Combine and paginate results
8. Return paginated response

## 6. Security Considerations
- **Authentication**: JWT token validation required
- **Authorization**: User must be a member of the organization (any role)
- **Data Isolation**: Users can only view members of organizations they belong to
- **Input Validation**: Organization ID must be valid GUID, pagination parameters must be within limits

## 7. Error Handling
- **Validation Errors**: Return 400 with specific validation messages
- **Authorization Errors**: Return 403 if user is not a member of the organization
- **Not Found**: Return 404 if organization doesn't exist
- **Database Errors**: Log and return 500 for unexpected database issues

## 8. Performance Considerations
- Use efficient joins to fetch member and invitation data
- Implement proper pagination to avoid loading large datasets
- Consider caching for frequently accessed organization member lists
- Use database indexes on organization ID and user ID columns

## 9. Implementation Steps
1. Add `GetOrganizationMembersAsync` method to `IOrganizationService`
2. Implement `GetOrganizationMembersAsync` in `OrganizationService`
3. Add `GET /api/organizations/{orgId}/members` endpoint to `OrganizationsController`
4. Create service method to fetch user email addresses from Supabase
5. Add proper error handling and logging
