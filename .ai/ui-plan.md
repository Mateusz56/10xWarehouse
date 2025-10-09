# UI Architecture for 10xWarehouse

## 1. UI Structure Overview

The UI for 10xWarehouse will be a single-page application (SPA) built with Vue.js, following a desktop-first design philosophy. The core layout is composed of a persistent, collapsible vertical sidebar for main navigation and a header containing a user profile dropdown and an organization switcher.

The design prioritizes clarity and efficiency, using a card-based layout for most resource lists (Warehouses, Products, Inventory) to provide scannable information, and a data grid for dense, tabular data like the Stock Movement Log. User actions such as creating or editing resources are handled through modals to keep the user within their current context. State is managed centrally by Pinia, and all API interactions use the native `fetch` API, with data being refreshed on each view visit to ensure freshness. Role-Based Access Control (RBAC) is visually enforced by conditionally rendering UI elements based on the user's role.

## 2. View List

### Authentication Views

-   **View name**: Login
-   **View path**: `/login`
-   **Main purpose**: To allow registered users to authenticate and access the application.
-   **Key information to display**: Email and password fields, a "Login" button, and links to the registration and password reset pages.
-   **Key view components**: `LoginForm`, `TextInput`, `Button`.
-   **UX, accessibility, and security considerations**: The form will provide clear inline validation for both fields. The password field will be masked. All form elements will have associated labels for screen readers.

-   **View name**: Register
-   **View path**: `/register`
-   **Main purpose**: To allow new users to create an account.
-   **Key information to display**: Email, password, and confirm password fields.
-   **Key view components**: `RegisterForm`, `TextInput`, `Button`.
-   **UX, accessibility, and security considerations**: Real-time password strength indicators and validation will guide the user. All form elements will have associated labels.

### Core Application Views

-   **View name**: Dashboard
-   **View path**: `/`
-   **Main purpose**: To provide a high-level overview of recent activity and important alerts for the active organization.
-   **Key information to display**: A list of low-stock items and a log of the 5 most recent stock movements.
-   **Key view components**: `LowStockAlertsWidget` (card list), `RecentMovementsWidget` (grid), `DashboardLayout`.
-   **UX, accessibility, and security considerations**: The view serves as the main landing page. It should load quickly and provide clear calls-to-action within the widgets (e.g., clicking a low-stock item navigates to its inventory view).

-   **View name**: Inventory Summary
-   **View path**: `/inventory`
-   **Main purpose**: To view, filter, and manage inventory levels. This is the primary view for initiating stock operations.
-   **Key information to display**: A paginated list of inventory items, filterable by product, location, and low-stock status. Each item will show product name, location name, and current quantity.
-   **Key view components**: `FilterBar`, `InventoryCard`, `PaginationControl`, `AddStockModal`, `MoveStockModal`, `WithdrawStockModal`, `ReconcileStockModal`.
-   **UX, accessibility, and security considerations**: Action buttons (`Move`, `Withdraw`, `Reconcile`) will be conditionally rendered based on the user's role (`Member` or `Owner`). A global "Add Stock" button will also be role-restricted. The view must handle empty states gracefully.

-   **View name**: Stock Movement Log
-   **View path**: `/movements`
-   **Main purpose**: To provide a complete, read-only audit trail of all inventory changes.
-   **Key information to display**: A paginated data grid with columns for Date, Product, Movement Type, Quantity Change (Delta), Location(s), Calculated Quantity Before, and Quantity After.
-   **Key view components**: `DataGrid`, `PaginationControl`.
-   **UX, accessibility, and security considerations**: This is a read-only view for all authorized roles. The data grid must be accessible, with proper headers for screen readers.

-   **View name**: Warehouse List
-   **View path**: `/warehouses`
-   **Main purpose**: To display all warehouses within the active organization and allow for their creation.
-   **Key information to display**: A paginated list of cards, where each card shows the warehouse name and a summary (e.g., number of locations).
-   **Key view components**: `WarehouseCard`, `PaginationControl`, `CreateWarehouseModal`.
-   **UX, accessibility, and security considerations**: A prominent "Create Warehouse" button will be visible only to `Owners`. Each card will link to the `WarehouseDetailView`.

-   **View name**: Warehouse Detail
-   **View path**: `/warehouses/:id`
-   **Main purpose**: To display the details of a specific warehouse and manage its associated locations.
-   **Key information to display**: Warehouse name and details at the top, followed by a paginated list of location cards.
-   **Key view components**: `LocationCard`, `PaginationControl`, `CreateLocationModal`, `EditWarehouseModal`, `DeleteConfirmationModal`.
-   **UX, accessibility, and security considerations**: CRUD actions for locations are restricted to `Members` and `Owners`. Deleting the warehouse is restricted to `Owners` and requires a two-step confirmation to prevent accidental data loss.

