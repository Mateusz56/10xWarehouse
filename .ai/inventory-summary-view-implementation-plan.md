# View Implementation Plan: Inventory Summary

## 1. Overview
The Inventory Summary view is the primary interface for viewing, filtering, and managing inventory levels across all warehouses and locations within the active organization. This view displays a paginated list of inventory items with filtering capabilities by product, location, and low stock status. Users can initiate stock operations (Add, Withdraw, Move, Reconcile) directly from this view, making it the central hub for inventory management operations.

## 2. View Routing
- **Path**: `/inventory`
- **Component**: `InventorySummaryView.vue`
- **Layout**: Uses `MainLayout.astro` with sidebar navigation

## 3. Component Structure
```
InventorySummaryView
├── FilterBar
│   ├── ProductFilter
│   ├── LocationFilter
│   └── LowStockToggle
├── ActionBar
│   ├── AddStockButton (role-restricted)
│   └── RefreshButton
├── InventoryGrid
│   ├── InventoryCard (for each item)
│   └── PaginationControl
└── EmptyState (when no inventory exists)
```

## 4. Component Details

### InventorySummaryView
- **Component description**: Main view component that manages inventory display, filtering, pagination, and stock operations
- **Main elements**: 
  - Page header with title "Inventory Summary"
  - FilterBar for filtering options
  - ActionBar with operation buttons
  - InventoryGrid for displaying inventory items
  - EmptyState when no data
- **Handled interactions**: 
  - Page load and data fetching
  - Filter application and clearing
  - Pagination navigation
  - Stock operation initiation
- **Handled validation**: 
  - Organization access validation
  - Role-based action visibility
  - Filter parameter validation
- **Types**: 
  - `InventorySummaryState` (view state)
  - `InventorySummaryDto` (API response)
  - `PaginatedResponseDto<InventorySummaryDto>` (API response)
- **Props**: None (top-level view)

### FilterBar
- **Component description**: Filtering component with dropdowns and toggles for filtering inventory data
- **Main elements**: 
  - Product dropdown (searchable)
  - Location dropdown (searchable)
  - Low stock toggle switch
  - Clear filters button
- **Handled interactions**: 
  - Filter selection changes
  - Clear filters action
  - Search input for dropdowns
- **Handled validation**: 
  - Filter value validation
  - Search input validation
- **Types**: 
  - `InventoryFilters` (filter state)
  - `ProductSummaryDto` (product options)
  - `LocationSummaryDto` (location options)
- **Props**: 
  - `filters: InventoryFilters`
  - `onFiltersChange: (filters: InventoryFilters) => void`
  - `onClearFilters: () => void`

### ProductFilter
- **Component description**: Searchable dropdown for selecting products to filter by
- **Main elements**: 
  - Dropdown trigger button
  - Search input field
  - Product option list
  - Clear selection button
- **Handled interactions**: 
  - Dropdown open/close
  - Search input typing
  - Product selection
  - Clear selection
- **Handled validation**: 
  - Search input length validation
  - Product selection validation
- **Types**: 
  - `ProductSummaryDto` (product data)
- **Props**: 
  - `products: ProductSummaryDto[]`
  - `selectedProductId?: string`
  - `onProductSelect: (productId: string | null) => void`
  - `loading: boolean`

### LocationFilter
- **Component description**: Searchable dropdown for selecting locations to filter by
- **Main elements**: 
  - Dropdown trigger button
  - Search input field
  - Location option list
  - Clear selection button
- **Handled interactions**: 
  - Dropdown open/close
  - Search input typing
  - Location selection
  - Clear selection
- **Handled validation**: 
  - Search input length validation
  - Location selection validation
- **Types**: 
  - `LocationSummaryDto` (location data)
- **Props**: 
  - `locations: LocationSummaryDto[]`
  - `selectedLocationId?: string`
  - `onLocationSelect: (locationId: string | null) => void`
  - `loading: boolean`

### LowStockToggle
- **Component description**: Toggle switch for filtering low stock items
- **Main elements**: 
  - Toggle switch input
  - Label text
  - Optional count indicator
- **Handled interactions**: 
  - Toggle switch change
- **Handled validation**: 
  - Toggle state validation
- **Types**: 
  - `boolean` (toggle state)
- **Props**: 
  - `enabled: boolean`
  - `onToggle: (enabled: boolean) => void`
  - `lowStockCount?: number`

### ActionBar
- **Component description**: Action bar containing operation buttons and controls
- **Main elements**: 
  - Add Stock button (role-restricted)
  - Refresh button
  - View options (grid/list toggle if implemented)
- **Handled interactions**: 
  - Button clicks
  - Role-based visibility
- **Handled validation**: 
  - Role permission validation
