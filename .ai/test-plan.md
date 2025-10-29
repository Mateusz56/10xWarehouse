# Comprehensive Test Plan for 10xWarehouse

## 1. Introduction and Testing Objectives

### 1.1 Purpose

This document outlines a comprehensive testing strategy for the 10xWarehouse application, a multi-tenant warehouse management system built with .NET 9.0 backend and Astro 5 + Vue 3 frontend.

### 1.2 Testing Objectives

- Ensure data integrity and accuracy of inventory management operations
- Validate multi-tenant data isolation and security
- Verify role-based access control (RBAC) across all endpoints
- Validate business logic for stock movements (Add, Withdraw, Move, Reconcile)
- Ensure proper error handling and user feedback
- Verify API contract compliance and interoperability
- Validate frontend component functionality and user workflows
- Test system performance under expected load scenarios

### 1.3 Scope

The test plan covers:

- Backend API endpoints (all controllers)
- Frontend Vue components and user interactions
- Database operations and transactions
- Authentication and authorization flows
- Stock movement business logic
- Multi-tenant data isolation
- API validation and error handling
- Frontend form validation and state management

## 2. Test Scope

### 2.1 In Scope

- **Backend Services**: All services in `10xWarehouseNet/Services/`
- **API Controllers**: All controllers in `10xWarehouseNet/Controllers/`
- **Frontend Components**: All Vue components in `10xWarehouseAstro/astro-app/src/components/`
- **Authentication**: Supabase JWT validation and RBAC policies
- **Database**: Entity Framework operations, migrations, constraints
- **Business Logic**: Stock movement transactions, inventory calculations
- **Validation**: Server-side (Data Annotations, custom validators) and client-side (Zod schemas)

### 2.2 Out of Scope

- Third-party services (Supabase infrastructure) - integration testing only
- Network infrastructure and deployment pipelines
- Browser compatibility testing (assume modern browsers)
- Load testing beyond expected production capacity
- Penetration testing (security audit separate)

## 3. Types of Tests

### 3.1 Unit Tests

#### 3.1.1 Backend Unit Tests (.NET xUnit)

**Location**: `10xWarehouseNet.Tests/Unit/`

**Services to Test**:

- `OrganizationService`
  - Organization creation and membership assignment
  - Organization retrieval with user permission checks
  - Edge cases: duplicate names, invalid IDs

- `StockMovementService` (CRITICAL)
  - `CreateStockMovementAsync` for each movement type:
    - Add: Positive delta, creates inventory if missing
    - Withdraw: Negative delta validation, insufficient stock detection
    - Move: Creates MoveSubtract and MoveAdd records atomically
    - Reconcile: Sets absolute quantity, calculates delta correctly
  - Inventory update calculations (`AddToInventoryAsync`, `SubtractFromInventoryAsync`, `SetInventoryQuantityAsync`)
  - Transaction rollback on failures
  - Concurrent movement handling

- `InventoryService`
  - Inventory aggregation queries
  - Filter application (locationId, productTemplateId, lowStock)
  - Pagination correctness

- `ProductTemplateService`
  - Product creation with unique barcode validation per organization
  - Product updates and retrieval
  - Barcode uniqueness constraint enforcement

- `WarehouseService` & `LocationService`
  - CRUD operations with organization scoping
  - Cascading delete behavior

- `RoleService`
  - Role validation for different operations
  - Permission matrix verification

**Validation Tests**:

- `StockMovementValidationAttribute`:
  - Movement type-specific validation rules
  - Required field validation per movement type
  - Delta sign validation (non-negative for Add, etc.)

#### 3.1.2 Frontend Unit Tests (Vitest/Vue Test Utils)

**Location**: `10xWarehouseAstro/astro-app/src/__tests__/`

**Components to Test**:

- Form components (`CreateProductModal`, `CreateWarehouseModal`, etc.):
  - Zod schema validation execution
  - Form state management (loading, error, success)
  - Submit handler execution

- Store modules (`stores/*.ts`):
  - State mutations
  - Action execution with API mocking
  - Error state handling

