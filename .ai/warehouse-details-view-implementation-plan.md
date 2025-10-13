# View Implementation Plan: Warehouse Details

## 1. Overview
The Warehouse Details view displays comprehensive information about a specific warehouse and manages its associated locations. This view allows users to view warehouse information, create new locations, edit existing locations, and delete locations (with proper role-based restrictions). The view implements role-based access control, showing different actions based on the user's role (Owner, Member, or Viewer). Users can also edit warehouse details and delete the entire warehouse (with proper constraints).

## 2. View Routing
**Path:** `/warehouses/:id`

## 3. Component Structure
```
WarehouseDetailsView
├── WarehouseDetailsHeader (warehouse info + edit/delete buttons)
├── WarehouseLocationsSection
│   ├── LocationsHeader (title + create location button)
│   ├── LocationList
│   │   ├── LocationCard (for each location)
│   │   └── EmptyState (when no locations)
│   └── PaginationControl (for locations)
├── EditWarehouseModal
├── DeleteConfirmationModal (for warehouse)
├── CreateLocationModal
├── EditLocationModal
└── DeleteConfirmationModal (for location)
```

## 4. Component Details

### WarehouseDetailsView
- **Component description**: Main container component that orchestrates warehouse details and location management functionality
- **Main elements**: Header section, locations section, modals for warehouse and location operations
- **Handled interactions**: Page navigation, modal state management, data fetching, breadcrumb navigation
- **Handled validation**: Warehouse access validation, role-based UI rendering, warehouse existence validation
- **Types**: WarehouseWithLocationsDto, LocationDto[], UserVM, PaginationDto
- **Props**: warehouseId: string (from route params)

### WarehouseDetailsHeader
- **Component description**: Header section containing warehouse information and action buttons
- **Main elements**: Warehouse name, description, location count, edit/delete warehouse buttons, breadcrumb navigation
- **Handled interactions**: Edit warehouse button, delete warehouse button, breadcrumb navigation
- **Handled validation**: Role-based button visibility (Edit: Member+, Delete: Owner only)
- **Types**: WarehouseWithLocationsDto, UserVM (for role checking)
- **Props**: warehouse: WarehouseWithLocationsDto

### WarehouseLocationsSection
- **Component description**: Container for managing warehouse locations with pagination
- **Main elements**: Section header, location list, pagination controls
- **Handled interactions**: Location management, pagination navigation
- **Handled validation**: Empty state when no locations exist, pagination boundary checking
- **Types**: LocationDto[], PaginationDto, boolean (loading)
- **Props**: locations: LocationDto[], pagination: PaginationDto, loading: boolean

### LocationCard
- **Component description**: Individual location display card with actions
- **Main elements**: Location name, description, action buttons (Edit/Delete)
- **Handled interactions**: Edit button, delete button
- **Handled validation**: Role-based action button visibility
- **Types**: LocationDto, UserVM (for role checking)
- **Props**: location: LocationDto

### CreateLocationModal
- **Component description**: Modal for creating new locations within the warehouse
- **Main elements**: Dialog container, form inputs (name, description), validation messages
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required name field, max length (100 chars), duplicate name checking within warehouse
- **Types**: CreateLocationRequestDto, LocationDto
- **Props**: warehouseId: string

### EditLocationModal
- **Component description**: Modal for editing existing locations
- **Main elements**: Dialog container, pre-filled form inputs, validation messages
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required name field, max length (100 chars), duplicate name checking (excluding current)
- **Types**: UpdateLocationRequestDto, LocationDto
- **Props**: location: LocationDto, warehouseId: string

### EditWarehouseModal
- **Component description**: Modal for editing warehouse details
- **Main elements**: Dialog container, pre-filled form inputs, validation messages
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required name field, max length (100 chars), duplicate name checking (excluding current)
- **Types**: UpdateWarehouseRequestDto, WarehouseDto
- **Props**: warehouse: WarehouseWithLocationsDto

### DeleteConfirmationModal (Warehouse)
- **Component description**: Confirmation modal for warehouse deletion
- **Main elements**: Dialog container, warning message, confirmation input, location existence check
- **Handled interactions**: Confirmation input, delete action, modal close
- **Handled validation**: Warehouse name confirmation, location existence check (cannot delete if locations exist)
- **Types**: WarehouseWithLocationsDto
- **Props**: warehouse: WarehouseWithLocationsDto

### DeleteConfirmationModal (Location)
- **Component description**: Confirmation modal for location deletion
- **Main elements**: Dialog container, warning message, confirmation input
- **Handled interactions**: Confirmation input, delete action, modal close
- **Handled validation**: Location name confirmation
- **Types**: LocationDto
- **Props**: location: LocationDto

### PaginationControl
- **Component description**: Navigation controls for paginated location list
- **Main elements**: Previous/Next buttons, page numbers, page size selector
- **Handled interactions**: Page navigation, page size changes
- **Handled validation**: Boundary checking (first/last page)
- **Types**: PaginationDto
- **Props**: pagination: PaginationDto, onPageChange: function, onPageSizeChange: function

