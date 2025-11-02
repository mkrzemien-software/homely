using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for UserProfile operations
/// </summary>
public interface IUserProfileRepository : IBaseRepository<UserProfileEntity, Guid>
{
    /// <summary>
    /// Get user profile with household memberships
    /// </summary>
    Task<UserProfileEntity?> GetUserWithHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search users by various criteria with pagination
    /// </summary>
    Task<PagedResult<UserProfileEntity>> SearchUsersAsync(
        string? searchTerm,
        string? role,
        string? status,
        Guid? householdId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get users by household ID
    /// </summary>
    Task<IEnumerable<UserProfileEntity>> GetUsersByHouseholdAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update last active timestamp for user
    /// </summary>
    Task UpdateLastActiveAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