- Utility functions (`lib/validation.ts`):
  - `validateDisplayName`, `validatePassword`, `validateEmail`
  - `calculatePasswordStrength` scoring algorithm

**State Management Tests**:

- `authStore`: Token management, sign in/out flows
- `organizationStore`: Active organization switching
- `productStore`, `inventoryStore`: Data fetching and caching

### 3.2 Integration Tests

#### 3.2.1 API Integration Tests

**Location**: `10xWarehouseNet.Tests/Integration/`

**Setup**: In-memory database or test PostgreSQL container

**Test Scenarios**:

**Authentication Flow**:

- JWT token extraction from Authorization header
- Token validation via `SupabaseJwtAuthenticationService`
- Invalid/expired token handling
- User claims extraction (userId, email, active_org_id)

**Authorization (RBAC)**:

- `GET /api/warehouses`: Viewer, Member, Owner can access
- `POST /api/warehouses`: Only Member and Owner
- `DELETE /api/warehouses`: Only Owner
- `POST /api/organizations/{orgId}/invitations`: Only Owner
- Cross-organization access attempts (should return 403)

**Organization Endpoints**:

- `POST /api/organizations`: Creates organization and assigns creator as Owner
- `GET /api/organizations`: Returns only organizations user belongs to
- Pagination verification

**Warehouse & Location Endpoints**:

- Create warehouse → Verify organization scoping
- Create location → Verify warehouse relationship
- Delete warehouse → Verify cascading delete of locations

**Product Template Endpoints**:

- Create product with barcode → Verify uniqueness per organization
- Create duplicate barcode (same org) → Expect 409 Conflict
- Create duplicate barcode (different org) → Should succeed

**Stock Movement Endpoints** (CRITICAL):

- `POST /api/stock-movements` (Add):
  - Creates stock_movements record
  - Creates/updates inventory summary atomically
  - Returns 201 with correct delta/total
- `POST /api/stock-movements` (Withdraw):
  - Sufficient stock → Reduces inventory, creates negative delta record
  - Insufficient stock → Returns 409 Conflict, no inventory change
- `POST /api/stock-movements` (Move):
  - Creates MoveSubtract and MoveAdd records
  - Updates both source and destination inventory in transaction
  - Rollback if either update fails
- `POST /api/stock-movements` (Reconcile):
  - Sets inventory to absolute quantity (command.Delta)
  - Calculates correct delta value in record
- `GET /api/stock-movements`: Filtering by productTemplateId, locationId, pagination

**Inventory Endpoints**:

- `GET /api/inventory`: Aggregation correctness
- Low stock filter: `quantity <= lowStockThreshold`
- Multi-location and multi-product queries

**Dashboard Endpoint**:

- `GET /api/dashboard`: Returns recent movements and low stock alerts
- Verification of data aggregation

**Error Handling**:

- 400 Bad Request: Invalid request body, missing required fields
- 401 Unauthorized: Missing/invalid JWT token
- 403 Forbidden: Insufficient permissions
- 404 Not Found: Non-existent resources
- 409 Conflict: Business rule violations (duplicate barcode, insufficient stock)
- 500 Internal Server Error: Unhandled exceptions

**Multi-Tenancy Verification**:

- User A creates organization → User B cannot access it
- User A creates warehouse in Org1 → User B in Org2 cannot see it
- Organization ID scoping in all queries

#### 3.2.2 Database Integration Tests

**Location**: `10xWarehouseNet.Tests/Integration/Db/`

**Test Scenarios**:

- Entity Framework migrations apply successfully
- Unique constraints: ProductTemplate barcode per organization
- Unique constraints: Inventory (orgId, productTemplateId, locationId)
- Foreign key constraints: Locations require valid WarehouseId
- Cascade delete: Warehouse deletion removes locations
- Transaction isolation: Concurrent stock movements
- Index performance: Query execution plans for paginated endpoints

### 3.3 End-to-End (E2E) Tests

**Location**: `10xWarehouseAstro/astro-app/e2e/`

**Framework**: Playwright or Cypress

**Critical User Flows**:

**Authentication & Registration**:

