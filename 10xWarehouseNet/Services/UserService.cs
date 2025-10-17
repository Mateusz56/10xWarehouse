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
            // Get user from Supabase Admin API
            var user = await _supabaseUsers.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            
            var displayName = user.UserMetadata?.GetValueOrDefault("display_name")?.ToString() ?? user.Email ?? "";
            
            return new UserProfileDto
            {
                Id = userId,
                Email = user.Email ?? "",
                DisplayName = displayName
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
            // Update display name in Supabase metadata
            await _supabaseUsers.UpdateUserDisplayNameAsync(userId, request.DisplayName);
            
            _logger.LogInformation("User profile update requested for user {UserId} - DisplayName: {DisplayName}", 
                userId, request.DisplayName);
            
            // Return updated profile
            return new UserProfileDto
            {
                Id = userId,
                Email = "", // Will be populated from JWT in controller
                DisplayName = request.DisplayName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for user {UserId}", userId);
            throw new DatabaseOperationException("An error occurred while updating user profile.", ex);
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        try
        {
            // Update password using Supabase Admin API
            await _supabaseUsers.ChangeUserPasswordAsync(userId, request.NewPassword);
            
            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw new DatabaseOperationException("An error occurred while changing password.", ex);
        }
    }

    public async Task<List<UserSearchResult>> SearchUsersAsync(string query, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<UserSearchResult>();
            }

            var users = await _supabaseUsers.SearchUsersAsync(query, limit);
            
            var searchResults = users.Select(user => new UserSearchResult
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                DisplayName = user.UserMetadata?.GetValueOrDefault("display_name")?.ToString() ?? string.Empty
            }).ToList();

            _logger.LogInformation("Found {Count} users matching query '{Query}'", searchResults.Count, query);
            return searchResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query '{Query}'", query);
            throw new DatabaseOperationException("An error occurred while searching users.", ex);
        }
    }
}
