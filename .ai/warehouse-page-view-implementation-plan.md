# View Implementation Plan: Warehouse Page

## 1. Overview
The Warehouse Page is a comprehensive view for managing warehouses within an organization. It displays a paginated list of warehouses in a card-based layout, allowing users to view warehouse details, create new warehouses, edit existing ones, and delete warehouses (with proper constraints). The view implements role-based access control, showing different actions based on the user's role (Owner, Member, or Viewer).

## 2. View Routing
**Path:** `/warehouses`

## 3. Component Structure
```
WarehousePageView
├── WarehouseHeader (title + create button)
├── WarehouseList
│   ├── WarehouseCard (for each warehouse)
│   └── EmptyState (when no warehouses)
├── PaginationControl
├── CreateWarehouseModal
├── EditWarehouseModal
└── DeleteConfirmationModal
```

## 4. Component Details

### WarehousePageView
- **Component description**: Main container component that orchestrates warehouse management functionality
- **Main elements**: Header section, warehouse list, pagination controls, modals
- **Handled interactions**: Page navigation, modal state management, data fetching
- **Handled validation**: Organization access validation, role-based UI rendering
- **Types**: WarehouseDto[], PaginatedResponseDto<WarehouseDto>, UserVM
- **Props**: None (uses stores for state management)

### WarehouseHeader
- **Component description**: Header section containing page title and create warehouse button
- **Main elements**: Page title, conditional "Create Warehouse" button
- **Handled interactions**: Button click to open create modal
- **Handled validation**: Role-based button visibility (Owner/Member only)
- **Types**: UserVM (for role checking)
- **Props**: None (uses organization store for role)

### WarehouseList
- **Component description**: Container for displaying warehouse cards with loading and empty states
- **Main elements**: Grid layout, WarehouseCard components, EmptyState component
- **Handled interactions**: Card click navigation, loading state management
- **Handled validation**: Empty state when no warehouses exist
- **Types**: WarehouseDto[], boolean (loading)
- **Props**: warehouses: WarehouseDto[], loading: boolean

### WarehouseCard
- **Component description**: Individual warehouse display card with actions
- **Main elements**: Warehouse name, location count, action buttons (Edit/Delete)
- **Handled interactions**: Click to navigate to warehouse detail, edit button, delete button
- **Handled validation**: Role-based action button visibility
- **Types**: WarehouseDto, UserVM (for role checking)
- **Props**: warehouse: WarehouseDto

### CreateWarehouseModal
- **Component description**: Modal for creating new warehouses
- **Main elements**: Dialog container, form inputs, validation messages
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required name field, max length (100 chars), duplicate name checking
- **Types**: CreateWarehouseRequestDto, WarehouseDto
- **Props**: None (uses stores for state)

### EditWarehouseModal
- **Component description**: Modal for editing existing warehouses
- **Main elements**: Dialog container, pre-filled form inputs, validation messages
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required name field, max length (100 chars), duplicate name checking (excluding current)
- **Types**: UpdateWarehouseRequestDto, WarehouseDto
- **Props**: warehouse: WarehouseDto

### DeleteConfirmationModal
- **Component description**: Confirmation modal for warehouse deletion
- **Main elements**: Dialog container, warning message, confirmation input
- **Handled interactions**: Confirmation input, delete action, modal close
- **Handled validation**: Warehouse name confirmation, location existence check
- **Types**: WarehouseDto
- **Props**: warehouse: WarehouseDto

### PaginationControl
- **Component description**: Navigation controls for paginated warehouse list
- **Main elements**: Previous/Next buttons, page numbers, page size selector
- **Handled interactions**: Page navigation, page size changes
- **Handled validation**: Boundary checking (first/last page)
- **Types**: PaginationDto
- **Props**: pagination: PaginationDto, onPageChange: function, onPageSizeChange: function

## 5. Types

### Warehouse DTOs (from backend)
```typescript
interface WarehouseDto {
  id: string;
  name: string;
  organizationId: string;
}

interface WarehouseWithLocationsDto {
  id: string;
  name: string;
  organizationId: string;
  locations: LocationDto[];
}

interface LocationDto {
  id: string;
  name: string;
  description?: string;
}
```

### Request DTOs
```typescript
interface CreateWarehouseRequestDto {
  name: string;
  organizationId: string;
}

interface UpdateWarehouseRequestDto {
  name: string;
}
```

### Pagination DTOs
```typescript
interface PaginationDto {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface PaginatedResponseDto<T> {
  data: T[];
  pagination: PaginationDto;
}

interface PaginationRequestDto {
  page: number;
  pageSize: number;
}
```

### View Models
```typescript
interface WarehouseVM {
  id: string;
  name: string;
  organizationId: string;
  locationCount?: number;
}

interface WarehousePageState {
  warehouses: WarehouseVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}
```

## 6. State Management

The view uses Pinia stores for state management:

### Warehouse Store
```typescript
interface WarehouseStore {
  warehouses: WarehouseVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  
  // Actions
  fetchWarehouses(organizationId: string, page?: number, pageSize?: number): Promise<void>;
  createWarehouse(data: CreateWarehouseRequestDto): Promise<WarehouseDto>;
  updateWarehouse(id: string, data: UpdateWarehouseRequestDto): Promise<WarehouseDto>;
  deleteWarehouse(id: string): Promise<void>;
  setPage(page: number): void;
  setPageSize(pageSize: number): void;
  clearError(): void;
}
```

### UI Store (existing)
Uses existing UI store for modal state management:
- `isCreateWarehouseModalOpen`
- `isEditWarehouseModalOpen`
- `isDeleteWarehouseModalOpen`
- `openCreateWarehouseModal()`
- `closeCreateWarehouseModal()`
- etc.

## 7. API Integration

