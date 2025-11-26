using Homely.API.Entities;

namespace Homely.API.Services;

/// <summary>
/// Service interface for household member management
/// </summary>
public interface IHouseholdMemberService
{
    /// <summary>
    /// Add a new member to a household
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="userId">The user ID to add</param>
    /// <param name="role">The role to assign (admin, member, dashboard)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created household member entity</returns>
    Task<HouseholdMemberEntity> AddMemberAsync(Guid householdId, Guid userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a member from a household (soft delete)
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="userId">The user ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if member was removed, false if member was not found</returns>
    Task<bool> RemoveMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a member's role in a household
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="userId">The user ID</param>
    /// <param name="newRole">The new role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated household member entity</returns>
    Task<HouseholdMemberEntity> UpdateMemberRoleAsync(Guid householdId, Guid userId, string newRole, CancellationToken cancellationToken = default);
}
