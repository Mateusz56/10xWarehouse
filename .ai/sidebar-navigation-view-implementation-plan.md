# View Implementation Plan: Sidebar Navigation

## 1. Overview
The Sidebar Navigation is a core, persistent UI component, not a routable view. Its purpose is to provide primary application navigation, organization management (switching and creation), and access to user-specific actions like profile and logout. It will be displayed on all authenticated pages. The sidebar's content, particularly the visibility of certain navigation links, will dynamically adapt based on the user's role within the currently selected organization.

## 2. View Routing
This is a layout component and does not have a dedicated route. It will be integrated into the main application layout file, likely `src/layouts/MainLayout.astro`, to ensure it's present across all authenticated views.

## 3. Component Structure
The sidebar will be composed of several distinct Vue components to ensure separation of concerns and reusability.

```
- SidebarNav.vue (Main container for the navigation bar)
  - OrganizationSwitcher.vue (Dropdown for selecting an organization)
  - CreateOrganizationButton.vue (Button that triggers the creation modal)
  - NavItem.vue[] (A list of individual navigation links)
  - LogoutButton.vue (Handles user logout)
- CreateOrganizationModal.vue (A modal dialog for creating a new organization)
  - CreateOrganizationForm.vue (The form inside the modal)
```

## 4. Component Details

### `SidebarNav.vue`
-   **Component description**: The main container component that assembles all other navigation elements. It fetches the necessary data from the state manager (Pinia) and passes it down to child components. It also controls the overall layout of the sidebar.
-   **Main elements**: A `<nav>` element that vertically arranges its child components: `OrganizationSwitcher`, `CreateOrganizationButton`, a list of `NavItem` components, and a `LogoutButton`.
-   **Handled interactions**: None directly. It orchestrates child components.
-   **Handled validation**: None.
-   **Types**: `OrganizationVM`, `UserVM`, `NavItemVM`.
-   **Props**: None.

### `OrganizationSwitcher.vue`
-   **Component description**: A dropdown component that displays the user's list of organizations and allows them to switch the active organization context for the entire application.
-   **Main elements**: A dropdown/select component from the UI library (`shadcn/ui`). It will display the name of the currently active organization and, when clicked, will show a list of all available organizations.
-   **Handled interactions**: `on-select`: when a user selects a new organization from the list. This will trigger a state management action to update the application's current organization context.
-   **Handled validation**: The component should handle the case where a user belongs to zero or one organizations (e.g., by disabling the dropdown).
-   **Types**: `OrganizationVM[]`, `string` (for `currentOrganizationId`).
-   **Props**:
    -   `organizations: OrganizationVM[]`
    -   `currentOrganizationId: string`

### `CreateOrganizationButton.vue`
-   **Component description**: A button that, when clicked, opens the `CreateOrganizationModal`.
-   **Main elements**: A `<button>` element.
-   **Handled interactions**: `click`: Triggers a state management action to open the modal.
-   **Handled validation**: None.
-   **Types**: None.
-   **Props**: None.

### `NavItem.vue`
-   **Component description**: A reusable component that represents a single link in the navigation sidebar. It includes an icon and a text label and indicates when the link is active.
-   **Main elements**: An Astro `<Link>` or `<a>` tag wrapped in a `<li>` or `<div>`. It will contain an icon and a `<span>` for the text.
-   **Handled interactions**: `click`: Navigates the user to the specified route.
-   **Handled validation**: None.
-   **Types**: `NavItemVM`.
-   **Props**:
    -   `item: NavItemVM`
    -   `isActive: boolean`

### `CreateOrganizationModal.vue`
-   **Component description**: A modal dialog that contains the form for creating a new organization.
-   **Main elements**: A modal/dialog component from `shadcn/ui` that contains a title, the `CreateOrganizationForm.vue` component, and action buttons (e.g., "Create", "Cancel").
-   **Handled interactions**: `close`: When the user clicks the cancel button or outside the modal. `submit`: When the form inside is submitted.
-   **Handled validation**: None directly (delegated to the form).
-   **Types**: None.
-   **Props**:
    -   `isOpen: boolean`

### `CreateOrganizationForm.vue`
-   **Component description**: The form used to collect the new organization's name.
-   **Main elements**: A `<form>` element with a text `<input>` for the organization name and a submit `<button>`.
-   **Handled interactions**: `submit`: Emits the form data.
-   **Handled validation**: The organization name field must not be empty. An error message will be displayed if the validation fails.
-   **Types**: `CreateOrganizationRequestDto`.
-   **Props**: None.

## 5. Types

### DTOs (Data Transfer Objects from API)
```typescript
// GET /api/organizations - Response item
interface OrganizationDto {
  id: string;
  name: string;
}

// POST /api/organizations - Request body
interface CreateOrganizationRequestDto {
  name: string;
}

// From user profile/session endpoint
interface UserDto {
  id: string;
  email: string;
  displayName: string;
  memberships: MembershipDto[];
}

interface MembershipDto {
  organizationId: string;
  organizationName: string;
  role: 'Owner' | 'Member' | 'Viewer';
}
```

### ViewModels (Types for Frontend Components)
```typescript
// Used in OrganizationSwitcher and organizationStore
interface OrganizationVM {
  id: string;
  name: string;
}

// Represents the full user session state
interface UserVM {
  id: string;
  email: string;
  displayName: string;
  currentOrganizationId: string;
  currentRole: 'Owner' | 'Member' | 'Viewer';
  organizations: OrganizationVM[];
}

// For rendering navigation links
interface NavItemVM {
  to: string;
  label: string;
  icon: string; // e.g., SVG component name or path
  requiredRole?: 'Owner'; // Only render for this role or higher
}
```

