# View Implementation Plan - My Invitations

## 1. Overview

The My Invitations view allows logged-in users to view and respond to their pending invitations to join other organizations. This view displays a list of invitations with organization names, assigned roles, and action buttons to accept or decline each invitation. The view is accessible to all authenticated users and provides a clean interface for managing organization invitations.

## 2. View Routing

- **Path**: `/invitations`
- **Access**: All authenticated users
- **Layout**: Uses main application layout with sidebar navigation

## 3. Component Structure

```
MyInvitationsView
├── PageHeader
├── InvitationsList
│   ├── InvitationCard
│   │   ├── OrganizationInfo
│   │   ├── RoleInfo
│   │   ├── InvitationDate
│   │   └── ActionButtons
│   └── EmptyState
└── LoadingState
```

## 4. Component Details

### MyInvitationsView
- **Component description**: Main container component that manages the invitations page state and orchestrates all sub-components
- **Main elements**: Page header, invitations list, loading state, empty state
- **Handled interactions**: Page load, invitation acceptance/decline, error handling
- **Handled validation**: User authentication, invitation ownership validation
- **Types**: MyInvitationsState, UserInvitationDto
- **Props**: None (page-level component)

### PageHeader
- **Component description**: Displays the page title and description
- **Main elements**: H1 title, page description text
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: None
- **Props**: None

### InvitationsList
- **Component description**: Container for displaying the list of invitations
- **Main elements**: Invitation cards, empty state when no invitations
- **Handled interactions**: None (delegates to individual cards)
- **Handled validation**: None
- **Types**: UserInvitationDto[]
- **Props**: invitations, loading, error, onAccept, onDecline

### InvitationCard
- **Component description**: Individual invitation display with organization details and action buttons
- **Main elements**: Organization name, role badge, invitation date, accept/decline buttons
- **Handled interactions**: Accept invitation, decline invitation
- **Handled validation**: Invitation must be pending, user must be the invited user
- **Types**: UserInvitationDto
- **Props**: invitation, onAccept, onDecline, loading, disabled

### OrganizationInfo
- **Component description**: Displays organization name and basic information
- **Main elements**: Organization name, organization icon/avatar
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: UserInvitationDto
- **Props**: invitation

### RoleInfo
- **Component description**: Displays the role assigned in the invitation
- **Main elements**: Role badge with color coding
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: UserRole
- **Props**: role

### InvitationDate
- **Component description**: Displays when the invitation was sent
- **Main elements**: Formatted date string
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: Date
- **Props**: invitedAt

### ActionButtons
- **Component description**: Accept and decline buttons for the invitation
- **Main elements**: Accept button, decline button
- **Handled interactions**: Accept invitation, decline invitation
- **Handled validation**: Invitation must be pending
- **Types**: UserInvitationDto
- **Props**: invitation, onAccept, onDecline, loading, disabled

### EmptyState
- **Component description**: Displays when user has no pending invitations
- **Main elements**: Empty state icon, message, call-to-action
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: None
- **Props**: None

### LoadingState
- **Component description**: Displays loading spinner while fetching invitations
- **Main elements**: Loading spinner, loading message
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: None
- **Props**: None

## 5. Types

### MyInvitationsState
```typescript
interface MyInvitationsState {
  invitations: UserInvitationDto[];
  loading: boolean;
  error: string | null;
  processingInvitations: Set<string>; // IDs of invitations being processed
}
```

### UserInvitationDto
```typescript
interface UserInvitationDto {
  id: string;
  organizationName: string;
  role: 'Owner' | 'Member' | 'Viewer';
  invitedAt: string; // ISO date string
}
```

### InvitationAction
```typescript
type InvitationAction = 'accept' | 'decline';
```

### InvitationCardProps
```typescript
interface InvitationCardProps {
  invitation: UserInvitationDto;
  onAccept: (invitationId: string) => void;
  onDecline: (invitationId: string) => void;
  loading: boolean;
  disabled: boolean;
}
```

### RoleDisplay
```typescript
interface RoleDisplay {
  label: string;
  color: 'blue' | 'green' | 'purple';
  description: string;
}
```

## 6. State Management

The view uses a custom hook `useMyInvitations` that manages:
- Invitations data fetching and caching
- Individual invitation processing states
- Loading states and error handling
- Optimistic updates for better UX

The hook integrates with the existing user store for authentication state and organization context updates after accepting invitations.

## 7. API Integration

### GET /api/invitations
- **Request**: None (uses authenticated user context)
- **Response**: `{ data: UserInvitationDto[] }`
- **Usage**: Load all pending invitations for the current user

### POST /api/invitations/{invitationId}/accept
- **Request**: None
- **Response**: 204 No Content
- **Usage**: Accept an invitation to join an organization

### POST /api/invitations/{invitationId}/decline
- **Request**: None
- **Response**: 204 No Content
- **Usage**: Decline an invitation to join an organization

## 8. User Interactions

1. **View Invitations**: Users can see all their pending invitations
2. **Accept Invitation**: Users can accept invitations to join organizations
3. **Decline Invitation**: Users can decline invitations
4. **Refresh Data**: Users can refresh the invitations list
5. **Navigate to Organization**: After accepting, users can switch to the new organization

## 9. Conditions and Validation

### Access Control
- Only authenticated users can access this view
- Users can only see their own invitations
- Users can only accept/decline invitations sent to them

### Business Logic
- Only pending invitations are displayed
- Accepted/declined invitations are filtered out
- After accepting an invitation, user is added to the organization
- After declining, the invitation status is updated

### Form Validation
- Invitation must exist and be pending
- User must be the invited user
- No duplicate actions on the same invitation

## 10. Error Handling

### API Errors
- 401 Unauthorized: Redirect to login
- 403 Forbidden: Show access denied message
- 404 Not Found: Show invitation not found message
- 500 Server Error: Show generic error message

### Validation Errors
- Show inline error messages for failed actions
- Display toast notifications for successful operations
- Handle network errors with retry options

### Edge Cases
- Empty invitations list: Show empty state with helpful message
- Network failure: Show retry button
- Invalid invitation: Show error message and remove from list
- Concurrent actions: Disable buttons during processing

## 11. Implementation Steps

1. **Create base page structure** (`/invitations`)
   - Set up routing and authentication guard
   - Create main layout with header

2. **Implement invitations list component**
   - Create container for invitation cards
   - Add loading and empty states
   - Implement error handling

3. **Create invitation card component**
   - Display organization information
   - Show role and invitation date
   - Add action buttons

4. **Implement organization info display**
   - Show organization name
   - Add organization icon/avatar
   - Style consistently with app theme

5. **Create role display component**
   - Show role with color coding
   - Add role descriptions
   - Ensure accessibility

6. **Add action buttons functionality**
   - Implement accept/decline actions
   - Add loading states for individual cards
   - Handle success/error feedback

7. **Add state management**
   - Create useMyInvitations hook
   - Integrate with existing user store
   - Implement optimistic updates

8. **Implement API integration**
   - Create API service methods
   - Add error handling and response mapping
   - Implement retry logic

9. **Add comprehensive error handling**
   - Implement error boundaries
   - Add user-friendly error messages
   - Handle edge cases and empty states

10. **Add accessibility features**
    - Ensure proper ARIA labels
    - Add keyboard navigation
    - Implement screen reader support

11. **Add visual feedback**
    - Implement loading spinners
    - Add success/error toast notifications
    - Show processing states for individual actions

12. **Testing and refinement**
    - Add unit tests for components
    - Test all user interactions
    - Verify error handling scenarios
    - Test with various invitation states