## 5. Types

### Location DTOs (new)
```typescript
interface CreateLocationRequestDto {
  name: string;
  description?: string;
  warehouseId: string;
}

interface UpdateLocationRequestDto {
  name: string;
  description?: string;
}

interface LocationVM {
  id: string;
  name: string;
  description?: string;
  warehouseId: string;
}
```

### Warehouse Detail View Models (extend existing)
```typescript
interface WarehouseDetailsState {
  warehouse: WarehouseWithLocationsDto | null;
  locations: LocationVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
}

interface WarehouseDetailsVM {
  warehouse: WarehouseWithLocationsDto;
  locations: LocationVM[];
  pagination: PaginationDto;
}
```

### Extended API Types
```typescript
// Add to existing warehouseApi
interface LocationApi {
  async getLocations(warehouseId: string, page: number = 1, pageSize: number = 10): Promise<PaginatedResponseDto<LocationDto>>;
  async createLocation(data: CreateLocationRequestDto): Promise<LocationDto>;
  async updateLocation(id: string, data: UpdateLocationRequestDto): Promise<LocationDto>;
  async deleteLocation(id: string): Promise<void>;
}
```

## 6. State Management

### Warehouse Details Store (new)
```typescript
interface WarehouseDetailsStore {
  warehouse: WarehouseWithLocationsDto | null;
  locations: LocationVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  
  // Computed
  hasLocations: ComputedRef<boolean>;
  isEmpty: ComputedRef<boolean>;
  canCreateLocation: ComputedRef<boolean>;
  canEditLocation: ComputedRef<boolean>;
  canDeleteLocation: ComputedRef<boolean>;
  canEditWarehouse: ComputedRef<boolean>;
  canDeleteWarehouse: ComputedRef<boolean>;
  
  // Actions
  fetchWarehouseDetails(warehouseId: string): Promise<void>;
  fetchLocations(warehouseId: string, page?: number, pageSize?: number): Promise<void>;
  createLocation(data: CreateLocationRequestDto): Promise<LocationDto>;
  updateLocation(id: string, data: UpdateLocationRequestDto): Promise<LocationDto>;
  deleteLocation(id: string): Promise<void>;
  updateWarehouse(id: string, data: UpdateWarehouseRequestDto): Promise<WarehouseDto>;
  deleteWarehouse(id: string): Promise<void>;
  setPage(page: number): void;
  setPageSize(pageSize: number): void;
  clearError(): void;
  clearWarehouseDetails(): void;
}
```

### UI Store Extensions
Extend existing UI store for modal state management:
- `isEditWarehouseModalOpen`
- `isDeleteWarehouseModalOpen`
- `isCreateLocationModalOpen`
- `isEditLocationModalOpen`
- `isDeleteLocationModalOpen`
- `selectedWarehouse`
- `selectedLocation`
- Modal open/close functions for each modal type

## 7. API Integration

