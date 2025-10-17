# View Implementation Plan - Organization Settings

## 1. Overview

The Organization Settings view allows users with the `Owner` role to manage organization members and invitations. This view provides functionality to view current members, invite new users, and manage pending invitations. The view is restricted to users with `Owner` role and provides comprehensive organization management capabilities.

## 2. View Routing

- **Path**: `/settings/organization`
- **Access**: Restricted to users with `Owner` role
- **Layout**: Uses main application layout with sidebar navigation

## 3. Component Structure

```
OrganizationSettingsView
├── PageHeader
├── OrganizationInfoSection
├── MembersSection
│   ├── MembersList
│   │   └── MemberCard
│   └── InviteUserButton
├── InvitationsSection
│   ├── InvitationsList
│   │   └── InvitationCard
│   └── InviteUserButton
└── InviteUserModal
    ├── UserSearchInput
    ├── RoleSelector
    └── ActionButtons
```

## 4. Component Details

### OrganizationSettingsView
- **Component description**: Main container component that manages the organization settings page state and orchestrates all sub-components
- **Main elements**: Page header, organization info section, members section, invitations section
- **Handled interactions**: Page load, organization switching, error handling
- **Handled validation**: Role-based access control (Owner only), organization existence validation
- **Types**: OrganizationSettingsState, OrganizationVM, UserVM
- **Props**: None (page-level component)

### PageHeader
- **Component description**: Displays the page title and breadcrumb navigation
- **Main elements**: H1 title, breadcrumb navigation
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: None
- **Props**: None

### OrganizationInfoSection
- **Component description**: Displays current organization information with edit capability
- **Main elements**: Organization name display, edit button, organization name input (when editing)
- **Handled interactions**: Edit organization name, save changes, cancel editing
- **Handled validation**: Organization name required (1-100 characters), name uniqueness validation
- **Types**: OrganizationVM, UpdateOrganizationRequestDto
- **Props**: organization, onUpdateOrganization, loading, error

### MembersSection
- **Component description**: Displays current organization members with management actions
- **Main elements**: Section header, members list, invite user button
- **Handled interactions**: Remove member, invite new user
- **Handled validation**: Owner role required for actions
- **Types**: OrganizationMemberDto[], PaginationDto
- **Props**: members, pagination, onRemoveMember, onInviteUser, loading, error

### MembersList
- **Component description**: Paginated list of organization members
- **Main elements**: Member cards, pagination controls
- **Handled interactions**: Pagination, member removal
- **Handled validation**: Owner role required for removal actions
- **Types**: OrganizationMemberDto[], PaginationDto
- **Props**: members, pagination, onRemoveMember, onPageChange, loading

### MemberCard
- **Component description**: Individual member display with role and action buttons
- **Main elements**: Member email, role badge, remove button
- **Handled interactions**: Remove member confirmation
- **Handled validation**: Cannot remove self, Owner role required
- **Types**: OrganizationMemberDto
- **Props**: member, onRemove, currentUserId, loading

### InvitationsSection
- **Component description**: Displays pending invitations with management actions
- **Main elements**: Section header, invitations list, invite user button
- **Handled interactions**: Cancel invitation, invite new user
- **Handled validation**: Owner role required for actions
- **Types**: InvitationDto[], PaginationDto
- **Props**: invitations, pagination, onCancelInvitation, onInviteUser, loading, error

### InvitationsList
- **Component description**: Paginated list of pending invitations
- **Main elements**: Invitation cards, pagination controls
- **Handled interactions**: Pagination, invitation cancellation
- **Handled validation**: Owner role required for cancellation
- **Types**: InvitationDto[], PaginationDto
- **Props**: invitations, pagination, onCancelInvitation, onPageChange, loading

### InvitationCard
- **Component description**: Individual invitation display with status and action buttons
- **Main elements**: Invited user email, role badge, status indicator, cancel button
- **Handled interactions**: Cancel invitation confirmation
- **Handled validation**: Owner role required, invitation must be pending
- **Types**: InvitationDto
- **Props**: invitation, onCancel, loading

### InviteUserModal
- **Component description**: Modal for inviting new users to the organization
- **Main elements**: User search input, role selector, action buttons
- **Handled interactions**: User search, role selection, form submission, modal close
- **Handled validation**: User selection required, role selection required, user not already member/invited
- **Types**: CreateInvitationRequestDto, UserSearchResult
- **Props**: isOpen, onClose, onSubmit, loading, error

### UserSearchInput
- **Component description**: Search input for finding users to invite
- **Main elements**: Search input field, search results dropdown
- **Handled interactions**: Search input, result selection
- **Handled validation**: Valid email format, user exists
- **Types**: UserSearchResult
- **Props**: onSearch, onSelect, loading, error

### RoleSelector
- **Component description**: Dropdown for selecting user role
- **Main elements**: Role dropdown with Member/Viewer options
- **Handled interactions**: Role selection
- **Handled validation**: Role selection required
- **Types**: UserRole
- **Props**: selectedRole, onRoleChange, disabled

## 5. Types

