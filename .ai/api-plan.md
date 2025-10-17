# REST API Plan

## 1. Resources
- **Users**: Manages user profiles and authentication. Corresponds to the `auth.users` table in Supabase.
- **Organizations**: Manages organizations (tenants). Corresponds to the `app.organizations` table.
- **Members**: Manages users within an organization. Corresponds to the `app.organization_members` table.
- **Invitations**: Manages invitations for users to join an organization. Corresponds to the `app.invitations` table.
- **Warehouses**: Manages warehouses within an organization. Corresponds to the `app.warehouses` table.
- **Locations**: Manages locations within a warehouse. Corresponds to the `app.locations` table.
- **ProductTemplates**: Manages master product definitions. Corresponds to the `app.product_templates` table.
- **Inventory**: Manages the summary of stock levels. Corresponds to the `app.inventory` table.
- **StockMovements**: Manages the immutable log of all inventory changes. Corresponds to the `app.stock_movements` table.
- **Dashboard**: A virtual resource providing aggregated data for the main dashboard.

## 2. Endpoints

### Authentication & User Management

- **`POST /api/auth/register`**
  - **Description**: Complete user registration flow after Supabase authentication. Creates user profile and optionally creates their first organization.
  - **Request Body**:
    ```json
    {
      "email": "user@example.com",
      "displayName": "John Doe",
      "createOrganization": true,
      "organizationName": "My Company" // Required if createOrganization is true
    }
    ```
  - **Response (201 Created)**:
    ```json
    {
      "user": {
        "id": "uuid",
        "email": "user@example.com",
        "displayName": "John Doe"
      },
      "organization": {
        "id": "uuid",
        "name": "My Company"
      } // Only included if createOrganization is true
    }
    ```
  - **Error Codes**: `400 Bad Request` (validation error), `409 Conflict` (user already exists).
  - **Note**: This endpoint should be called after successful Supabase authentication. The JWT token should be validated to ensure the user is authenticated.

- **`GET /api/auth/me`**
  - **Description**: Get current user profile information.
  - **Response (200 OK)**:
    ```json
    {
      "id": "uuid",
      "email": "user@example.com",
      "displayName": "John Doe"
    }
    ```
  - **Error Codes**: `401 Unauthorized`.

- **`PUT /api/auth/me`**
  - **Description**: Update current user profile information.
  - **Request Body**:
    ```json
    {
      "displayName": "Updated Name"
    }
    ```
  - **Response (200 OK)**: Updated user profile object.
  - **Error Codes**: `400 Bad Request`, `401 Unauthorized`.

### Organizations

- **`GET /api/organizations`**
  - **Description**: Retrieve a paginated list of organizations the current user belongs to.
  - **Query Parameters**: `page` (number, default: 1), `pageSize` (number, default: 10).
  - **Response (200 OK)**:
    ```json
    {
      "data": [
        {
          "id": "uuid",
          "name": "string"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 10,
        "total": 1
      }
    }
    ```

- **`POST /api/organizations`**
  - **Description**: Create a new organization. The creator is automatically assigned the 'Owner' role.
  - **Request Body**:
    ```json
    {
      "name": "string"
    }
    ```
  - **Response (201 Created)**:
    ```json
    {
      "id": "uuid",
      "name": "string"
    }
    ```
  - **Error Codes**: `400 Bad Request` (validation error), `401 Unauthorized`.

- **`PUT /api/organizations/{orgId}`**
  - **Description**: Update an organization's name. (Requires 'Owner' role).
  - **Request Body**:
    ```json
    {
      "name": "string"
    }
    ```
  - **Response (200 OK)**:
    ```json
    {
      "id": "uuid",
      "name": "string"
    }
    ```
  - **Error Codes**: `400 Bad Request` (validation error), `401 Unauthorized`, `403 Forbidden`, `404 Not Found`.

### Members & Invitations (Scoped to an Organization)

- **`GET /api/organizations/{orgId}/members`**
  - **Description**: Get all members and pending invitations for an organization.
  - **Query Parameters**: `page` (number, default: 1), `pageSize` (number, default: 10).
  - **Response (200 OK)**:
    ```json
    {
      "data": [
        {
          "userId": "uuid", // For pending invites, this is the ID of the invited user.
          "email": "user@example.com", // The email of the member or invited user.
          "role": "owner" | "member" | "viewer",
          "status": "accepted" | "pending"
        }
      ],
      "pagination": { ... }
    }
    ```
  - **Error Codes**: `401 Unauthorized`, `403 Forbidden`, `404 Not Found`.

