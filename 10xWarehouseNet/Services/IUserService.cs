using _10xWarehouseNet.Dtos;

namespace _10xWarehouseNet.Services;

public interface IUserService
{
    Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request);
    Task<UserProfileDto> GetUserProfileAsync(string userId);
    Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileRequestDto request);
}
