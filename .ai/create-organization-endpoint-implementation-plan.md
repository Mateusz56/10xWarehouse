# API Endpoint Implementation Plan: Create Organization

## 1. Endpoint Overview
This document outlines the implementation plan for the `POST /api/organizations` endpoint. The purpose of this endpoint is to allow an authenticated user to create a new organization. Upon creation, the user who initiated the request will be automatically assigned as the 'Owner' of the new organization.

## 2. Request Details
- **HTTP Method**: `POST`
- **URL Structure**: `/api/organizations`
- **Request Body**: The request body must be a JSON object with the following structure:
  ```json
  {
    "name": "string"
  }
  ```
- **Headers**:
    - `Authorization`: `Bearer <JWT_TOKEN>` (Required)
    - `Content-Type`: `application/json` (Required)

## 3. Used Types
- **Request DTO**: `_10xWarehouseNet.Dtos.OrganizationDtos.CreateOrganizationRequestDto`
- **Response DTO**: `_10xWarehouseNet.Dtos.OrganizationDtos.OrganizationDto`
- **Database Models**:
    - `_10xWarehouseNet.Db.Models.Organization`
    - `_10xWarehouseNet.Db.Models.OrganizationMember`
- **Enum**: `_10xWarehouseNet.Db.Enums.UserRole`

## 4. Data Flow
1. A client sends a `POST` request to `/api/organizations` with a valid JWT and a JSON body containing the organization `name`.
2. The ASP.NET Core authentication middleware validates the JWT. If invalid, it rejects the request with a `401 Unauthorized` status.
3. The request is routed to the `CreateOrganization` action in the `OrganizationsController`.
4. The framework performs model binding and validation on the request body using the `CreateOrganizationRequestDto`. If validation fails, a `400 Bad Request` is returned.
5. The controller extracts the creator's user ID from the JWT claims (`HttpContext.User`).
6. The controller calls the `IOrganizationService.CreateOrganizationAsync` method, passing the request DTO and the user ID.
7. The service initiates a database transaction.
8. Inside the transaction, it:
    a. Creates a new `Organization` entity with the provided name.
    b. Creates a new `OrganizationMember` entity, linking the new organization's ID with the creator's user ID and setting the `Role` to `UserRole.Owner`.
    c. Saves both entities to the database using `DbContext.SaveChangesAsync()`.
9. If the transaction is successful, the service returns the newly created `Organization` entity.
10. The controller maps the `Organization` entity to an `OrganizationDto`.
11. The controller returns a `201 Created` response with the `OrganizationDto` in the body.

## 5. Security Considerations
- **Authentication**: The endpoint will be decorated with the `[Authorize]` attribute to ensure only authenticated users can access it.
- **Authorization**: Any authenticated user is permitted to create a new organization. No specific roles are required for this action.
- **Input Validation**: The `Name` property on the `CreateOrganizationRequestDto` will be validated using `[Required]` and `[StringLength(100)]` attributes to prevent null/empty values and overly long inputs.
- **User Identity**: The user's ID must be reliably retrieved from the JWT's `sub` (subject) claim to ensure the correct user is assigned as the owner.

## 6. Error Handling
The following error conditions and their corresponding HTTP status codes will be handled:
- **400 Bad Request**:
    - The request body is malformed or missing.
    - The `name` field is missing, empty, or exceeds the maximum length.
    - A `ValidationProblemDetails` object will be returned with details about the validation errors.
- **401 Unauthorized**:
    - The `Authorization` header is missing or the JWT is invalid/expired.
- **500 Internal Server Error**:
    - An unhandled exception occurs, such as a database connection failure or a transaction commit failure.
    - A generic error message will be returned, and detailed exception information will be logged for diagnostics.

## 7. Performance Considerations
- The operation involves two database inserts within a single transaction. This is a fast operation and is not expected to be a performance bottleneck.
- Database indexes on the foreign keys of the `organization_members` table (`organization_id`, `user_id`) will ensure efficient writes and subsequent queries. These are typically created automatically by EF Core when defining relationships.

## 8. Implementation Steps
1. **Create Service Layer**:
   - Define an `IOrganizationService` interface with a method: `Task<Organization> CreateOrganizationAsync(CreateOrganizationRequestDto request, Guid userId);`
   - Implement the `OrganizationService` class, injecting `WarehouseDbContext` and `ILogger`.
   - Implement the `CreateOrganizationAsync` method, ensuring the database operations are wrapped in a transaction (`await _context.Database.BeginTransactionAsync()`).
2. **Register Service**:
   - Register the `IOrganizationService` and `OrganizationService` for dependency injection in `Program.cs` (`builder.Services.AddScoped<IOrganizationService, OrganizationService>();`).
3. **Create Controller**:
   - Create a new API controller named `OrganizationsController.cs`.
   - Inject `IOrganizationService` and `ILogger` into the controller's constructor.
4. **Implement Controller Action**:
   - Create a `POST` action method `CreateOrganization([FromBody] CreateOrganizationRequestDto request)`.
   - Add the `[Authorize]` and `[HttpPost]` attributes to the method.
   - Add model validation logic (`if (!ModelState.IsValid)`).
   - Retrieve the current user's ID from `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
   - Call the `organizationService.CreateOrganizationAsync` method.
   - Map the result to `OrganizationDto`.
   - Return a `CreatedAtActionResult` with the 201 status code, the route to get the new organization (if applicable), and the `OrganizationDto`.
5. **Add Validation Attributes**:
   - Ensure the `CreateOrganizationRequestDto.Name` property has `[Required]` and `[StringLength(100)]` attributes.
6. **Unit/Integration Testing**:
   - Write unit tests for the `OrganizationService` to verify the transaction logic and correct entity creation.