## 6. State Management
State will be managed using Pinia. Two stores will be primarily involved.

### `organizationStore` (`src/stores/organization.ts`)
-   **State**:
    -   `user: UserVM | null`: Stores all information about the logged-in user session.
    -   `organizations: OrganizationVM[]`: The list of organizations the user belongs to.
    -   `currentOrganization: OrganizationVM | null`: The currently selected organization.
    -   `currentRole: 'Owner' | 'Member' | 'Viewer' | null`: The user's role in the current organization.
-   **Actions**:
    -   `fetchUserData()`: Fetches the `UserDto` from the backend and populates the store's state.
    -   `switchOrganization(organizationId: string)`: Updates `currentOrganization` and `currentRole`. This will likely trigger a re-fetch of organization-specific data in other parts of the app.
    -   `createOrganization(data: CreateOrganizationRequestDto)`: Calls the API to create a new organization, then re-fetches user data or adds the new organization to the list and switches to it.
    -   `logout()`: Clears all state and redirects to the login page.

### `uiStore` (`src/stores/ui.ts`)
-   **State**:
    -   `isCreateOrganizationModalOpen: boolean`: Controls the visibility of the `CreateOrganizationModal`.
-   **Actions**:
    -   `openCreateOrganizationModal()`: Sets `isCreateOrganizationModalOpen` to `true`.
    -   `closeCreateOrganizationModal()`: Sets `isCreateOrganizationModalOpen` to `false`.

## 7. API Integration
-   A wrapper around the native `fetch` API will be created to automatically inject the `Authorization: Bearer <token>` and `X-Organization-Id: <currentOrganizationId>` headers into every relevant API request. The current organization ID will be retrieved from the `organizationStore`.
-   **`GET /api/users/me`**: Called by `organizationStore.fetchUserData()` on application startup to get all necessary session information.
    -   **Response Type**: `UserDto`.
-   **`POST /api/organizations`**: Called by `organizationStore.createOrganization()`.
    -   **Request Type**: `CreateOrganizationRequestDto`.
    -   **Response Type**: `OrganizationDto`.

## 8. User Interactions
-   **Switching Organization**: User clicks the `OrganizationSwitcher`, selects a new organization from the dropdown. The `switchOrganization` action is dispatched, the `currentOrganization` in the store updates, and the UI re-renders with the new context.
-   **Creating an Organization**: User clicks `CreateOrganizationButton`. The `uiStore` opens the `CreateOrganizationModal`. User fills in the name in `CreateOrganizationForm` and submits. The `createOrganization` action is called. On success, the modal closes, the new organization appears in the `OrganizationSwitcher`, and it becomes the active organization.
-   **Navigation**: User clicks a `NavItem`. The application navigates to the specified route using Astro's router.
-   **Role-Based Navigation**: The "Settings" `NavItem` will only be rendered if the `currentRole` in the `organizationStore` is 'Owner'.
-   **Logout**: User clicks the `LogoutButton`. The `logout` action is dispatched, clearing Pinia state and local storage, and redirecting the user to `/login`.

## 9. Conditions and Validation
-   **Role-Based Access**: The `SidebarNav.vue` component will be responsible for filtering the list of `NavItemVM`s based on the `currentRole` from the `organizationStore`.
-   **Form Validation**: `CreateOrganizationForm.vue` will validate that the `name` field is not empty before enabling the submit button and attempting to call the API.
-   **No Organizations State**: If `organizations` array in the store is empty, the `OrganizationSwitcher` should be disabled or hidden, and the `CreateOrganizationButton` should be highlighted to guide the user's first action.

## 10. Error Handling
-   **API Failures**: If the initial `fetchUserData` call fails, a full-page error state should be shown, preventing the app from loading.
-   **Creation Failure**: If the `createOrganization` API call fails (e.g., validation error, network error), the `CreateOrganizationModal` will remain open, and an error message returned from the API will be displayed within the form.
-   **UI Feedback**: Success or failure of asynchronous actions (like creating an organization) will be communicated to the user via toast notifications.

## 11. Implementation Steps
1.  **Setup Pinia Stores**: Create and configure `organizationStore` and `uiStore` with the state and actions defined above.
2.  **API Service Layer**: Implement the API service functions for `fetchUserData` and `createOrganization`, including the `fetch` wrapper for adding headers.
3.  **Build Child Components**:
    -   Create the stateless `NavItem.vue` component that accepts `item` and `isActive` props.
    -   Create `OrganizationSwitcher.vue`, which takes the list of organizations and the current one as props and emits an event on selection.
    -   Create the `CreateOrganizationButton.vue` which dispatches an action to open the modal.
    -   Create the `CreateOrganizationForm.vue` with validation for the name field.
    -   Create the `CreateOrganizationModal.vue` which uses the `uiStore` to manage its open/closed state and contains the form.
4.  **Assemble `SidebarNav.vue`**:
    -   Build the main `SidebarNav.vue` container.
    -   Connect it to the `organizationStore` and `uiStore` to get state (e.g., user's organizations, current role, modal visibility).
    -   Render the child components, passing the required props and listening for events.
    -   Implement the logic to filter navigation items based on `currentRole`.
5.  **Integrate into Layout**: Add the `<SidebarNav />` component to the `src/layouts/MainLayout.astro` file.
6.  **Initial Data Fetch**: Ensure that the `organizationStore.fetchUserData()` action is called once when the application first loads for an authenticated user.
7.  **Implement Logout**: Create the `LogoutButton.vue` component and connect its click event to the `organizationStore.logout()` action.
8.  **Styling**: Apply Tailwind CSS classes to all components to match the design system.
