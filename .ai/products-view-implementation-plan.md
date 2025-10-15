# View Implementation Plan - Products View

## 1. Overview

The Products View is a comprehensive interface for managing product templates within an organization. It provides a paginated list of product cards displaying essential product information (name, barcode, description, low stock threshold) with role-based CRUD operations. The view follows the established card-based design pattern used throughout the application, ensuring consistency with warehouse and location management interfaces.

## 2. View Routing

- **Path**: `/products`
- **Access**: All authenticated users with access to the current organization
- **Role Restrictions**: 
  - `Viewer`: Read-only access
  - `Member` and `Owner`: Full CRUD operations

## 3. Component Structure

```
ProductsPage (Astro page)
├── ProductList (Vue component)
│   ├── ProductCard (Vue component) [repeated for each product]
│   └── PaginationControl (Vue component)
├── CreateProductModal (Vue component)
├── EditProductModal (Vue component)
└── DeleteConfirmationModal (Vue component)
```

## 4. Component Details

### ProductsPage (Astro Page)
- **Component description**: Main page component that orchestrates the products view, handles data fetching, and manages modal states
- **Main elements**: Page layout, product list container, create product button, modals
- **Handled interactions**: Page load, organization switching, modal opening/closing
- **Handled validation**: Organization access validation, role-based permission checks
- **Types**: `ProductTemplateDto`, `PaginatedResponseDto<ProductTemplateDto>`, `CreateProductTemplateRequestDto`
- **Props**: None (page-level component)

### ProductList (Vue Component)
- **Component description**: Container component that displays the list of product cards with loading, empty, and error states
- **Main elements**: Grid container, loading skeletons, empty state message, product cards grid
- **Handled interactions**: Card click navigation, action button clicks, pagination
- **Handled validation**: Empty state handling, loading state management
- **Types**: `ProductTemplateVM[]`, `PaginationDto`, `boolean` (loading)
- **Props**: 
  - `products: ProductTemplateVM[]` - Array of products to display
  - `loading: boolean` - Loading state indicator
  - `pagination: PaginationDto` - Pagination information
  - `canCreate: boolean` - Whether user can create products
  - `canEdit: boolean` - Whether user can edit products
  - `canDelete: boolean` - Whether user can delete products

### ProductCard (Vue Component)
- **Component description**: Individual product card displaying product information with action buttons
- **Main elements**: Product name, barcode, description, low stock threshold, action buttons
- **Handled interactions**: Card click, edit button click, delete button click
- **Handled validation**: Role-based button visibility, action confirmation
- **Types**: `ProductTemplateVM`
- **Props**:
  - `product: ProductTemplateVM` - Product data to display
  - `canEdit: boolean` - Whether edit button should be shown
  - `canDelete: boolean` - Whether delete button should be shown

### CreateProductModal (Vue Component)
- **Component description**: Modal form for creating new product templates
- **Main elements**: Form fields (name, barcode, description, low stock threshold), submit/cancel buttons
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required field validation, barcode uniqueness, numeric validation for threshold
- **Types**: `CreateProductTemplateRequestDto`
- **Props**: None (managed by UI store)

### EditProductModal (Vue Component)
- **Component description**: Modal form for editing existing product templates
- **Main elements**: Pre-filled form fields, submit/cancel buttons
- **Handled interactions**: Form submission, validation, modal close
- **Handled validation**: Required field validation, barcode uniqueness (excluding current product), numeric validation
- **Types**: `ProductTemplateVM`, `CreateProductTemplateRequestDto`
- **Props**: None (managed by UI store)

### DeleteConfirmationModal (Vue Component)
- **Component description**: Confirmation dialog for product deletion
- **Main elements**: Warning message, product details, confirm/cancel buttons
- **Handled interactions**: Confirmation, cancellation
- **Handled validation**: Product existence check, inventory dependency check
- **Types**: `ProductTemplateVM`
- **Props**: None (managed by UI store)

## 5. Types

### ProductTemplateVM (View Model)
```typescript
interface ProductTemplateVM {
  id: string;
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}
```

### ProductPageState (State Management)
```typescript
interface ProductPageState {
  products: ProductTemplateVM[];
  loading: boolean;
  error: string | null;
  pagination: PaginationDto;
  currentPage: number;
  pageSize: number;
  searchQuery: string;
}
```

