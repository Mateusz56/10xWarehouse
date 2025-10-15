using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using _10xWarehouseNet.Services;

namespace _10xWarehouseNet.Authentication
{
    public class DatabaseRoleAuthorizationHandler : AuthorizationHandler<DatabaseRoleRequirement>
    {
        private readonly IRoleService _roleService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseRoleAuthorizationHandler(IRoleService roleService, IHttpContextAccessor httpContextAccessor)
        {
            _roleService = roleService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DatabaseRoleRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // Get organization ID from route or query parameters
            var organizationId = GetOrganizationIdFromRequest();
            if (organizationId == null)
            {
                context.Fail();
                return;
            }

            bool hasRequiredRole = false;

            switch (requirement.RoleType)
            {
                case DatabaseRoleType.OwnerOrMember:
                    hasRequiredRole = await _roleService.IsUserOwnerOrMemberAsync(userId, organizationId.Value);
                    break;
                case DatabaseRoleType.OwnerOnly:
                    hasRequiredRole = await _roleService.IsUserOwnerAsync(userId, organizationId.Value);
                    break;
                case DatabaseRoleType.OrganizationMember:
                    hasRequiredRole = await _roleService.IsUserOrganizationMemberAsync(userId, organizationId.Value);
                    break;
            }

            if (hasRequiredRole)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        private Guid? GetOrganizationIdFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // Try to get organization ID from route parameters
            if (httpContext.Request.RouteValues.TryGetValue("organizationId", out var orgIdValue) && 
                Guid.TryParse(orgIdValue?.ToString(), out var orgIdFromRoute))
            {
                return orgIdFromRoute;
            }

            // Try to get from query parameters
            if (httpContext.Request.Query.TryGetValue("organizationId", out var orgIdQuery) && 
                Guid.TryParse(orgIdQuery.FirstOrDefault(), out var orgIdFromQuery))
            {
                return orgIdFromQuery;
            }

            // Try to get from headers
            if (httpContext.Request.Headers.TryGetValue("X-Organization-Id", out var orgIdHeader) && 
                Guid.TryParse(orgIdHeader.FirstOrDefault(), out var orgIdFromHeader))
            {
                return orgIdFromHeader;
            }

            return null;
        }
    }

    public class DatabaseRoleRequirement : IAuthorizationRequirement
    {
        public DatabaseRoleType RoleType { get; }

        public DatabaseRoleRequirement(DatabaseRoleType roleType)
        {
            RoleType = roleType;
        }
    }

    public enum DatabaseRoleType
    {
        OwnerOrMember,
        OwnerOnly,
        OrganizationMember
    }
}