- **`POST /api/organizations/{orgId}/invitations`**
  - **Description**: Invite a new user to the organization by their user ID. (Requires 'Owner' role).
  - **Request Body**:
    ```json
    {
      "invitedUserId": "uuid",
      "role": "member" | "viewer"
    }
    ```
  - **Response (201 Created)**:
    ```json
    {
      "id": "uuid",
      "invitedUserId": "uuid",
      "role": "member",
      "status": "pending"
    }
    ```
  - **Error Codes**: `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `409 Conflict` (invitation for this user already exists).

- **`DELETE /api/organizations/{orgId}/members/{userId}`**
  - **Description**: Remove a member from the organization. (Requires 'Owner' role).
  - **Response (204 No Content)**
  - **Error Codes**: `401 Unauthorized`, `403 Forbidden`, `404 Not Found`.

- **`DELETE /api/organizations/{orgId}/invitations/{invitationId}`**
  - **Description**: Cancel a pending invitation. (Requires 'Owner' role).
  - **Response (204 No Content)**
  - **Error Codes**: `401 Unauthorized`, `403 Forbidden`, `404 Not Found`.

### Invitations (for the current user)

- **`GET /api/invitations`**
  - **Description**: Get all pending invitations for the currently logged-in user.
  - **Response (200 OK)**:
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
  - **Error Codes**: `401 Unauthorized`.

- **`POST /api/invitations/{invitationId}/accept`**
  - **Description**: Accept an invitation to join an organization.
  - **Response (204 No Content)**
  - **Logic**: Creates an `organization_members` record and updates the invitation `status` to 'accepted'.
  - **Error Codes**: `401 Unauthorized`, `403 Forbidden` (if not the invited user), `404 Not Found`.

- **`POST /api/invitations/{invitationId}/decline`**
  - **Description**: Decline an invitation to join an organization.
  - **Response (204 No Content)**
  - **Logic**: Updates the invitation `status` to 'declined'.
  - **Error Codes**: `401 Unauthorized`, `403 Forbidden` (if not the invited user), `404 Not Found`.

### Warehouses

- **`GET /api/warehouses`**
  - **Description**: Retrieve a list of warehouses for the active organization.
  - **Query Parameters**: `page`, `pageSize`.
  - **Response (200 OK)**: Paginated list of warehouse objects.
    ```json
    {
      "data": [
        {
          "id": "uuid",
          "name": "string"
        }
      ],
      "pagination": { ... }
    }
    ```

- **`POST /api/warehouses`**
  - **Description**: Create a new warehouse. (Requires 'Owner' or 'Member' role).
  - **Request Body**: `{ "name": "string" }`
  - **Response (201 Created)**: The new warehouse object.

- **`PUT /api/warehouses/{warehouseId}`**
  - **Description**: Update a warehouse's details. (Requires 'Owner' or 'Member' role).
  - **Request Body**: `{ "name": "string" }`
  - **Response (200 OK)**: The updated warehouse object.

- **`DELETE /api/warehouses/{warehouseId}`**
  - **Description**: Delete a warehouse and all its associated locations and inventory (cascading delete). (Requires 'Owner' role).
  - **Response (204 No Content)**

### Locations (Scoped to a Warehouse)

- **`GET /api/warehouses/{warehouseId}/locations`**
  - **Description**: Get all locations for a specific warehouse.
  - **Query Parameters**: `page`, `pageSize`.
  - **Response (200 OK)**: Paginated list of location objects.
    ```json
    {
      "data": [
        {
          "id": "uuid",
          "name": "string",
          "description": "string"
        }
      ],
      "pagination": { ... }
    }
    ```

- **`POST /api/warehouses/{warehouseId}/locations`**
  - **Description**: Create a new location within a warehouse. (Requires 'Owner' or 'Member' role).
  - **Request Body**: `{ "name": "string", "description": "string" }`
  - **Response (201 Created)**: The new location object.

### Product Templates

- **`GET /api/product-templates`**
  - **Description**: Get product templates for the active organization.
  - **Query Parameters**: `page`, `pageSize`, `search` (string).
  - **Response (200 OK)**: Paginated list of product template objects.
    ```json
    {
      "data": [
        {
          "id": "uuid",
          "name": "string",
          "barcode": "string",
          "description": "string",
          "lowStockThreshold": 0
        }
      ],
      "pagination": { ... }
    }
    ```

- **`POST /api/product-templates`**
  - **Description**: Create a new product template. (Requires 'Owner' or 'Member' role).
  - **Request Body**: `{ "name": "string", "barcode": "string", "description": "string", "lowStockThreshold": 0 }`
  - **Response (201 Created)**: The new product template object.
  - **Error Codes**: `409 Conflict` if barcode is not unique for the organization.

### Inventory

- **`GET /api/inventory`**
  - **Description**: Get a summary of inventory levels.
  - **Query Parameters**: `page`, `pageSize`, `locationId` (uuid), `productTemplateId` (uuid), `lowStock` (boolean).
  - **Response (200 OK)**: Paginated list of inventory summary objects.
    ```json
    {
      "data": [
        {
          "product": { "id": "uuid", "name": "string" },
          "location": { "id": "uuid", "name": "string" },
          "quantity": 100
        }
      ],
      "pagination": { ... }
    }
    ```

### Stock Movements

- **`GET /api/stock-movements`**
  - **Description**: Get the immutable log of stock movements.
  - **Query Parameters**: `page`, `pageSize`, `productTemplateId` (uuid), `locationId` (uuid).
  - **Response (200 OK)**: Paginated list of stock movement objects.

- **`POST /api/stock-movements`**
  - **Description**: Create a new stock movement (Add, Withdraw, Move, Reconcile). This is the primary endpoint for all inventory changes. (Requires 'Owner' or 'Member' role).
  - **Request Body**:
    ```json
    // For 'add', 'withdraw', 'reconcile'
    {
      "productTemplateId": "uuid",
      "movementType": "add" | "withdraw" | "reconcile",
      "locationId": "uuid", // from_location for withdraw, to_location for add/reconcile
      "delta": 10 // For reconcile, this is the *new total quantity*
    }

    // For 'move'
    {
      "productTemplateId": "uuid",
      "movementType": "move",
      "fromLocationId": "uuid",
      "toLocationId": "uuid",
      "delta": 5 // Amount to move
    }
    ```
  - **Response (201 Created)**: The created stock movement record.

### Dashboard

- **`GET /api/dashboard`**
  - **Description**: Retrieve aggregated data for the main dashboard.
  - **Response (200 OK)**:
    ```json
    {
      "recentMovements": [ /* last 5 stock movement objects */ ],
      "lowStockAlerts": [ /* list of inventory objects where quantity <= lowStockThreshold */ ]
    }
    ```

## 3. Authentication and Authorization

- **Authentication**: Authentication will be handled via JSON Web Tokens (JWTs) provided by Supabase. The client must include the JWT in the `Authorization` header of every request (`Authorization: Bearer <token>`). The .NET API backend will validate the token on every incoming request.

- **Authorization**:
  - The API will be multi-tenant. A custom claim in the JWT (e.g., `active_org_id`) will specify the user's currently active organization. All API calls will be scoped to this organization ID to enforce data isolation.
  - Role-Based Access Control (RBAC) will be implemented as middleware or action filters in .NET.
    - **Viewer**: Can only perform `GET` requests.
    - **Member**: Can perform `GET` requests and `POST`/`PUT`/`DELETE` on `Warehouses`, `Locations`, `ProductTemplates`, and `POST` on `StockMovements`.
    - **Owner**: Has full permissions, including managing members and invitations.
  - If a user attempts an action they are not authorized for, the API will respond with a `403 Forbidden` status code.

## 4. Validation and Business Logic

- **Validation**:
  - All incoming request bodies will be validated. Required fields must be present, data types must be correct, and values must be within valid ranges (e.g., `quantity >= 0`).
  - The API will return a `400 Bad Request` response with a structured error message detailing validation failures.
  - Uniqueness constraints (e.g., product barcode per organization) will be enforced, returning a `409 Conflict` if violated.

- **Business Logic**:
  - **User Registration**: The `POST /api/auth/register` endpoint will handle the complete registration flow:
    1. Validate the JWT token to ensure the user is authenticated via Supabase.
    2. Create or update the user profile in the local database.
    3. If `createOrganization` is true, create a new organization and assign the user as the owner.
    4. Return the user profile and organization (if created) information.
  - **Inventory Management**: The `POST /api/stock-movements` endpoint will encapsulate the core business logic. When a movement is created, the API will:
    1. Create an immutable `stock_movements` record.
    2. Update the `inventory` summary table in the same transaction to ensure data consistency. For a 'move', this involves decrementing from the source and incrementing at the destination. For 'reconcile', it sets the quantity directly.
  - **Cascading Deletes**: The `DELETE /api/warehouses/{warehouseId}` endpoint will rely on the `ON DELETE CASCADE` constraint defined in the database to automatically remove associated locations. Additional logic may be required to clean up related `inventory` and `stock_movements` records if not handled by the database schema directly.
  - **User Roles**: When a new organization is created via `POST /api/organizations` or during registration, the calling user will be automatically inserted into the `organization_members` table with the `owner` role.
