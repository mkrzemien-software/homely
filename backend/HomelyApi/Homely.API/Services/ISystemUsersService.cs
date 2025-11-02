using Homely.API.Models.DTOs.SystemUsers;

namespace Homely.API.Services;

/// <summary>
/// Service interface for system-level user management (System Developer role)
/// </summary>
public interface ISystemUsersService
{
    /// <summary>
    /// Search users with filtering and pagination
    /// </summary>
    Task<PaginatedUsersDto> SearchUsersAsync(UserSearchFiltersDto filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed user information
    /// </summary>
    Task<SystemUserDto?> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user activity history
    /// </summary>
    Task<List<UserActivityDto>> GetUserActivityAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new user with Supabase auth and profile
    /// </summary>
    Task<SystemUserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user role in household
    /// </summary>
    Task<SystemUserDto> UpdateUserRoleAsync(Guid userId, Guid householdId, string newRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset user password (sends reset email)
    /// </summary>
    Task<bool> ResetUserPasswordAsync(Guid userId, bool sendEmail = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlock user account
    /// </summary>
    Task<bool> UnlockUserAccountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move user to different household
    /// </summary>
    Task<SystemUserDto> MoveUserToHouseholdAsync(Guid userId, Guid newHouseholdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user from system (soft delete profile, hard delete from Supabase Auth)
    /// </summary>
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