### Warehouse API Functions
```typescript
// Add to api.ts
export const warehouseApi = {
  async getWarehouses(organizationId: string, page: number = 1, pageSize: number = 10): Promise<PaginatedResponseDto<WarehouseDto>> {
    return fetchWrapper<PaginatedResponseDto<WarehouseDto>>(
      `${API_BASE_URL}/warehouses?organizationId=${organizationId}&page=${page}&pageSize=${pageSize}`
    );
  },

  async getWarehouse(id: string): Promise<WarehouseWithLocationsDto> {
    return fetchWrapper<WarehouseWithLocationsDto>(`${API_BASE_URL}/warehouses/${id}`);
  },

  async createWarehouse(data: CreateWarehouseRequestDto): Promise<WarehouseDto> {
    return fetchWrapper<WarehouseDto>(`${API_BASE_URL}/warehouses`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async updateWarehouse(id: string, data: UpdateWarehouseRequestDto): Promise<WarehouseDto> {
    return fetchWrapper<WarehouseDto>(`${API_BASE_URL}/warehouses/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  async deleteWarehouse(id: string): Promise<void> {
    return fetchWrapper<void>(`${API_BASE_URL}/warehouses/${id}`, {
      method: 'DELETE',
    });
  }
};
```

### Request/Response Types
- **GET /api/warehouses**: Returns `PaginatedResponseDto<WarehouseDto>`
- **GET /api/warehouses/{id}**: Returns `WarehouseWithLocationsDto`
- **POST /api/warehouses**: Accepts `CreateWarehouseRequestDto`, returns `WarehouseDto`
- **PUT /api/warehouses/{id}**: Accepts `UpdateWarehouseRequestDto`, returns `WarehouseDto`
- **DELETE /api/warehouses/{id}**: Returns 204 No Content

## 8. User Interactions

### Primary Interactions
1. **View Warehouses**: User navigates to `/warehouses` and sees paginated list
2. **Create Warehouse**: Owner/Member clicks "Create Warehouse" → modal opens → fills form → submits
3. **Edit Warehouse**: Owner/Member clicks edit button → modal opens with pre-filled data → submits changes
4. **Delete Warehouse**: Owner clicks delete button → confirmation modal → types warehouse name → confirms deletion
5. **Navigate to Detail**: User clicks warehouse card → navigates to `/warehouses/{id}`
6. **Pagination**: User clicks page numbers or changes page size → list updates

### Role-Based Interactions
- **Owner**: Can create, edit, and delete warehouses
- **Member**: Can create and edit warehouses (delete restricted)
- **Viewer**: Can only view warehouses (no action buttons)

## 9. Conditions and Validation

### Frontend Validation
1. **Warehouse Name**: Required, max 100 characters
2. **Duplicate Names**: Check against existing warehouses in organization
3. **Role Access**: Verify user has appropriate role for actions
4. **Organization Access**: Ensure user belongs to current organization
5. **Pagination**: Validate page numbers and page sizes

### API Conditions
1. **Authentication**: All requests require valid JWT token
2. **Organization Membership**: User must be member of warehouse's organization
3. **Role Permissions**: Delete restricted to Owners, create/edit to Members+
4. **Deletion Constraints**: Cannot delete warehouse with existing locations
5. **Name Uniqueness**: Warehouse names must be unique within organization

### UI State Conditions
1. **Loading States**: Show loading indicators during API calls
2. **Error States**: Display error messages for failed operations
3. **Empty States**: Show empty state when no warehouses exist
4. **Modal States**: Manage modal open/close states properly

## 10. Error Handling

### API Error Scenarios
1. **401 Unauthorized**: Token expired/invalid → redirect to login
2. **403 Forbidden**: Insufficient permissions → show error message
3. **404 Not Found**: Warehouse not found → show not found message
4. **409 Conflict**: Cannot delete warehouse with locations → show constraint error
5. **400 Bad Request**: Validation errors → show field-specific errors
6. **500 Server Error**: Database/server errors → show generic error message

### Frontend Error Handling
1. **Network Errors**: Show "Connection failed" message with retry option
2. **Validation Errors**: Display inline validation messages
3. **Permission Errors**: Show appropriate access denied messages
4. **Timeout Errors**: Show timeout message with retry option
5. **Unexpected Errors**: Show generic error with option to refresh

### Error Recovery
1. **Retry Mechanisms**: Allow users to retry failed operations
2. **Fallback States**: Show appropriate fallback content
3. **Error Logging**: Log errors for debugging purposes
4. **User Feedback**: Provide clear, actionable error messages

## 11. Implementation Steps

### Phase 1: Core Infrastructure
1. Create warehouse store (`stores/warehouse.ts`)
2. Add warehouse DTOs to `types/dto.ts`
3. Extend API functions in `lib/api.ts`
4. Create warehouse page route in `pages/warehouses.astro`

### Phase 2: Basic Components
1. Create `WarehouseCard.vue` component
2. Create `WarehouseList.vue` component
3. Create `WarehouseHeader.vue` component
4. Create `PaginationControl.vue` component

### Phase 3: Modal Components
1. Create `CreateWarehouseModal.vue`
2. Create `EditWarehouseModal.vue`
3. Create `DeleteConfirmationModal.vue`
4. Add modal state management to UI store

### Phase 4: Main Page Integration
1. Create `WarehousePageView.vue` main component
2. Integrate all components
3. Implement state management
4. Add error handling

### Phase 5: Testing and Polish
1. Add loading states and error handling
2. Implement role-based access control
3. Add empty states
4. Test all user interactions
5. Add accessibility features
6. Performance optimization

### Phase 6: Integration
1. Add warehouse navigation to sidebar
2. Update organization store integration
3. Test with real API endpoints
4. Add proper error boundaries
5. Final testing and bug fixes