### ProductTemplateDto (API Response)
```typescript
interface ProductTemplateDto {
  id: string;
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}
```

### CreateProductTemplateRequestDto (API Request)
```typescript
interface CreateProductTemplateRequestDto {
  name: string;
  barcode?: string;
  description?: string;
  lowStockThreshold?: number;
}
```

## 6. State Management

The products view will use a dedicated Pinia store (`useProductStore`) for state management:

- **State**: `products`, `loading`, `error`, `pagination`, `currentPage`, `pageSize`, `searchQuery`
- **Actions**: `fetchProducts()`, `createProduct()`, `updateProduct()`, `deleteProduct()`, `setSearchQuery()`, `setPage()`
- **Getters**: `canCreateProduct`, `canEditProduct`, `canDeleteProduct` (based on user role)
- **Integration**: Connected to organization store for current organization context

## 7. API Integration

### Endpoints Used
- `GET /api/producttemplates?organizationId={id}&page={page}&pageSize={size}&search={query}` - Fetch paginated products
- `GET /api/producttemplates/{id}` - Fetch single product
- `POST /api/producttemplates?organizationId={id}` - Create new product
- `DELETE /api/producttemplates/{id}` - Delete product

### Request/Response Types
- **Get Products**: `PaginatedResponseDto<ProductTemplateDto>`
- **Create Product**: `CreateProductTemplateRequestDto` → `ProductTemplateDto`
- **Delete Product**: No body → `void`

### Error Handling
- 401 Unauthorized: Redirect to login
- 403 Forbidden: Show permission denied message
- 409 Conflict: Show barcode uniqueness error
- 422 Unprocessable Entity: Show validation errors
- 500 Internal Server Error: Show generic error message

## 8. User Interactions

### Primary Interactions
1. **View Products**: Users can browse paginated product list with search functionality
2. **Create Product**: Owners/Members can create new products via modal form
3. **Edit Product**: Owners/Members can edit existing products via modal form
4. **Delete Product**: Owners/Members can delete products (with confirmation)
5. **Search Products**: All users can search products by name or barcode
6. **Navigate Pages**: All users can navigate through paginated results

### Interaction Flow
1. User navigates to `/products`
2. System fetches products for current organization
3. User can search, create, edit, or delete products based on role
4. All actions update the UI immediately with optimistic updates
5. Error states are handled gracefully with user feedback

## 9. Conditions and Validation

### Client-Side Validation
- **Name**: Required, max 100 characters
- **Barcode**: Optional, max 50 characters, must be unique within organization
- **Description**: Optional, max 500 characters
- **Low Stock Threshold**: Optional, must be non-negative number

### API Conditions
- User must be authenticated
- User must have access to the organization
- Barcode must be unique within the organization
- Product cannot be deleted if it has associated inventory

### Role-Based Access
- **Viewer**: Read-only access, no action buttons visible
- **Member/Owner**: Full CRUD access, all action buttons visible

## 10. Error Handling

### Error Scenarios
1. **Network Errors**: Show retry option with user-friendly message
2. **Authentication Errors**: Redirect to login page
3. **Permission Errors**: Show appropriate message based on user role
4. **Validation Errors**: Display field-specific error messages
5. **Barcode Conflicts**: Show specific error for duplicate barcodes
6. **Delete Conflicts**: Show warning if product has inventory

### Error Recovery
- Automatic retry for network errors
- Form validation prevents invalid submissions
- Confirmation dialogs prevent accidental deletions
- Clear error messages guide user actions

## 11. Implementation Steps

1. **Create Product Store**: Implement `useProductStore` with state management
2. **Add API Functions**: Extend `api.ts` with product template endpoints
3. **Create Product Types**: Add product-related types to `dto.ts`
4. **Build ProductCard Component**: Create individual product card component
5. **Build ProductList Component**: Create product list container component
6. **Build Modal Components**: Create create/edit/delete modal components
7. **Create Products Page**: Implement main Astro page component
8. **Add Navigation**: Update sidebar navigation to include products link
9. **Implement Search**: Add search functionality to product list
10. **Add Pagination**: Implement pagination controls
11. **Test Role-Based Access**: Verify permissions work correctly
12. **Add Error Handling**: Implement comprehensive error handling
13. **Style Components**: Apply consistent styling with Tailwind CSS
14. **Test Integration**: End-to-end testing of all functionality
