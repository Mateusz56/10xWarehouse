This is the final database schema plan, created by analyzing the Product Requirements Document (PRD), session notes, and tech stack. It's designed to be comprehensive and serve as a blueprint for database migrations.

### 1. List of Tables, Columns, and Constraints

#### Custom Types (Enums)
- **`app.user_role`**: Represents user roles within an organization.
  - Values: `owner`, `member`, `viewer`
- **`app.movement_type`**: Defines the types of stock movements.
  - Values: `add`, `withdraw`, `move`, `reconcile`
- **`app.invitation_status`**: Tracks the status of user invitations.
  - Values: `pending`, `accepted`

#### Table Definitions

- **`app.organizations`**: Stores organization records.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `name`: `TEXT`, NOT NULL

- **`app.organization_members`**: Links users to organizations and defines their roles.
  - `organization_id`: `UUID`, Primary Key, Foreign Key to `app.organizations(id)`
  - `user_id`: `UUID`, Primary Key, Foreign Key to `auth.users(id)`
  - `role`: `app.user_role`, NOT NULL

- **`app.invitations`**: Stores pending invitations for users to join an organization.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `user_id`: `UUID`, Foreign Key to `auth.users(id)`
  - `role`: `app.user_role`, NOT NULL
  - `token`: `TEXT`, NOT NULL, UNIQUE
  - `status`: `app.invitation_status`, NOT NULL (default: `'pending'`)

- **`app.warehouses`**: Stores warehouse records for an organization.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `name`: `TEXT`, NOT NULL

- **`app.locations`**: Stores specific locations within a warehouse.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `warehouse_id`: `UUID`, Foreign Key to `app.warehouses(id)` with `ON DELETE CASCADE`, NOT NULL
  - `name`: `TEXT`, NOT NULL
  - `description`: `TEXT`, NULL

- **`app.product_templates`**: Master records for products.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `name`: `TEXT`, NOT NULL
  - `barcode`: `TEXT`, NULL
  - `description`: `TEXT`, NULL
  - `low_stock_threshold`: `DECIMAL`, NULL (default: 0), `CHECK (low_stock_threshold >= 0)`
  - **Constraint**: `UNIQUE(organization_id, barcode)`

- **`app.inventory`**: A summary table of current stock levels for each product at each location.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `product_template_id`: `UUID`, Foreign Key to `app.product_templates(id)`, NOT NULL
  - `location_id`: `UUID`, Foreign Key to `app.locations(id)`, NOT NULL
  - `quantity`: `DECIMAL`, NOT NULL (default: 0), `CHECK (quantity >= 0)`
  - **Constraint**: `UNIQUE(organization_id, product_template_id, location_id)`

- **`app.stock_movements`**: An immutable log of all inventory changes.
  - `id`: `UUID`, Primary Key (default: `gen_random_uuid()`)
  - `organization_id`: `UUID`, Foreign Key to `app.organizations(id)`, NOT NULL
  - `product_template_id`: `UUID`, Foreign Key to `app.product_templates(id)`, NOT NULL
  - `movement_type`: `app.movement_type`, NOT NULL
  - `from_location_id`: `UUID`, Foreign Key to `app.locations(id)`, NULL
  - `to_location_id`: `UUID`, Foreign Key to `app.locations(id)`, NULL
  - `delta`: `DECIMAL`, NOT NULL
  - `total`: `DECIMAL`, NOT NULL
  - `user_id`: `UUID`, Foreign Key to `auth.users(id)`

### 2. Relationships Between Tables

- **`organizations`**: The central entity, with one-to-many relationships to:
  - `organization_members`, `invitations`, `warehouses`, `locations`, `product_templates`, `inventory`, and `stock_movements`.
- **`warehouses` to `locations`**: One-to-Many. Deleting a warehouse cascades and deletes all associated locations.
- **`product_templates`**: Has one-to-many relationships with `inventory` and `stock_movements`.
- **`locations`**: Has one-to-many relationships with `inventory` and `stock_movements`.

### 3. Indexes

- **Primary Keys**: Automatically indexed for all tables.
- **Foreign Keys**: All foreign key columns will be indexed to optimize join performance.
- **Custom Indexes**:
  - **`stock_movements`**: Composite index on `(organization_id, created_at DESC)` for fast dashboard queries.
  - **`product_templates`**: Unique composite index on `(organization_id, barcode)` to enforce unique barcodes within an organization.
  - **`inventory`**: Unique composite index on `(organization_id, product_template_id, location_id)` to ensure a product has only one inventory record per location.

### 4. PostgreSQL Policies (Row-Level Security)

- **Strategy**: RLS will be enabled on all tables in the `app` schema to enforce multi-tenancy.
- **Helper Functions**:
  - `auth.active_org_id() returns uuid`: Extracts the currently active organization ID from the user's JWT.
  - `app.get_user_role(org_id uuid) returns app.user_role`: Retrieves the user's role for a given organization.
- **Policy Implementation**: Policies will be created for `SELECT`, `INSERT`, `UPDATE`, and `DELETE` operations on each table. These policies will use the helper functions to ensure users can only access data belonging to their active organization and that their actions are authorized by their role (`owner`, `member`, or `viewer`).

### 5. Design Notes

- **Schema Separation**: Using the `app` schema isolates application data from the `public` schema and extensions, improving organization and security.
- **UUID Primary Keys**: UUIDs are used to prevent ID enumeration and support distributed data generation.
- **`TIMESTAMPTZ`**: Storing all timestamps in UTC with timezone information prevents ambiguity.
- **Immutable Log**: The `stock_movements` table is an append-only audit trail and does not have an `updated_at` column.
- **Inventory Summary**: The `inventory` table is a denormalized summary of stock levels, designed to provide fast reads for current inventory counts without needing to aggregate the entire movement history.