1. User registration with Supabase
2. Email verification flow
3. User login with valid credentials
4. JWT token storage and usage in API calls
5. Token expiration handling and re-authentication
6. Invalid credentials rejection

**Organization Management**:

1. Create new organization
2. Switch between multiple organizations
3. View organization members (Owner role)
4. Invite new member (Owner role)
5. Accept invitation flow
6. Remove member (Owner role)

**Warehouse & Location Management**:

1. Create warehouse
2. Create multiple locations within warehouse
3. Edit warehouse details
4. Edit location details
5. Delete location
6. Delete warehouse (with confirmation modal)
7. Verify locations cascade delete

**Product Management**:

1. Create product template with barcode
2. Attempt duplicate barcode (should show error)
3. Edit product template
4. Delete product template
5. View product list with pagination
6. Search/filter products

**Inventory Operations**:

1. Add stock to location
2. Withdraw stock from location
3. Move stock between locations
4. Reconcile inventory (set absolute quantity)
5. View inventory summary with filters (location, product, low stock)
6. Verify inventory calculations after operations
7. Attempt withdrawal exceeding available stock (should show error)

**Stock Movement Log**:

1. View stock movement history
2. Filter by product
3. Filter by location
4. Pagination through movements
5. Verify movement records match operations performed

**Dashboard**:

1. View dashboard with recent movements
2. View low stock alerts
3. Verify data accuracy

**Role-Based Access Control**:

1. Viewer role: Can only view, cannot create/edit/delete
2. Member role: Can create/edit warehouses, products, stock movements
3. Owner role: Full access including member management
4. UI elements hidden/shown based on role (v-if directives)

**Error Handling**:

1. Network errors show user-friendly messages
2. 401 errors redirect to login
3. 403 errors show access denied message
4. 400 validation errors show field-specific messages
5. 409 conflicts show appropriate business error messages

### 3.4 Performance Tests

#### 3.4.1 Backend Performance Tests

**Location**: `10xWarehouseNet.Tests/Performance/`

**Tool**: NBomber or k6

**Test Scenarios**:

- **Stock Movement Throughput**:
  - Concurrent stock movements (100+ requests/sec)
  - Measure transaction success rate and rollback handling
  - Verify no inventory inconsistencies under load

- **Pagination Performance**:
  - Large datasets (10,000+ records)
  - Measure query execution time for various page sizes
  - Index effectiveness verification

- **Multi-Tenant Query Isolation**:
  - Multiple organizations with large datasets
  - Verify query performance doesn't degrade with tenant count
  - Organization ID filter effectiveness

- **Dashboard Aggregation**:
  - Large number of stock movements (10,000+)
  - Recent movements query performance
  - Low stock calculation performance

#### 3.4.2 Frontend Performance Tests

**Tool**: Lighthouse CI or WebPageTest

**Metrics to Measure**:

- Time to First Contentful Paint (FCP)
- Largest Contentful Paint (LCP)
- Time to Interactive (TTI)
- Bundle size (Astrostudio analyze)
- Component rendering performance (Vue DevTools Profiler)

**Test Scenarios**:

- Initial page load time
- Dashboard data loading
- Large inventory list rendering (virtual scrolling if implemented)
- Form submission responsiveness

### 3.5 Security Tests

#### 3.5.1 Authentication & Authorization Security

**Manual/OWASP ZAP**

**Test Scenarios**:

- JWT token manipulation: Invalid signature, expired token, missing claims
- Token injection: Attempt to use token from different user
- Authorization bypass: Attempt operations with insufficient role
- Organization ID manipulation: Try to access different organization's data via query parameters
- SQL injection: Test user-controlled input in queries (EF Core should protect, but verify)
- XSS prevention: Input sanitization in API responses
- CORS policy verification: Only allowed origins can access API

#### 3.5.2 Data Isolation Tests

**Manual/Integration Tests**

**Test Scenarios**:

- User A cannot query User B's organizations
- User A cannot access User B's warehouses by ID guessing
- Stock movements from Org1 don't affect Org2's inventory
- Product barcode uniqueness scoped to organization correctly

