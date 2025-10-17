# View Implementation Plan - Profile

## 1. Overview

The Profile view allows users to manage their personal account settings, including updating their display name and changing their password. This view provides a clean, single-column layout with separate form cards for different account management functions. The view is accessible to all authenticated users and provides real-time validation feedback for form inputs.

## 2. View Routing

- **Path**: `/profile`
- **Access**: Authenticated users only
- **Layout**: Single-column layout with form cards

## 3. Component Structure

```
ProfileView
├── ProfileHeader (title and description)
├── DisplayNameCard
│   ├── CardHeader (title and description)
│   ├── DisplayNameForm
│   │   ├── TextInput (display name)
│   │   └── Button (save changes)
│   └── CardFooter (success/error messages)
├── PasswordChangeCard
│   ├── CardHeader (title and description)
│   ├── PasswordChangeForm
│   │   ├── TextInput (current password)
│   │   ├── TextInput (new password)
│   │   ├── TextInput (confirm new password)
│   │   ├── PasswordStrengthIndicator
│   │   └── Button (change password)
│   └── CardFooter (success/error messages)
└── AccountInfoCard
    ├── CardHeader (title)
    ├── AccountInfoDisplay
    │   ├── InfoRow (email)
    │   └── InfoRow (account creation date)
    └── CardFooter (security actions)
```

## 4. Component Details

### ProfileHeader
- **Component description**: Header section with page title and description
- **Main elements**: h1 title, p description text
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: None
- **Props**: None

### DisplayNameCard
- **Component description**: Card container for display name update functionality
- **Main elements**: Card wrapper, header, form, footer
- **Handled interactions**: Form submission, input changes
- **Handled validation**: 
  - Display name required (minimum 2 characters, maximum 100 characters)
  - Real-time validation feedback
- **Types**: `DisplayNameFormData`, `DisplayNameCardProps`
- **Props**: 
  - `user: UserProfileDto`
  - `onUpdate: (data: DisplayNameFormData) => Promise<void>`
  - `loading: boolean`
  - `error: string | null`

### DisplayNameForm
- **Component description**: Form for updating user display name
- **Main elements**: TextInput, Button, validation messages
- **Handled interactions**: Input changes, form submission
- **Handled validation**: 
  - Required field validation
  - Length validation (2-100 characters)
  - Real-time validation feedback
- **Types**: `DisplayNameFormData`
- **Props**:
  - `initialValue: string`
  - `onSubmit: (data: DisplayNameFormData) => Promise<void>`
  - `loading: boolean`
  - `error: string | null`

### PasswordChangeCard
- **Component description**: Card container for password change functionality
- **Main elements**: Card wrapper, header, form, footer
- **Handled interactions**: Form submission, input changes
- **Handled validation**: 
  - Current password required
  - New password required (minimum 6 characters, maximum 100 characters)
  - Password confirmation must match new password
  - Password strength validation
- **Types**: `PasswordChangeFormData`, `PasswordChangeCardProps`
- **Props**:
  - `onChangePassword: (data: PasswordChangeFormData) => Promise<void>`
  - `loading: boolean`
  - `error: string | null`

### PasswordChangeForm
- **Component description**: Form for changing user password
- **Main elements**: TextInput (current), TextInput (new), TextInput (confirm), PasswordStrengthIndicator, Button
- **Handled interactions**: Input changes, form submission, password strength calculation
- **Handled validation**: 
  - All fields required
  - New password minimum 6 characters
  - Password confirmation must match
  - Password strength indicator
- **Types**: `PasswordChangeFormData`
- **Props**:
  - `onSubmit: (data: PasswordChangeFormData) => Promise<void>`
  - `loading: boolean`
  - `error: string | null`

### PasswordStrengthIndicator
- **Component description**: Visual indicator showing password strength
- **Main elements**: Progress bar, strength text, color indicators
- **Handled interactions**: None (reactive to password input)
- **Handled validation**: Password strength calculation based on length, complexity
- **Types**: `PasswordStrength`
- **Props**:
  - `password: string`
  - `strength: PasswordStrength`

### AccountInfoCard
- **Component description**: Card displaying read-only account information
- **Main elements**: Card wrapper, header, info rows, footer
- **Handled interactions**: None (read-only)
- **Handled validation**: None
- **Types**: `AccountInfoCardProps`
- **Props**:
  - `user: UserProfileDto`
  - `accountCreatedAt: string`

### AccountInfoDisplay
- **Component description**: Display component for account information
- **Main elements**: InfoRow components for email and creation date
- **Handled interactions**: None
- **Handled validation**: None
- **Types**: `AccountInfoDisplayProps`
- **Props**:
  - `email: string`
  - `accountCreatedAt: string`

## 5. Types

### DisplayNameFormData
```typescript
interface DisplayNameFormData {
  displayName: string;
}
```

### PasswordChangeFormData
```typescript
interface PasswordChangeFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
```

### PasswordStrength
```typescript
interface PasswordStrength {
  score: number; // 0-4
  label: 'Very Weak' | 'Weak' | 'Fair' | 'Good' | 'Strong';
  color: 'red' | 'orange' | 'yellow' | 'blue' | 'green';
}
```

### UserProfileDto (existing)
```typescript
interface UserProfileDto {
  id: string;
  email: string;
  displayName: string;
}
```

