# Product Requirements Document (PRD) - 10xWarehouse

## 1. Product Overview

10xWarehouse is a web-based application designed for simple warehouse and inventory management. The project's primary purpose is educational: to serve as a complete case study for building a full-stack application with AI assistance, from planning to deployment.

The application allows users to manage products, warehouses, and the specific locations of products within those warehouses. It is a multi-tenant system where data is securely partitioned by `Organization`. A user can belong to multiple organizations and switch between them. The target user is a small business owner who needs a straightforward tool for inventory tracking without the complexity of enterprise-level systems.

The technology stack consists of a .NET REST API backend, a PostgreSQL database, Supabase for authentication, and a frontend built with Astro and Vue components, styled with Tailwind CSS and shadcn/ui.

## 2. User Problem

Small business owners often struggle with inventory management. They need to know what products they have, how many they have, and where they are located. Existing solutions can be overly complex, expensive, or require significant setup. The lack of a simple, accessible tool leads to disorganized spreadsheets, manual tracking errors, and wasted time trying to locate stock. This results in inaccurate inventory counts, potential overstocking or understocking, and inefficient operations. 10xWarehouse aims to solve this by providing a clean, intuitive, and focused application for core inventory tracking needs.

## 3. Functional Requirements

### 3.1. User and Organization Management
- Users must be able to register, log in, and log out via Supabase.
- A user can create a new `Organization`.
- A user can belong to and switch between multiple `Organizations`.
- The system must support three user roles with distinct permissions:
    - `Owner`: Full control over the organization, including user management.
    - `Member`: Can manage inventory but not users or warehouses.
    - `Viewer`: Read-only access to all data within the organization.
- An `Owner` can invite new users to an organization via email and assign them a role.

### 3.2. Warehouse and Location Management
- Full CRUD (Create, Read, Update, Delete) functionality for `Warehouses`.
- Full CRUD functionality for `Locations`, which are nested under a specific `Warehouse`.
- The system will display a strong warning before allowing an `Owner` to delete a `Warehouse` that contains inventory. The deletion will be a cascading operation, removing all associated locations and inventory records.

### 3.3. Product and Inventory Management
- Full CRUD for `Product Templates` (master product definitions), which include `Name`, `Barcode`, `Description`, and a `Low Stock Threshold`.
- Product barcodes must be unique within an `Organization`.
- The system must support four primary inventory operations:
    - `Add Stock`: Add new inventory for a product at a location.
    - `Withdraw Stock`: Remove inventory from a location.
    - `Move Stock`: Transfer inventory from one location to another within the same warehouse.
    - `Reconcile Stock`: Directly set the inventory count for a product at a location, with the system creating a corresponding audit log entry.
- Every inventory change must be recorded in an immutable `Stock Movement` log for a complete audit trail.

### 3.4. Dashboard and Reporting
- A main dashboard will serve as the landing page, displaying:
    - A list of the 5 most recent stock movements.
    - A "Low Stock Alerts" widget showing products whose total quantity is at or below their defined threshold.
- A simple, on-screen inventory summary report will be available, with filters for location and low-stock items.

### 3.5. Technical Requirements
- The backend will be a .NET REST API.
- All timestamps will be stored in UTC.
- The API will enforce role-based access control (RBAC) by validating Supabase JWTs.
- The API will provide clear, structured JSON error responses.
- All lists of data (products, movements, etc.) will be paginated.
- The frontend will implement real-time inline form validation.

## 4. Product Boundaries

### 4.1. In Scope for MVP
- All functional requirements listed in section 3.
- A web-based application accessible via modern browsers.
- Basic user profile management (changing display name).
- UI with empty states and calls-to-action to guide new users.

### 4.2. Out of Scope for MVP
- Monetization or subscription plans.
- Native mobile applications (iOS/Android).
- Advanced reporting (e.g., PDF or CSV exports).
- Barcode scanning via device camera.
- Integration with third-party e-commerce or accounting software.
- Multi-language support.

## 5. User Stories

### 5.1. Authentication and Authorization
- ID: US-001
- Title: User Registration
- Description: As a new user, I want to create an account using my email and a password so that I can access the application.
- Acceptance Criteria:
    - Given I am on the registration page,
    - When I enter a valid email and a strong password and submit the form,
    - Then my account is created, and I am automatically logged in and redirected to the dashboard.

- ID: US-002
- Title: User Login
- Description: As a registered user, I want to log in with my email and password to access my organizations and data.
- Acceptance Criteria:
    - Given I am on the login page,
    - When I enter my correct credentials and submit the form,
    - Then I am successfully logged in and redirected to my default organization's dashboard.

- ID: US-003
- Title: Role-Based Access Control
- Description: As a user with a specific role, I want my permissions to be enforced so that I can only perform actions I am authorized to do.
- Acceptance Criteria:
    - Given I am logged in as a `Viewer`,
    - Then I cannot see any "Create", "Edit", or "Delete" buttons in the UI.
    - And if I attempt to make a write-action API call, it must be rejected with a `403 Forbidden` status.

