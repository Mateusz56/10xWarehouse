# View Implementation Plan: Create Organization

## 1. Overview
This document outlines the implementation plan for the "Create Organization" feature. The purpose of this feature is to allow an authenticated user to create a new organization by providing a name. The creation process will be handled within a modal, triggered from a button in the main sidebar navigation, consistent with the application's UI patterns for resource creation.

## 2. View Routing
This feature is implemented as a modal and does not have a dedicated URL path. The modal will be triggered by a global application event or state change, making it accessible from any view where the main sidebar is present.

## 3. Component Structure
The feature will be composed of the following components in a parent-child hierarchy:

```
SidebarNav
└── CreateOrganizationButton
    └── (triggers) CreateOrganizationModal
        └── CreateOrganizationForm
            ├── TextInput (for organization name)
            └── Button (for submit)
```

## 4. Component Details
### CreateOrganizationButton
-   **Component description**: A simple button component, likely an icon and text, placed in the `SidebarNav`. Its sole purpose is to open the `CreateOrganizationModal`.
-   **Main elements**: A `<button>` element.
-   **Handled interactions**:
    -   `click`: Emits an event or updates a global Pinia state variable to set the `CreateOrganizationModal` to be visible.
-   **Handled validation**: None.
-   **Types**: None.
-   **Props**: None.

### CreateOrganizationModal
-   **Component description**: A modal dialog that houses the form for creating a new organization. It handles the visibility of the form and provides a mechanism to close it.
-   **Main elements**: A modal/dialog wrapper component (e.g., from `shadcn/ui`), a title (e.g., "Create a new organization"), and a close button. It will contain the `CreateOrganizationForm` component as a child.
-   **Handled interactions**:
    -   `close`: Handles requests to close the modal (e.g., clicking the 'X' button, pressing Escape, or clicking the overlay). This will update the global state to hide the modal.
-   **Handled validation**: None.
-   **Types**: None.
-   **Props**:
    -   `isOpen (boolean)`: Controls the visibility of the modal, passed down from a parent component or read from a global store.

### CreateOrganizationForm
-   **Component description**: The core form for creating an organization. It contains the input field for the organization name and the submit button. It is responsible for handling user input, validation, and the API submission process.
-   **Main elements**: A `<form>` element containing one `TextInput` component for the name, and one `Button` component for submission. It will also include an area to display form-level error messages from the API.
-   **Handled interactions**:
    -   `input`: Updates the local state for the organization name.
    -   `submit`: Triggers the form submission process, including validation and the API call to `POST /api/organizations`.
-   **Handled validation**:
    -   **Organization Name**:
        -   **Required**: The field cannot be empty.
        -   **Minimum Length**: Must be at least 3 characters long.
        -   **Maximum Length**: Must be no more than 100 characters long.
-   **Types**: `CreateOrganizationRequestDto`, `OrganizationDto`, `CreateOrganizationViewModel`.
-   **Props**: None.

## 5. Types
### DTOs (from Backend)
-   **`CreateOrganizationRequestDto`**: The object sent to the API.
    ```typescript
    interface CreateOrganizationRequestDto {
      name: string;
    }
    ```
-   **`OrganizationDto`**: The object received from the API upon successful creation.
    ```typescript
    interface OrganizationDto {
      id: string; // uuid
      name: string;
    }
    ```

### ViewModels (Frontend-specific)
-   **`CreateOrganizationViewModel`**: The local state model for the `CreateOrganizationForm` component.
    ```typescript
    interface CreateOrganizationViewModel {
      name: string;
      isLoading: boolean;
      error: string | null;
    }
    ```
    -   `name`: Bound to the organization name input field.
    -   `isLoading`: A boolean to track the submission status, used to disable the submit button and show a loading indicator.
    -   `error`: A string to hold any form-level error messages returned from the API (e.g., "An unexpected error occurred.").

## 6. State Management
-   **Local Component State**: The `CreateOrganizationForm` will manage its own UI state (`name`, `isLoading`, `error`) using Vue's `ref` or `reactive`. This state is encapsulated within the form and does not need to be shared globally.
-   **Global State (Pinia)**: A global `uiStore` will manage the visibility of the `CreateOrganizationModal`.
    -   **State**: `isCreateOrganizationModalOpen: boolean`.
    -   **Actions**: `openCreateOrganizationModal()`, `closeCreateOrganizationModal()`.
    -   The `CreateOrganizationButton` calls `open...`, and the `CreateOrganizationModal` calls `close...`.
