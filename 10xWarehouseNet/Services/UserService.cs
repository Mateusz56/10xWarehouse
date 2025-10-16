using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;
using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Clients;
using Supabase.Gotrue.Exceptions;

namespace _10xWarehouseNet.Services;

public class UserService : IUserService
{
    private readonly WarehouseDbContext _context;
    private readonly IOrganizationService _organizationService;
    private readonly SupabaseUsers _supabaseUsers;
    private readonly ILogger<UserService> _logger;

    public UserService(
        WarehouseDbContext context, 
        IOrganizationService organizationService, 
        SupabaseUsers supabaseUsers,
        ILogger<UserService> logger)
    {
        _context = context;
        _organizationService = organizationService;
        _supabaseUsers = supabaseUsers;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request)
    {
        try
        {
            // Register user with Supabase
            var supabaseUser = await _supabaseUsers.RegisterUserAsync(
                request.Email, 
                request.Password, 
                request.DisplayName);

            if (supabaseUser == null)
            {
                throw new InvalidOperationException("Failed to create user account.");
            }

            var response = new RegisterResponseDto
            {
                User = new UserProfileDto
                {
                    Id = supabaseUser.Id.ToString(),
                    Email = request.Email,
                    DisplayName = request.DisplayName
                }
            };

            // Create organization if requested
            if (request.CreateOrganization)
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName))
                {
                    throw new ArgumentException("Organization name is required when creating an organization.");
                }

                var organization = await _organizationService.CreateOrganizationAsync(
                    new CreateOrganizationRequestDto { Name = request.OrganizationName }, 
                    supabaseUser.Id.ToString());

                response.Organization = new OrganizationDto(organization.Id, organization.Name);
            }

            return response;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException || ex is GotrueException))
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            throw new DatabaseOperationException("An error occurred during user registration.", ex);
        }
    }

    public async Task<UserProfileDto> GetUserProfileAsync(string userId)
    {
        try
        {
            // Get user from Supabase via the existing UsersController logic
            // This is a simplified version - in practice, you might want to cache this
            var userMemberships = await _organizationService.GetUserMembershipsAsync(userId);
            
            // For now, we'll return basic profile info
            // In a real implementation, you might want to store additional user profile data
            return new UserProfileDto
            {
                Id = userId,
                Email = "", // This would come from the JWT token in the controller
                DisplayName = "" // This would come from the JWT token in the controller
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
            throw new DatabaseOperationException("An error occurred while retrieving user profile.", ex);
        }
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileRequestDto request)
    {
        try
        {
            // In this implementation, we don't store user profile data locally
            // The display name comes from Supabase metadata
            // This method is here for API consistency but would need to be implemented
            // by updating Supabase user metadata
            
            _logger.LogInformation("User profile update requested for user {UserId} - DisplayName: {DisplayName}", 
                userId, request.DisplayName);
            
            // For now, return the updated profile
            return new UserProfileDto
            {
                Id = userId,
                Email = "", // Would come from JWT
                DisplayName = request.DisplayName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for user {UserId}", userId);
            throw new DatabaseOperationException("An error occurred while updating user profile.", ex);
        }
    }
}
