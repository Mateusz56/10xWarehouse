# View Implementation Plan: Stock Movement Log

## 1. Overview
The Stock Movement Log view provides a comprehensive, read-only audit trail of all inventory changes within the active organization. This view displays a paginated data grid showing all stock movements with detailed information including date, product, movement type, quantity changes, and affected locations. The view serves as a complete audit log for inventory operations and is accessible to all authorized users regardless of their role.

## 2. View Routing
- **Path**: `/movements`
- **Component**: `StockMovementLogView.vue`
- **Layout**: Uses `MainLayout.astro` with sidebar navigation

## 3. Component Structure
```
StockMovementLogView
├── DataGrid (main content area)
│   ├── StockMovementRow (for each movement)
│   └── PaginationControl
└── EmptyState (when no movements exist)
```

## 4. Component Details

### StockMovementLogView
- **Component description**: Main view component that manages the stock movement log display, data fetching, and pagination
- **Main elements**: 
  - Page header with title "Stock Movement Log"
  - DataGrid component for displaying movements
  - PaginationControl for navigation
  - EmptyState component when no data
- **Handled interactions**: 
  - Page load and data fetching
  - Pagination navigation
  - Filter application (if implemented)
- **Handled validation**: 
  - Organization access validation
  - Pagination parameter validation
- **Types**: 
  - `StockMovementLogState` (view state)
  - `StockMovementDto` (API response)
  - `PaginatedResponseDto<StockMovementDto>` (API response)
- **Props**: None (top-level view)

### DataGrid
- **Component description**: Reusable data grid component for displaying tabular data with sorting and column headers
- **Main elements**: 
  - Table with headers: Date, Product, Movement Type, Quantity Change, Location(s), Quantity Before, Quantity After
  - Scrollable tbody for movement rows
  - Loading state indicator
- **Handled interactions**: 
  - Column sorting (if implemented)
  - Row hover effects
- **Handled validation**: 
  - Data format validation for display
- **Types**: 
  - `StockMovementDto[]` (data array)
  - `DataGridColumn` (column configuration)
- **Props**: 
  - `data: StockMovementDto[]`
  - `columns: DataGridColumn[]`
  - `loading: boolean`

### StockMovementRow
- **Component description**: Individual row component for displaying a single stock movement with formatted data
- **Main elements**: 
  - Table cells for each data field
  - Formatted date display
  - Movement type badge with color coding
  - Quantity change with +/- indicators
  - Location names (from/to for moves)
- **Handled interactions**: 
  - Row click (if drill-down is needed)
- **Handled validation**: 
  - Data presence validation
  - Format validation for display
- **Types**: 
  - `StockMovementDto` (movement data)
  - `ProductSummaryDto` (product info)
  - `LocationSummaryDto` (location info)
- **Props**: 
  - `movement: StockMovementDto`
  - `product: ProductSummaryDto`
  - `fromLocation?: LocationSummaryDto`
  - `toLocation?: LocationSummaryDto`

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
- **Component description**: Placeholder component shown when no stock movements exist
- **Main elements**: 
  - Icon (document or list icon)
  - Message "No stock movements found"
  - Optional call-to-action button
- **Handled interactions**: 
  - Call-to-action button click (if provided)
- **Handled validation**: None
- **Types**: None specific
- **Props**: 
  - `message: string`
  - `actionText?: string`
  - `onAction?: () => void`

## 5. Types

### StockMovementDto
```typescript
interface StockMovementDto {
  id: string;
  productTemplateId: string;
  movementType: 'Add' | 'Withdraw' | 'Move' | 'Reconcile';
  delta: number;
  fromLocationId?: string;
  toLocationId?: string;
  createdAt: string; // ISO date string
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

### StockMovementLogState
```typescript
interface StockMovementLogState {
  movements: StockMovementDto[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}
```

### DataGridColumn
```typescript
interface DataGridColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
}
```

### MovementTypeDisplay
```typescript
interface MovementTypeDisplay {
  label: string;
  color: string;
  icon: string;
}
```

## 6. State Management
The view uses a dedicated Pinia store `useStockMovementStore` for managing stock movement data:

- **State properties**:
  - `movements: StockMovementDto[]` - Array of stock movements
  - `loading: boolean` - Loading state
  - `error: string | null` - Error message
  - `pagination: PaginationDto` - Pagination information
  - `currentPage: number` - Current page number
  - `pageSize: number` - Items per page

- **Actions**:
  - `fetchMovements(organizationId: string, page: number, pageSize: number)` - Fetch movements
  - `setPage(page: number)` - Change current page
  - `setPageSize(pageSize: number)` - Change page size
  - `clearError()` - Clear error state

- **Getters**:
  - `hasMovements: boolean` - Check if movements exist
  - `totalPages: number` - Calculate total pages
  - `isFirstPage: boolean` - Check if on first page
  - `isLastPage: boolean` - Check if on last page

## 7. API Integration
The view integrates with the existing `GET /api/stock-movements` endpoint:

- **Request**: 
  - Method: GET
  - URL: `/api/stock-movements?organizationId={id}&page={page}&pageSize={pageSize}`
  - Headers: Authorization Bearer token, X-Organization-Id

- **Response**: 
  - Type: `PaginatedResponseDto<StockMovementDto>`
  - Contains paginated array of stock movements with metadata

- **Error handling**:
  - 401: Redirect to login
  - 403: Show access denied message
  - 500: Show generic error message

## 8. User Interactions
- **Page load**: Automatically fetch first page of movements
- **Pagination**: Click page numbers or prev/next buttons to navigate
- **Page size change**: Select different page size from dropdown
- **Row interaction**: Hover effects for better UX (no click actions for read-only view)
- **Refresh**: Manual refresh button to reload data

## 9. Conditions and Validation
- **Organization access**: User must be member of the organization (any role)
- **Data validation**: 
  - Movement type must be valid enum value
  - Delta must be non-zero number
  - Dates must be valid ISO strings
- **Pagination validation**:
  - Page number must be positive integer
  - Page size must be between 1-100
- **Display validation**:
  - Handle missing product/location names gracefully
  - Format dates consistently
  - Show appropriate movement type labels

## 10. Error Handling
- **Network errors**: Display toast notification and retry button
- **Authentication errors**: Redirect to login page
- **Authorization errors**: Show access denied message
- **Data errors**: Show error message in place of data grid
- **Empty state**: Show empty state component with helpful message
- **Loading states**: Show loading spinner during data fetch

## 11. Implementation Steps
1. **Create StockMovementStore**: Set up Pinia store for state management
2. **Create DataGrid component**: Build reusable data grid component
3. **Create StockMovementRow component**: Build individual row component
4. **Create EmptyState component**: Build empty state component
5. **Create StockMovementLogView**: Build main view component
6. **Add API integration**: Add stock movement API calls to api.ts
7. **Add types**: Add TypeScript interfaces to types/dto.ts
8. **Add routing**: Add route to Astro router
9. **Add navigation**: Add link to sidebar navigation
10. **Add styling**: Apply Tailwind CSS styling
11. **Add error handling**: Implement comprehensive error handling
12. **Add loading states**: Implement loading indicators
13. **Test functionality**: Test pagination, error states, and empty states