#### 3.5.3 Input Validation Security

**Manual/Integration Tests**

**Test Scenarios**:

- Large payloads (DoS prevention)
- SQL injection attempts in text fields
- XSS attempts in text fields
- Negative values where not allowed (e.g., negative delta for Add)
- Extremely large numbers (integer overflow)

## 4. Test Scenarios for Key Functionalities

### 4.1 Stock Movement Business Logic (CRITICAL)

**Scenario 4.1.1: Add Stock**

- **Given**: Product exists, location低价 exists, inventory quantity = 50
- **When**: Add 25 units
- **Then**: 
  - Stock movement record created with delta = 25, total = 75
  - Inventory updated to quantity = 75
  - Transaction committed atomically

**Scenario 4.1.2: Add Stock to Empty Location**

- **Given**: Product exists, location exists, no inventory record
- **When**: Add 10 units
- **Then**: 
  - New inventory record created with quantity = 10
  - Stock movement record created correctly

**Scenario 4.1.3: Withdraw Stock - Sufficient**

- **Given**: Inventory quantity = 100
- **When**: Withdraw 30 units
- **Then**: 
  - Stock movement record created with delta = -30, total = 70
  - Inventory updated to quantity = 70的反转
  - Returns 201 Created

**Scenario 4.1.4: Withdraw Stock - Insufficient**

- **Given**: Inventory quantity = 20
- **When**: Withdraw 50 units
- **Then**: 
  - Returns 409 Conflict with error message
  - No inventory change
  - No stock movement record created
  - Transaction rolled back

**Scenario 4.1.5: Move Stock**

- **Given**: Source location quantity = 100, destination location quantity = 50
- **When**: Move 25 units
- **Then**: 
  - MoveSubtract record: delta = -25, total = 75
  - MoveAdd record: delta = 25, total = 75 (destination)
  - Both inventory records updated in single transaction
  - If MoveAdd fails, MoveSubtract is rolled back

**Scenario 4.1.6: Reconcile Stock**

- **Given**: Current inventory quantity = 45
- **When**: Reconcile to 60
- **Then**: 
  - Stock movement delta calculated as +15 (60 - 45)
  - Inventory set to quantity = 60
  - Total in movement record = 60

**Scenario 4.1.7: Concurrent Stock Movements**

- **Given**: Inventory quantity = 100
- **When**: Two concurrent withdrawals of 60 units each
- **Then**: 
  - One succeeds, one returns 409 Conflict
  - Final quantity = 40 (not negative)
  - Database transaction isolation prevents race condition

### 4.2 Multi-Tenancy Isolation

**Scenario 4.2.1: Organization Data Isolation**

- **Given**: User A in Org1, User B in Org2
- **When**: User A queries warehouses
- **Then**: Only Org1 warehouses returned, Org2 data not visible

**Scenario 4.2.2: Cross-Organization Access Attempt**

- **Given**: User A belongs to Org1 only
- **When**: User A attempts `GET /api/warehouses?organizationId={Org2Id}`
- **Then**: Returns 403 Forbidden or 404 Not Found

**Scenario 4.2.3: Barcode Uniqueness Per Organization**

- **Given**: Org1 has product with barcode "12345"
- **When**: Org2 creates product with barcode "12345"
- **Then**: Operation succeeds (uniqueness scoped to organization)

### 4.3 Role-Based Access Control

**Scenario 4.3.1: Viewer Permissions**

- **Given**: User with Viewer role
- **When**: Attempts POST /api/warehouses
- **Then**: Returns 403 Forbidden

**Scenario 4.3.2: Member Permissions**

- **Given**: User with Member role
- **When**: Creates warehouse, product, stock movement
- **Then**: Operations succeed
- **When**: Attempts to invite member
- **Then**: Returns 403 Forbidden

**Scenario 4.3.3: Owner Permissions**

- **Given**: User with Owner role
- **When**: Performs all operations including member/invitation management
- **Then**: All operations succeed

## 5. Test Environment

### 5.1 Development Environment