- **Types**: 
  - `UserRole` (for role checking)
- **Props**: 
  - `userRole: UserRole`
  - `onAddStock: () => void`
  - `onRefresh: () => void`

### InventoryGrid
- **Component description**: Grid container for displaying inventory cards with pagination
- **Main elements**: 
  - Grid layout container
  - InventoryCard components
  - PaginationControl
  - Loading state indicator
- **Handled interactions**: 
  - Pagination navigation
  - Card interactions (delegated to cards)
- **Handled validation**: 
  - Data presence validation
  - Pagination validation
- **Types**: 
  - `InventorySummaryDto[]` (inventory data)
  - `PaginationDto` (pagination info)
- **Props**: 
  - `inventory: InventorySummaryDto[]`
  - `pagination: PaginationDto`
  - `loading: boolean`
  - `onPageChange: (page: number) => void`

### InventoryCard
- **Component description**: Individual card component displaying inventory item with action buttons
- **Main elements**: 
  - Product name and details
  - Location information
  - Current quantity with low stock indicator
  - Action buttons (Move, Withdraw, Reconcile)
- **Handled interactions**: 
  - Action button clicks
  - Card hover effects
- **Handled validation**: 
  - Role-based button visibility
  - Action availability validation
- **Types**: 
  - `InventorySummaryDto` (inventory data)
  - `UserRole` (for role checking)
- **Props**: 
  - `inventory: InventorySummaryDto`
  - `userRole: UserRole`
  - `onMove: (inventory: InventorySummaryDto) => void`
  - `onWithdraw: (inventory: InventorySummaryDto) => void`
  - `onReconcile: (inventory: InventorySummaryDto) => void`

### PaginationControl
- **Component description**: Reusable pagination component for navigating through pages of data
- **Main elements**: 
  - Previous/Next buttons
  - Page number buttons
  - Page size selector
  - Total count display
- **Handled interactions**: 
  - Page navigation
  - Page size changes
- **Handled validation**: 
  - Page number bounds validation
  - Page size validation
- **Types**: 
  - `PaginationDto` (pagination info)
- **Props**: 
  - `pagination: PaginationDto`
  - `onPageChange: (page: number) => void`
  - `onPageSizeChange: (pageSize: number) => void`

### EmptyState
- **Component description**: Placeholder component shown when no inventory items exist
- **Main elements**: 
  - Icon (package or inventory icon)
  - Message "No inventory items found"
  - Call-to-action button "Add Stock"
- **Handled interactions**: 
  - Call-to-action button click
- **Handled validation**: 
  - Role-based button visibility
- **Types**: 
  - `UserRole` (for role checking)
- **Props**: 
  - `message: string`
  - `actionText: string`
  - `onAction: () => void`
  - `userRole: UserRole`

## 5. Types

### InventorySummaryDto
```typescript
interface InventorySummaryDto {
  product: ProductSummaryDto;
  location: LocationSummaryDto;
  quantity: number;
}
```

### ProductSummaryDto
```typescript
interface ProductSummaryDto {
  id: string;
  name: string;
}
```

### LocationSummaryDto
```typescript
interface LocationSummaryDto {
  id: string;
  name: string;
}
```

### InventorySummaryState
```typescript
interface InventorySummaryState {
  inventory: InventorySummaryDto[];
  products: ProductSummaryDto[];
  locations: LocationSummaryDto[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  filters: InventoryFilters;
}
```

### InventoryFilters
```typescript
interface InventoryFilters {
  productId?: string;
  locationId?: string;
  lowStock: boolean;
}
```

### UserRole
```typescript
type UserRole = 'Owner' | 'Member' | 'Viewer';
```

### StockOperationType
```typescript
type StockOperationType = 'Add' | 'Withdraw' | 'Move' | 'Reconcile';
```

### InventoryCardProps
```typescript
interface InventoryCardProps {
  inventory: InventorySummaryDto;
  userRole: UserRole;
  onMove: (inventory: InventorySummaryDto) => void;
  onWithdraw: (inventory: InventorySummaryDto) => void;
  onReconcile: (inventory: InventorySummaryDto) => void;
}
```

## 6. State Management
The view uses a dedicated Pinia store `useInventoryStore` for managing inventory data:

- **State properties**:
  - `inventory: InventorySummaryDto[]` - Array of inventory items
  - `products: ProductSummaryDto[]` - Available products for filtering
  - `locations: LocationSummaryDto[]` - Available locations for filtering
  - `loading: boolean` - Loading state
  - `error: string | null` - Error message
  - `pagination: PaginationDto` - Pagination information
  - `currentPage: number` - Current page number
  - `pageSize: number` - Items per page
  - `filters: InventoryFilters` - Current filter state

