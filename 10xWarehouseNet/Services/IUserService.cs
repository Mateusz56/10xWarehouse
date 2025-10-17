using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IUserService
{
    Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request);
    Task<UserProfileDto> GetUserProfileAsync(string userId);
    Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileRequestDto request);
    Task<bool> ChangeUserPasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<List<UserSearchResult>> SearchUsersAsync(string query, int limit = 10);
}