### 5.2. Organization and User Management
- ID: US-004
- Title: Create Organization
- Description: As a new user, I want to create my first organization so I can start managing my inventory.
- Acceptance Criteria:
    - Given I have just registered and have no organizations,
    - When I am prompted to create an organization and I provide a name,
    - Then a new, empty organization is created, and I am assigned as its `Owner`.

- ID: US-005
- Title: Invite User to Organization
- Description: As an `Owner`, I want to invite a new user to my organization by email and assign them a role.
- Acceptance Criteria:
    - Given I am an `Owner` on the user management page,
    - When I enter a valid email address, select a role (`Member` or `Viewer`), and send the invitation,
    - Then the user receives an invitation, and they appear in my user list as "Pending".

- ID: US-006
- Title: Switch Between Organizations
- Description: As a user belonging to multiple organizations, I want to easily switch between them.
- Acceptance Criteria:
    - Given I am logged in and belong to more than one organization,
    - When I click on the organization switcher in the navigation bar and select a different organization,
    - Then the application reloads to show the data for the selected organization.

### 5.3. Warehouse and Location Management
- ID: US-007
- Title: Create Warehouse
- Description: As an `Owner`, I want to create a new warehouse within my organization.
- Acceptance Criteria:
    - Given I am on the warehouses page,
    - When I click the "Create Warehouse" action and provide a unique name,
    - Then the new warehouse is created and appears in my list of warehouses.

- ID: US-008
- Title: Delete Warehouse
- Description: As an `Owner`, I want to delete a warehouse, even if it contains inventory.
- Acceptance Criteria:
    - Given I am an `Owner` and I initiate the delete action for a warehouse,
    - When a confirmation modal appears warning me that all associated data will be lost and I confirm,
    - Then the warehouse and all its locations and inventory records are permanently deleted.

- ID: US-009
- Title: Create Location
- Description: As an `Owner`, I want to create a location within a specific warehouse.
- Acceptance Criteria:
    - Given I am viewing a specific warehouse,
    - When I add a new location with a name and optional description,
    - Then the location is created and listed under that warehouse.

### 5.4. Product and Inventory Management
- ID: US-010
- Title: Create Product Template
- Description: As an `Owner`, I want to define a new product template for my organization.
- Acceptance Criteria:
    - Given I am on the products page,
    - When I create a new product and provide a `Name`, a unique `Barcode`, a `Description`, and a `Low Stock Threshold`,
    - Then the product template is saved and available to be added to inventory.

- ID: US-011
- Title: Add Stock
- Description: As an `Owner` or `Member`, I want to add stock for a product at a specific location.
- Acceptance Criteria:
    - Given I am at a location's inventory view,
    - When I select a product, enter a positive quantity, and confirm the addition,
    - Then the quantity of that product at that location is increased, and a `Stock Movement` record of type "add" is created.

- ID: US-012
- Title: Move Stock
- Description: As an `Owner` or `Member`, I want to move a quantity of a product from one location to another.
- Acceptance Criteria:
    - Given I initiate a "Move" action for a product at a source location,
    - When I select a valid destination location and a quantity less than or equal to the available amount,
    - Then the quantity is subtracted from the source and added to the destination, and a `Stock Movement` record of type "move" is created.

- ID: US-013
- Title: Reconcile Stock
- Description: As an `Owner` or `Member`, I want to correct the inventory count for a product after a physical stocktake.
- Acceptance Criteria:
    - Given I initiate a "Reconcile" action for a product at a location,
    - When I enter the new, correct quantity and save,
    - Then the inventory quantity is updated to the new value, and a `Stock Movement` record of type "reconciliation" is created.

### 5.5. Dashboard
- ID: US-014
- Title: View Dashboard
- Description: As a logged-in user, I want to see a dashboard with an overview of recent activity and important alerts.
- Acceptance Criteria:
    - Given I am logged in,
    - When I navigate to the dashboard,
    - Then I can see a widget with the 5 most recent stock movements in my current organization.
    - And I can see a widget listing all products whose total quantity is at or below their low stock threshold.

## 6. Success Metrics

- 6.1. Functionality: 100% of the user stories defined in this PRD for the MVP are implemented, tested, and working correctly in a staging environment.
- 6.2. Data Integrity: The application correctly enforces data rules, such as barcode uniqueness per organization and the creation of an accurate, immutable audit trail for every stock movement.
- 6.3. Security: The Role-Based Access Control system is fully functional, demonstrably preventing users from accessing or modifying data outside of their permissions.
- 6.4. Performance: All primary data lists (e.g., products, locations, stock movements) are paginated and load without noticeable delay, even with thousands of records.