-   **View name**: Product List
-   **View path**: `/products`
-   **Main purpose**: To manage the master product templates for the organization.
-   **Key information to display**: A paginated list of product cards, each showing the product name, barcode, and description.
-   **Key view components**: `ProductCard`, `PaginationControl`, `CreateProductModal`, `EditProductModal`.
-   **UX, accessibility, and security considerations**: CRUD actions for products are restricted to `Members` and `Owners`. The creation form must validate that the barcode is unique for the organization.

### Settings & Management Views

-   **View name**: Profile
-   **View path**: `/profile`
-   **Main purpose**: To allow a user to manage their own profile settings.
-   **Key information to display**: User's email address and an editable field for their display name.
-   **Key view components**: `ProfileForm`.
-   **UX, accessibility, and security considerations**: Standard form accessibility practices apply.

-   **View name**: Organization Settings
-   **View path**: `/settings/organization`
-   **Main purpose**: To allow `Owners` to manage organization members and invitations.
-   **Key information to display**: A list of current organization members (with their role) and a list of pending invitations.
-   **Key view components**: `MembersList`, `InvitationsList`, `InviteUserModal`.
-   **UX, accessibility, and security considerations**: This entire view is restricted to users with the `Owner` role. The invite modal requires a user search feature, which presents a potential data privacy consideration that must be handled by the API.

-   **View name**: My Invitations
-   **View path**: `/invitations`
-   **Main purpose**: To allow the logged-in user to see and respond to their pending invitations to join other organizations.
-   **Key information to display**: A list of invitations, each showing the organization name, assigned role, and "Accept"/"Decline" actions.
-   **Key view components**: `InvitationCard`.
-   **UX, accessibility, and security considerations**: This view is accessible to all logged-in users. Actions are clearly labeled.

## 3. User Journey Map

This map outlines the flow for a primary use case: **An Owner performs a physical stock count and reconciles the inventory for a product.**

1.  **Login & Dashboard**: The user logs in via the `LoginView` and lands on the `DashboardView`. They see a "Low Stock Alert" for the product they just counted.
2.  **Navigate to Inventory**: The user clicks the "Inventory" link in the `SidebarNav`.
3.  **Find the Item**: The user is now on the `InventorySummaryView`. They use the `FilterBar` to quickly find the specific product and location. The view updates to show the relevant `InventoryCard`.
4.  **Initiate Reconciliation**: The card shows the system quantity. The user clicks the "Reconcile" button on the card.
5.  **Update Quantity**: The `ReconcileStockModal` opens, pre-filled with the product and location details. The user enters the new, correct quantity from their physical count into a number input field and clicks "Save".
6.  **Confirmation**: The modal closes, and a `ToastNotification` appears confirming "Inventory reconciled successfully." The `InventoryCard` in the view immediately updates with the new quantity.
7.  **Verify Audit Log**: The user navigates to the `StockMovementLogView` via the sidebar. They see a new entry at the top of the grid with the type "Reconcile", confirming the operation was logged correctly.

## 4. Layout and Navigation Structure

-   **Primary Layout**: A two-part layout consisting of a collapsible `SidebarNav` on the left and a main content area.
-   **Sidebar Navigation**: The main method for navigating between the application's top-level views:
    -   Dashboard (`/`)
    -   Inventory (`/inventory`)
    -   Warehouses (`/warehouses`)
    -   Products (`/products`)
    -   Stock Movements (`/movements`)
    -   Profile (`/profile`)
    -   Settings (`/settings/organization`) - *Visible to Owners only*
    -   Contains the `OrganizationSwitcher` dropdown as first item in bar
    -   Add organization button next to `OrganizationSwitcher` dropdown
    -   Logout button
-   **Sub-Navigation**: Navigation to detail views (e.g., `WarehouseDetailView`) occurs by clicking on items within a list view (e.g., a `WarehouseCard`). Breadcrumbs can be used on detail pages to aid navigation back to the parent list.

## 5. Key Components

-   **SidebarNav**: The main collapsible navigation menu.
-   **OrganizationSwitcher**: A dropdown in the header for changing the active organization context.
-   **FilterBar**: A component used on list views (`InventorySummaryView`) with dropdowns and toggles to filter the displayed data.
-   **DataGrid**: A component for displaying tabular data, used in the `StockMovementLogView` and dashboard widgets. It will support pagination.
-   **[Resource]Card**: A generic card component pattern used for displaying items in lists (e.g., `WarehouseCard`, `ProductCard`, `InventoryCard`).
-   **[Action]Modal**: A generic modal component pattern for forms (e.g., `CreateProductModal`, `ReconcileStockModal`). It will contain form logic managed by VeeValidate.
-   **DeleteConfirmationModal**: A specialized modal for destructive actions, requiring the user to type the resource's name to confirm.
-   **PaginationControl**: A component used with grids and card lists to navigate between pages of data.
-   **ToastNotification**: A global component for displaying non-blocking feedback to the user (e.g., "Warehouse created successfully").
-   **EmptyState**: A placeholder component shown in list views when no data is available, containing a message and a call-to-action.