### ProfilePageState
```typescript
interface ProfilePageState {
  user: UserProfileDto | null;
  accountCreatedAt: string | null;
  displayNameLoading: boolean;
  passwordChangeLoading: boolean;
  displayNameError: string | null;
  passwordChangeError: string | null;
  displayNameSuccess: string | null;
  passwordChangeSuccess: string | null;
}
```

## 6. State Management

The Profile view will use a combination of Pinia store and local component state:

- **Global State (Pinia)**: User profile data from `authStore`
- **Local State**: Form data, loading states, error messages, success messages
- **Custom Hook**: `useProfilePage()` to manage form state and API calls

### useProfilePage Hook
```typescript
interface UseProfilePageReturn {
  // State
  displayNameForm: Ref<DisplayNameFormData>;
  passwordChangeForm: Ref<PasswordChangeFormData>;
  displayNameLoading: Ref<boolean>;
  passwordChangeLoading: Ref<boolean>;
  displayNameError: Ref<string | null>;
  passwordChangeError: Ref<string | null>;
  displayNameSuccess: Ref<string | null>;
  passwordChangeSuccess: Ref<string | null>;
  
  // Actions
  updateDisplayName: () => Promise<void>;
  changePassword: () => Promise<void>;
  resetDisplayNameForm: () => void;
  resetPasswordChangeForm: () => void;
  clearMessages: () => void;
}
```

## 7. API Integration

### Display Name Update
- **Endpoint**: `PUT /api/auth/me`
- **Request Type**: `UpdateUserProfileRequestDto`
- **Response Type**: `UserProfileDto`
- **Integration**: Direct API call with error handling and success feedback

### Password Change
- **Endpoint**: `POST /api/auth/change-password`
- **Request Type**: `ChangePasswordRequestDto`
- **Response Type**: `{ message: string }`
- **Integration**: Direct API call with error handling and success feedback

### User Profile Retrieval
- **Endpoint**: `GET /api/users/me`
- **Request Type**: None
- **Response Type**: `UserDto`
- **Integration**: Data retrieved from `authStore` (already available)

## 8. User Interactions

### Display Name Update
1. User types in display name field
2. Real-time validation provides feedback
3. User clicks "Save Changes" button
4. Form submits with loading state
5. Success message displays on success
6. Error message displays on failure
7. Form resets on success

### Password Change
1. User enters current password
2. User enters new password with strength indicator
3. User confirms new password
4. Real-time validation ensures passwords match
5. User clicks "Change Password" button
6. Form submits with loading state
7. Success message displays on success
8. Error message displays on failure
9. Form resets on success

### Account Information
1. User views read-only email and creation date
2. No interactions required

## 9. Conditions and Validation

### Display Name Validation
- **Required**: Display name must not be empty
- **Length**: 2-100 characters
- **Real-time**: Validation occurs on input change
- **API Validation**: Backend enforces same rules

### Password Change Validation
- **Current Password**: Required field
- **New Password**: Required, 6-100 characters
- **Password Confirmation**: Must match new password
- **Password Strength**: Visual indicator based on complexity
- **Real-time**: All validation occurs on input change
- **API Validation**: Backend enforces same rules

### Form State Management
- **Loading States**: Prevent multiple submissions
- **Error Handling**: Display specific error messages
- **Success Feedback**: Clear success indicators
- **Form Reset**: Clear forms after successful operations

## 10. Error Handling

### API Errors
- **400 Bad Request**: Display validation error messages
- **401 Unauthorized**: Redirect to login page
- **403 Forbidden**: Display access denied message
- **500 Internal Server Error**: Display generic error message
- **Network Errors**: Display connection error message

### Form Validation Errors
- **Required Fields**: Clear error messages for empty fields
- **Length Validation**: Specific messages for length requirements
- **Password Mismatch**: Clear indication when passwords don't match
- **Password Strength**: Visual feedback for weak passwords

### User Experience
- **Loading States**: Disable buttons during API calls
- **Success Messages**: Clear confirmation of successful operations
- **Error Persistence**: Errors remain until user takes action
- **Form Reset**: Clear forms after successful operations

## 11. Implementation Steps

1. **Create Profile Page Route**
   - Add `/profile` route to router configuration
   - Set up authentication guard

2. **Create Profile View Component**
   - Set up basic layout structure
   - Implement single-column card layout

3. **Create Display Name Card**
   - Implement card container
   - Create display name form with validation
   - Add real-time validation feedback
   - Integrate with API endpoint

4. **Create Password Change Card**
   - Implement card container
   - Create password change form with validation
   - Add password strength indicator
   - Integrate with API endpoint

5. **Create Account Info Card**
   - Implement read-only account information display
   - Add email and creation date display

6. **Implement State Management**
   - Create `useProfilePage` hook
   - Manage form state and API calls
   - Handle loading and error states

7. **Add API Integration**
   - Implement display name update API call
   - Implement password change API call
   - Add proper error handling

8. **Add Form Validation**
   - Implement real-time validation
   - Add password strength calculation
   - Create validation error messages

9. **Add Success/Error Feedback**
   - Implement toast notifications
   - Add form reset functionality
   - Create success message display

10. **Testing and Polish**
    - Test all form interactions
    - Verify API integration
    - Test error scenarios
    - Ensure accessibility compliance