-   The `orgStore` will be updated after a successful creation to add the new organization to the user's list and potentially switch to it.

## 7. API Integration
-   **Endpoint**: `POST /api/organizations`
-   **Integration Logic**: The `onSubmit` method in the `CreateOrganizationForm` will handle the API call.
    1.  Set `isLoading` to `true` and clear any previous `error`.
    2.  Validate the form fields. If invalid, stop and display validation errors.
    3.  Construct the `CreateOrganizationRequestDto` object from the form's state.
    4.  Make a `POST` request to `/api/organizations` using `fetch`, including the `Authorization: Bearer <token>` header and the JSON payload.
    5.  **On Success (200-299 status)**:
        -   Parse the `OrganizationDto` from the response.
        -   Call an action in the `orgStore` to add the new organization to the global list.
        -   Call `closeCreateOrganizationModal()` from the `uiStore`.
        -   Display a success toast notification (e.g., "Organization created!").
    6.  **On Failure**:
        -   Handle specific error codes (see Error Handling).
        -   Set the `error` state with a user-friendly message.
    7.  **Finally**: Set `isLoading` to `false`.

## 8. User Interactions
-   **Opening the Modal**: The user clicks the "Create Organization" button in the sidebar, which sets `isCreateOrganizationModalOpen` to `true`, displaying the modal.
-   **Entering Data**: The user types the desired name into the text input. Inline validation provides immediate feedback if the name is too short or long.
-   **Submitting**: The user clicks the "Create" button. The button becomes disabled, and a loading indicator may appear.
-   **Closing the Modal**: The user can close the modal by clicking the 'X' button, pressing the `Escape` key, or clicking the overlay. This resets the form state.

## 9. Conditions and Validation
-   **Submission Button State**: The "Create" button in the `CreateOrganizationForm` will be disabled if:
    -   The `name` field is invalid (empty, <3 chars, >100 chars).
    -   `isLoading` is `true` (an API request is in progress).
-   **Input Validation**: The `TextInput` component will display a visual error state (e.g., a red border) and an error message if its validation rules are not met. Validation will be handled by a library like `VeeValidate` with a `Zod` schema.

## 10. Error Handling
-   **400 Bad Request**: The API may return this for validation errors not caught by the client or other issues. The response body should be inspected for details, and a generic error message like "Invalid data provided." can be displayed in the form's error area.
-   **401 Unauthorized**: The user's token is invalid or missing. The application should perform a global redirect to the `/login` page.
-   **Network Errors**: If the `fetch` call fails due to a network issue, a generic error message like "Could not connect to the server. Please try again later." will be displayed in the form's error area.
-   **Other 5xx Errors**: For any other server-side errors, a generic message "An unexpected error occurred. Please try again." will be displayed.

## 11. Implementation Steps
1.  **State Management**: Add `isCreateOrganizationModalOpen` state and corresponding actions to the `uiStore` in Pinia.
2.  **Sidebar Button**: Create the `CreateOrganizationButton.vue` component and add it to the `SidebarNav`. Wire its `click` event to the `openCreateOrganizationModal` Pinia action.
3.  **Modal Component**: Create the `CreateOrganizationModal.vue` component. It should read its visibility from the `isCreateOrganizationModalOpen` state and call the `close...` action when it needs to be hidden.
4.  **Form Component**: Create the `CreateOrganizationForm.vue` component.
    -   Set up local state (`name`, `isLoading`, `error`).
    -   Build the form layout with a `TextInput` and a `Button`.
    -   Implement the validation logic for the `name` field using `VeeValidate` and `Zod`.
5.  **API Logic**: Implement the `onSubmit` method in `CreateOrganizationForm.vue`.
    -   Write the `fetch` call to the `POST /api/organizations` endpoint.
    -   Implement the full success and error handling logic.
6.  **Global State Update**: After a successful API call, dispatch an action to the `orgStore` to update the list of organizations.
7.  **Final Integration**: Place the `CreateOrganizationForm` inside the `CreateOrganizationModal` and ensure all props and events are correctly handled. Test the end-to-end flow.