- **Actions**:
  - `fetchInventory(organizationId: string, page: number, pageSize: number, filters: InventoryFilters)` - Fetch inventory
  - `fetchProducts(organizationId: string)` - Fetch products for filtering
  - `fetchLocations(organizationId: string)` - Fetch locations for filtering
  - `setPage(page: number)` - Change current page
  - `setPageSize(pageSize: number)` - Change page size
  - `setFilters(filters: InventoryFilters)` - Update filters
  - `clearFilters()` - Reset filters to default
  - `clearError()` - Clear error state

- **Getters**:
  - `hasInventory: boolean` - Check if inventory exists
  - `totalPages: number` - Calculate total pages
  - `isFirstPage: boolean` - Check if on first page
  - `isLastPage: boolean` - Check if on last page
  - `filteredInventory: InventorySummaryDto[]` - Get filtered inventory
  - `lowStockItems: InventorySummaryDto[]` - Get low stock items

## 7. API Integration
The view integrates with the existing `GET /api/inventory` endpoint:

- **Request**: 
  - Method: GET
  - URL: `/api/inventory?organizationId={id}&page={page}&pageSize={pageSize}&locationId={locationId}&productTemplateId={productId}&lowStock={lowStock}`
  - Headers: Authorization Bearer token, X-Organization-Id

- **Response**: 
  - Type: `PaginatedResponseDto<InventorySummaryDto>`
  - Contains paginated array of inventory items with metadata

- **Additional endpoints**:
  - `GET /api/producttemplates` - For product filter options
  - `GET /api/locations` - For location filter options (through warehouses)

- **Error handling**:
  - 401: Redirect to login
  - 403: Show access denied message
  - 500: Show generic error message

## 8. User Interactions
- **Page load**: Automatically fetch first page of inventory with default filters
- **Filtering**: 
  - Select product from dropdown to filter by product
  - Select location from dropdown to filter by location
  - Toggle low stock switch to show only low stock items
  - Clear all filters to reset view
- **Pagination**: Click page numbers or prev/next buttons to navigate
- **Page size change**: Select different page size from dropdown
- **Stock operations**: 
  - Click "Add Stock" button to open add stock modal
  - Click "Move" button on inventory card to open move stock modal
  - Click "Withdraw" button on inventory card to open withdraw stock modal
  - Click "Reconcile" button on inventory card to open reconcile stock modal
- **Refresh**: Manual refresh button to reload data
- **Card interactions**: Hover effects and button states

## 9. Conditions and Validation
- **Organization access**: User must be member of the organization (any role)
- **Role-based actions**: 
  - Add Stock button: Visible only to Owner and Member roles
  - Move/Withdraw/Reconcile buttons: Visible only to Owner and Member roles
  - Viewer role: Read-only access
- **Data validation**: 
  - Quantity must be non-negative number
  - Product and location must exist
  - Filter values must be valid UUIDs
- **Pagination validation**:
  - Page number must be positive integer
  - Page size must be between 1-100
- **Filter validation**:
  - Product ID must be valid UUID if provided
  - Location ID must be valid UUID if provided
  - Low stock toggle must be boolean

## 10. Error Handling
- **Network errors**: Display toast notification and retry button
- **Authentication errors**: Redirect to login page
- **Authorization errors**: Show access denied message
- **Data errors**: Show error message in place of inventory grid
- **Empty state**: Show empty state component with helpful message
- **Loading states**: Show loading spinner during data fetch
- **Filter errors**: Show validation messages for invalid filter inputs
- **Action errors**: Show error messages for failed stock operations

## 11. Implementation Steps
1. **Create InventoryStore**: Set up Pinia store for state management
2. **Create FilterBar component**: Build filtering component with dropdowns and toggle
3. **Create ProductFilter component**: Build searchable product dropdown
4. **Create LocationFilter component**: Build searchable location dropdown
5. **Create LowStockToggle component**: Build toggle switch component
6. **Create ActionBar component**: Build action bar with role-based buttons
7. **Create InventoryCard component**: Build individual inventory card
8. **Create InventoryGrid component**: Build grid container with pagination
9. **Create EmptyState component**: Build empty state component
10. **Create InventorySummaryView**: Build main view component
11. **Add API integration**: Add inventory API calls to api.ts
12. **Add types**: Add TypeScript interfaces to types/dto.ts
13. **Add routing**: Add route to Astro router
14. **Add navigation**: Add link to sidebar navigation
15. **Add styling**: Apply Tailwind CSS styling
16. **Add error handling**: Implement comprehensive error handling
17. **Add loading states**: Implement loading indicators
18. **Add role-based visibility**: Implement role-based UI elements
19. **Test functionality**: Test filtering, pagination, and stock operations
20. **Test role-based access**: Test different user roles and permissions
