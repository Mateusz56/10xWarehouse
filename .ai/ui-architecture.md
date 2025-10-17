# UI Architecture Plan for 10xWarehouse MVP

This document outlines the user interface architecture, key components, and user flow patterns for the 10xWarehouse application, based on the established product requirements and API plan.

## 1. Overall Structure & Navigation

-   **Main Navigation**: A collapsible vertical sidebar will be used for desktop navigation, transitioning to a hamburger menu on smaller screens.
-   **Navigation Links**: The sidebar will contain links to:
    -   Dashboard
    -   Inventory
    -   Warehouses
    -   Products
    -   Settings
    -   Profile
-   **Organization Switching**: A dropdown component will be placed in the main header/navigation bar to allow users to switch between their organizations. Selecting an organization will update the application's context and re-fetch relevant data.

## 2. Page & View Layouts

-   **General Approach**: The application will adopt a desktop-first, card-based design for most views to ensure a clean and modern interface.
-   **Dashboard**: A two-column grid layout.
    -   The left column will feature "Low Stock Alerts" displayed as a list of cards.
    -   The right column will display "Recent Movements" in a data grid.
-   **Warehouses & Locations**:
    -   The main `/warehouses` page will show a collection of cards, each representing a warehouse.
    -   Clicking a warehouse card will navigate to `/warehouses/{id}`, which will display warehouse details and a paginated list of cards for its associated locations.
-   **Product Templates**: A paginated card view. Each card will display the product's name, barcode, a truncated description, and action buttons.
-   **Inventory Summary**: A card-based view. Each card represents a unique product-location combination. A filter bar at the top will allow filtering by location, product, and a "low stock" toggle.
-   **Stock Movement Log**: A read-only data grid.
    -   **Columns**: `Date`, `Product`, `Movement Type`, `Quantity Change` (delta), `Location(s)`, `Calculated Quantity Before`, and `Quantity After`.
-   **Profile Page**: A single-column layout featuring user account management.
    -   **Display Name Section**: A form card allowing users to update their display name with real-time validation and a "Save Changes" button.
    -   **Password Change Section**: A separate form card for password updates, requiring current password confirmation and new password with strength indicator.
    -   **Account Information**: Read-only display of user email and account creation date.
    -   **Security Actions**: Additional options for account security (e.g., logout from all devices, if supported by the backend).

## 3. Core UI Components & Patterns

-   **Forms (Create/Edit)**: All forms for creating or editing resources (Warehouses, Products, etc.) will be presented in modals.
-   **Inventory Actions**:
    -   An "Add Stock" button will be located in the top-right corner of the Inventory Summary view.
    -   `Withdraw`, `Move`, and `Reconcile` actions will be initiated via buttons on each row/card of the Inventory Summary view.
    -   A "New Movement" button on the Stock Movement Log page will open a modal, allowing the user to select a movement type and fill out a dynamic form.
-   **User Invitations**: An "Invite User" modal (accessible from Organization Settings) will feature a searchable autocomplete input to find and select users by email, requiring a `GET /api/users?search=...` endpoint.
-   **Pagination**: A standardized pagination component will be used for all long lists of data, including products, inventory, and locations within a warehouse.
-   **Empty States**: All views that display lists will have a dedicated empty state component, featuring an icon, a helpful message, and a primary call-to-action button (e.g., "Create your first Warehouse").
-   **Deletion Confirmation**: Deleting a warehouse will trigger a two-step confirmation modal that requires the user to type the warehouse's name to confirm the destructive action.

## 4. State Management & Data Fetching

-   **State Management Library**: `Pinia` will be used for centralized state management.
-   **Pinia Stores**:
    -   `authStore`: Manages user authentication state, JWT, and profile.
    -   `orgStore`: Manages the list of organizations, the active organization, and the user's role.
    -   `uiStore`: Manages global UI state (e.g., modals, notifications).
    -   Data stores (e.g., `inventoryStore`, `productStore`) will be created as needed.
-   **Data Fetching Strategy**: The native JavaScript `fetch` API will be used for all API communication.
-   **Data Freshness**: To ensure data is current, the application will re-fetch data every time a user revisits a view.
-   **Loading & Error States**: Each Pinia store responsible for data fetching will manage its own `isLoading` and `error` state properties. Components will use these states to render loading spinners or error messages.

## 5. Forms & Validation

-   **Validation Library**: `VeeValidate` will be used for client-side form management, paired with `Zod` for schema definition.
-   **User Experience**: Forms will feature real-time, inline validation, providing immediate feedback to the user as they type.

## 6. Authentication & Authorization

-   **Role-Based UI**: The user's role for the active organization (retrieved from `orgStore`) will be used with `v-if` directives to conditionally render UI elements (e.g., create/edit/delete buttons), aligning the frontend with the API's RBAC rules.
-   **Error Handling**:
    -   **4xx/5xx Errors**: Handled via toast notifications for general server errors.
    -   **401/403 Errors**: Will trigger a redirection to a login or "access denied" page.
    -   **400 Validation Errors**: Displayed as inline messages next to the corresponding form fields.