### Location API Functions (new)
```typescript
// Add to api.ts
export const locationApi = {
  async getLocations(warehouseId: string, page: number = 1, pageSize: number = 10): Promise<PaginatedResponseDto<LocationDto>> {
    return fetchWrapper<PaginatedResponseDto<LocationDto>>(
      `${API_BASE_URL}/locations?warehouseId=${warehouseId}&page=${page}&pageSize=${pageSize}`
    );
  },

  async createLocation(data: CreateLocationRequestDto): Promise<LocationDto> {
    return fetchWrapper<LocationDto>(`${API_BASE_URL}/locations`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  async updateLocation(id: string, data: UpdateLocationRequestDto): Promise<LocationDto> {
    return fetchWrapper<LocationDto>(`${API_BASE_URL}/locations/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  async deleteLocation(id: string): Promise<void> {
    return fetchWrapper<void>(`${API_BASE_URL}/locations/${id}`, {
      method: 'DELETE',
    });
  }
};
```

### Request/Response Types
- **GET /api/warehouses/{id}**: Returns `WarehouseWithLocationsDto` (existing)
- **GET /api/locations**: Returns `PaginatedResponseDto<LocationDto>` (new)
- **POST /api/locations**: Accepts `CreateLocationRequestDto`, returns `LocationDto` (new)
- **PUT /api/locations/{id}**: Accepts `UpdateLocationRequestDto`, returns `LocationDto` (new)
- **DELETE /api/locations/{id}**: Returns 204 No Content (new)
- **PUT /api/warehouses/{id}**: Accepts `UpdateWarehouseRequestDto`, returns `WarehouseDto` (existing)
- **DELETE /api/warehouses/{id}**: Returns 204 No Content (existing)

## 8. User Interactions

### Primary Interactions
1. **View Warehouse Details**: User navigates to `/warehouses/{id}` and sees warehouse info and locations
2. **Create Location**: Member/Owner clicks "Create Location" → modal opens → fills form → submits
3. **Edit Location**: Member/Owner clicks edit button → modal opens with pre-filled data → submits changes
4. **Delete Location**: Member/Owner clicks delete button → confirmation modal → confirms deletion
5. **Edit Warehouse**: Member/Owner clicks edit warehouse button → modal opens → submits changes
6. **Delete Warehouse**: Owner clicks delete warehouse button → confirmation modal → types warehouse name → confirms deletion
7. **Location Pagination**: User clicks page numbers or changes page size → location list updates
8. **Breadcrumb Navigation**: User clicks breadcrumb to return to warehouse list

### Role-Based Interactions
- **Owner**: Can create, edit, and delete locations; can edit and delete warehouse
- **Member**: Can create, edit, and delete locations; can edit warehouse (delete restricted)
- **Viewer**: Can only view warehouse and locations (no action buttons)

### Navigation Flow
1. User clicks warehouse card in `/warehouses` → navigates to `/warehouses/{id}`
2. User can navigate back via breadcrumb or sidebar navigation
3. After warehouse deletion, user is redirected to `/warehouses`

## 9. Conditions and Validation

### Frontend Validation
1. **Location Name**: Required, max 100 characters
2. **Location Description**: Optional, max 500 characters
3. **Duplicate Location Names**: Check against existing locations in same warehouse
4. **Warehouse Name**: Required, max 100 characters (for warehouse editing)
5. **Role Access**: Verify user has appropriate role for actions
6. **Organization Access**: Ensure user belongs to warehouse's organization
7. **Pagination**: Validate page numbers and page sizes
8. **Warehouse Existence**: Verify warehouse exists and user has access

### API Conditions
1. **Authentication**: All requests require valid JWT token
2. **Organization Membership**: User must be member of warehouse's organization
3. **Role Permissions**: Delete warehouse restricted to Owners, create/edit to Members+
4. **Deletion Constraints**: Cannot delete warehouse with existing locations
5. **Name Uniqueness**: Location names must be unique within warehouse
6. **Warehouse Access**: User must have access to warehouse to manage locations

### UI State Conditions
1. **Loading States**: Show loading indicators during API calls
2. **Error States**: Display error messages for failed operations
3. **Empty States**: Show empty state when no locations exist
4. **Modal States**: Manage modal open/close states properly
5. **Navigation States**: Handle warehouse not found scenarios
6. **Permission States**: Show/hide action buttons based on user role

## 10. Error Handling

### API Error Scenarios
1. **401 Unauthorized**: Token expired/invalid → redirect to login
2. **403 Forbidden**: Insufficient permissions → show error message
3. **404 Not Found**: Warehouse/location not found → show not found message
4. **409 Conflict**: Cannot delete warehouse with locations → show constraint error
5. **400 Bad Request**: Validation errors → show field-specific errors
6. **500 Server Error**: Database/server errors → show generic error message

### Frontend Error Handling
1. **Network Errors**: Show "Connection failed" message with retry option
2. **Validation Errors**: Display inline validation messages
3. **Permission Errors**: Show appropriate access denied messages
4. **Timeout Errors**: Show timeout message with retry option
5. **Unexpected Errors**: Show generic error with option to refresh
6. **Warehouse Not Found**: Show 404 page with navigation back to warehouse list

### Error Recovery
1. **Retry Mechanisms**: Allow users to retry failed operations
2. **Fallback States**: Show appropriate fallback content
3. **Error Logging**: Log errors for debugging purposes
4. **User Feedback**: Provide clear, actionable error messages
5. **Navigation Recovery**: Handle navigation errors gracefully

## 11. Implementation Steps

### Phase 1: Core Infrastructure
1. Create warehouse details store (`stores/warehouseDetails.ts`)
2. Add location DTOs to `types/dto.ts`
3. Extend API functions in `lib/api.ts` for location operations
4. Create warehouse details page route in `pages/warehouses/[id].astro`
5. Extend UI store for warehouse details modal states

### Phase 2: Basic Components
1. Create `LocationCard.vue` component
2. Create `LocationList.vue` component
3. Create `WarehouseDetailsHeader.vue` component
4. Create `WarehouseLocationsSection.vue` component
5. Extend `PaginationControl.vue` for location pagination

### Phase 3: Modal Components
1. Create `CreateLocationModal.vue`
2. Create `EditLocationModal.vue`
3. Create `DeleteConfirmationModal.vue` (reusable for both warehouse and location)
4. Extend `EditWarehouseModal.vue` for warehouse details context
5. Add modal state management to UI store

### Phase 4: Main Page Integration
1. Create `WarehouseDetailsView.vue` main component
2. Integrate all components
3. Implement state management
4. Add error handling and loading states
5. Implement breadcrumb navigation

### Phase 5: Testing and Polish
1. Add loading states and error handling
2. Implement role-based access control
3. Add empty states for locations
4. Test all user interactions
5. Add accessibility features
6. Performance optimization

### Phase 6: Integration
1. Update warehouse card navigation to detail view
2. Update organization store integration
3. Test with real API endpoints
4. Add proper error boundaries
5. Final testing and bug fixes
6. Add breadcrumb component integration