### OrganizationSettingsState
```typescript
interface OrganizationSettingsState {
  organization: OrganizationVM | null;
  members: OrganizationMemberDto[];
  invitations: InvitationDto[];
  loading: boolean;
  error: string | null;
  membersPagination: PaginationDto;
  invitationsPagination: PaginationDto;
  currentMembersPage: number;
  currentInvitationsPage: number;
  pageSize: number;
  isInviteModalOpen: boolean;
  inviteLoading: boolean;
  inviteError: string | null;
}
```

### OrganizationMemberDto
```typescript
interface OrganizationMemberDto {
  userId: string | null;
  email: string;
  role: 'Owner' | 'Member' | 'Viewer';
  status: 'Accepted' | 'Pending';
}
```

### InvitationDto
```typescript
interface InvitationDto {
  id: string;
  invitedUserId: string;
  role: 'Owner' | 'Member' | 'Viewer';
  status: 'Pending' | 'Accepted' | 'Declined';
}
```

### UserSearchResult
```typescript
interface UserSearchResult {
  id: string;
  email: string;
  displayName: string;
}
```

### CreateInvitationRequestDto
```typescript
interface CreateInvitationRequestDto {
  invitedUserId: string;
  role: 'Member' | 'Viewer';
}
```

### UpdateOrganizationRequestDto
```typescript
interface UpdateOrganizationRequestDto {
  name: string;
}
```

## 6. State Management

The view uses a custom hook `useOrganizationSettings` that manages:
- Organization data fetching and caching
- Members and invitations pagination
- Modal state management
- Loading states and error handling
- Form validation state

The hook integrates with the existing user store for role-based access control and organization context.

## 7. API Integration

### GET /api/organizations/{orgId}/members
- **Request**: Pagination parameters
- **Response**: Paginated list of OrganizationMemberDto
- **Usage**: Load organization members with pagination

### POST /api/organizations/{orgId}/invitations
- **Request**: CreateInvitationRequestDto
- **Response**: InvitationDto
- **Usage**: Create new invitation for user

### DELETE /api/organizations/{orgId}/members/{userId}
- **Request**: None
- **Response**: 200 OK
- **Usage**: Remove member from organization

### DELETE /api/organizations/{orgId}/invitations/{invitationId}
- **Request**: None
- **Response**: 200 OK
- **Usage**: Cancel pending invitation

### PUT /api/organizations/{orgId}
- **Request**: UpdateOrganizationRequestDto
- **Response**: OrganizationDto
- **Usage**: Update organization name

## 8. User Interactions

1. **View Organization Members**: Users can view paginated list of current members
2. **Invite New User**: Users can open modal to search and invite new users
3. **Remove Member**: Users can remove existing members (except themselves)
4. **Cancel Invitation**: Users can cancel pending invitations
5. **Edit Organization Name**: Users can edit the organization name
6. **Pagination**: Users can navigate through members and invitations lists
7. **Search Users**: Users can search for users to invite by email

## 9. Conditions and Validation

### Access Control
- Only users with `Owner` role can access this view
- All API calls are scoped to the current organization
- Users cannot remove themselves from the organization

### Form Validation
- Organization name: Required, 1-100 characters
- User selection: Required for invitations
- Role selection: Required for invitations
- Email format: Valid email format for user search

### Business Logic
- Cannot invite users who are already members
- Cannot invite users who already have pending invitations
- Cannot remove the last owner from the organization
- Invitation role must be Member or Viewer (not Owner)

## 10. Error Handling

### API Errors
- 401 Unauthorized: Redirect to login
- 403 Forbidden: Show access denied message
- 404 Not Found: Show organization not found message
- 409 Conflict: Show user already invited/member message
- 500 Server Error: Show generic error message

### Validation Errors
- Display inline validation messages for form fields
- Show toast notifications for successful operations
- Handle network errors with retry options

### Edge Cases
- Empty members list: Show empty state with invite button
- Empty invitations list: Show empty state with invite button
- Network failure: Show retry button
- Invalid user search: Show "user not found" message

## 11. Implementation Steps

1. **Create base page structure** (`/settings/organization`)
   - Set up routing and role-based access control
   - Create main layout with sections

2. **Implement organization info section**
   - Display current organization name
   - Add edit functionality with inline form

3. **Create members management section**
   - Implement members list with pagination
   - Add member card component with remove action
   - Implement remove member functionality

4. **Create invitations management section**
   - Implement invitations list with pagination
   - Add invitation card component with cancel action
   - Implement cancel invitation functionality

5. **Build invite user modal**
   - Create modal component with user search
   - Implement user search functionality
   - Add role selection and form validation

6. **Add state management**
   - Create useOrganizationSettings hook
   - Integrate with existing user store
   - Implement error handling and loading states

7. **Implement API integration**
   - Create API service methods
   - Add error handling and response mapping
   - Implement optimistic updates

8. **Add comprehensive error handling**
   - Implement error boundaries
   - Add user-friendly error messages
   - Handle edge cases and empty states

9. **Add accessibility features**
   - Ensure proper ARIA labels
   - Add keyboard navigation
   - Implement screen reader support

10. **Testing and refinement**
    - Add unit tests for components
    - Test role-based access control
    - Verify all user interactions work correctly