- **Backend**: Local .NET runtime, local PostgreSQL database
- **Frontend**: Astro dev server (localhost:4321)
- **Supabase**: Development project instance
- **Tools**: Swagger UI for API testing, Vue DevTools for frontend debugging

### 5.2 Test Environment

- **Database**: Dockerized PostgreSQL container or in-memory database for unit tests
- **Backend API**: Test server instance on dedicated port
- **Frontend**: Build and serve test build
- **Supabase**: Separate test project or mocked Supabase client
- **Isolation**: Each test run uses fresh database (migrations applied, data seeded if needed)

### 5.3 Staging Environment

- **Purpose**: Pre-production validation with production-like setup
- **Database**: Staging PostgreSQL instance
- **Supabase**: Staging Supabase project
- **E2E Tests**: Automated runs against staging environment

### 5.4 Test Data Management

- **Test Data Seeds**: Scripts to create test organizations, users, warehouses, products
- **Data Isolation**: Each test should clean up or use unique identifiers
- **Role Test Users**: Pre-created users with Viewer, Member, Owner roles
- **Multi-Tenant Data**: Multiple organizations with overlapping data (e.g., same product names, barcodes)

## 6. Testing Tools

### 6.1 Backend Testing

- **Unit Testing**: xUnit.net (`dotnet add package xunit`)
- **Mocking**: Moq or NSubstitute for service dependencies
- **Integration Testing**: xUnit with TestContainers for PostgreSQL or in-memory database
- **API Testing**: xUnit with `WebApplicationFactory<T>` for in-process API testing
- **Performance Testing**: NBomber or k6 for load testing
- **Code Coverage**: Coverlet + ReportGenerator

### 6.2 Frontend Testing

- **Unit Testing**: Vitest (`npm install -D vitest @vue/test-utils`)
- **Component Testing**: Vue Test Utils with Vitest
- **E2E Testing**: Playwright (`npm install -D @playwright/test`) or Cypress
- **Visual Regression**: Playwright screenshots or Percy
- **Performance**: Lighthouse CI for performance budgets
- **Code Coverage**: Vitest coverage with `@vitest/coverage-v8`

### 6.3 API Testing

- **Manual Testing**: Swagger UI, Postman, or Insomnia
- **Automated API Tests**: xUnit integration tests or Newman (Postman collections)
- **Contract Testing**: Consider Pact for API contract validation (optional)

### 6.4 Test Data & Utilities

- **Database Seeding**: EF Core migrations with seed data or custom seeding scripts
- **Test Fixtures**: Builder pattern for creating test entities
- **API Helpers**: Helper methods for authenticated requests (JWT token generation)

## 7. Test Schedule

### 7.1 Phase 1: Unit Tests (Week 1-2)

- **Priority**: High
- **Focus**: Core business logic, especially `StockMovementService`
- **Deliverable**: 80%+ code coverage on services and controllers

### 7.2 Phase 2: Integration Tests (Week 3-4)

- **Priority**: High
- **Focus**: API endpoints, authentication, authorization, multi-tenancy
- **Deliverable**: All critical API endpoints covered with integration tests

### 7.3 Phase 3: E2E Tests (Week 5-6)

- **Priority**: Medium
- **Focus**: Critical user flows, role-based access in UI
- **Deliverable**: 10-15 critical E2E test scenarios

### 7.4 Phase 4: Performance & Security (Week 7-8)

- **Priority**: Medium
- **Focus**: Load testing, security vulnerability testing
- **Deliverable**: Performance benchmarks, security test report

### 7.5 Phase 5: Regression Testing (Ongoing)

- **Priority**: Continuous
- **Focus**: Automated regression suite runs on each PR
- **Deliverable**: CI/CD pipeline with automated test execution

## 8. Test Acceptance Criteria

### 8.1 Code Coverage Targets

- **Services**: Minimum 85% line coverage
- **Controllers**: Minimum 80% line coverage
- **Critical Business Logic** (`StockMovementService`): Minimum 95% line coverage
- **Frontend Components**: Minimum 70% coverage for form and business logic components

### 8.2 Test Execution Criteria

- All unit tests pass (no flaky tests)
- All integration tests pass with test database
- Critical E2E flows pass (authentication, stock movements, RBAC)
- No critical or high severity bugs in test execution
- Performance benchmarks meet SLA requirements:
  - API response time < 200ms (p95) for standard operations
  - Stock movement transactions complete < 100ms (p95)
  - Dashboard loads < 2s (p95)

### 8.3 Quality Gates

- **Before PR Merge**: All unit and integration tests pass
- **Before Release**: All E2E tests pass, security scan passes, performance tests pass
- **Production Deployment**: Full regression suite passes on staging environment

## 9. Roles and Responsibilities

### 9.1 Development Team

- **Developers**: Write unit tests alongside feature development, maintain test coverage
- **Tech Lead**: Review test strategies, ensure test quality standards

### 9.2 QA Team (if applicable)

- **QA Engineer**: Design E2E test scenarios, execute manual testing, maintain test automation
- **QA Lead**: Review test plans, coordinate test execution schedules

### 9.3 DevOps/Infrastructure

- **DevOps Engineer**: Set up CI/CD pipelines for automated test execution, manage test environments

### 9.4 Product Owner

- **Product Owner**: Validate test scenarios cover business requirements, prioritize bug fixes

## 10. Bug Reporting Procedures

### 10.1 Bug Severity Levels

- **Critical**: System crashes, data loss, security vulnerabilities, production blocking issues
- **High**: Major functionality broken, incorrect business logic (e.g., wrong inventory calculations)
- **Medium**: Minor functionality issues, UI/UX problems, edge cases
- **Low**: Cosmetic issues, minor improvements, typos

### 10.2 Bug Report Template

```
**Title**: [Short description]
**Severity**: [Critical/High/Medium/Low]
**Environment**: [Development/Staging/Production]
**Steps to Reproduce**:
1. [Step 1]
2. [Step 2]
3. [Step 3]

**Expected Behavior**: [What should happen]
**Actual Behavior**: [What actually happened]
**Screenshots/Logs**: [If applicable]
**Additional Context**: [API responses, error messages, etc.]
```

### 10.3 Bug Tracking

- Use project issue tracker (GitHub Issues, Jira, etc.)
- Tag bugs with appropriate labels (backend, frontend, api, security, etc.)
- Link bugs to corresponding test cases
- Track bug resolution and regression testing

### 10.4 Bug Triage Process

1. Bug reported → Assigned severity
2. Critical/High bugs → Immediate attention, blocking release if not fixed
3. Medium/Low bugs → Prioritized in sprint planning
4. Bug fix → Verified with original test case, regression tests run
5. Bug closed → Documented in test report

---

## Appendix A: Test Case Examples

### Unit Test Example: StockMovementService.Add

```csharp
[Fact]
public async Task CreateStockMovement_Add_UpdatesInventoryCorrectly()
{
    // Arrange
    var orgId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    var locationId = Guid.NewGuid();
    var command = new CreateStockMovementCommand 
    { 
        MovementType = MovementType.Add, 
        ProductTemplateId = productId, 
        LocationId = locationId, 
        Delta = 25 
    };
    
    // Act
    var result = await _service.CreateStockMovementAsync(orgId, userId, command);
    
    // Assert
    Assert.Equal(25, result.Delta);
    var inventory = await _context.Inventories.FirstAsync();
    Assert.Equal(25, inventory.Quantity);
}
```

### Integration Test Example: API Authorization

```csharp
[Fact]
public async Task POST_Warehouses_WithoutAuth_Returns401()
{
    var client = _factory.CreateClient();
    var response = await client.PostAsync("/api/warehouses", content);
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

### E2E Test Example: Add Stock Flow

```typescript
test('user can add stock to inventory', async ({ page }) => {
  await page.goto('/warehouses');
  await page.click('text=Add Stock');
  await page.fill('[name="quantity"]', '25');
  await page.click('button[type="submit"]');
  await expect(page.locator('.inventory-quantity')).toContainText('125');
});
```

---

**Document Version**: 1.0

**Last Updated**: [Current Date]

**Author**: QA Team

**Review Status**: Draft