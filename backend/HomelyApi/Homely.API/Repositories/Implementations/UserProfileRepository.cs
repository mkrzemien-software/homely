using System.Linq.Expressions;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for UserProfile operations
/// </summary>
public class UserProfileRepository : BaseRepository<UserProfileEntity, Guid>, IUserProfileRepository
{
    public UserProfileRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<UserProfileEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.UserId == id;
    }

    /// <summary>
    /// Get user profile with household memberships
    /// </summary>
    public async Task<UserProfileEntity?> GetUserWithHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.UserProfiles
            .Include(up => up.HouseholdMemberships)
                .ThenInclude(hm => hm.Household)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Search users by various criteria with pagination
    /// </summary>
    public async Task<PagedResult<UserProfileEntity>> SearchUsersAsync(
        string? searchTerm,
        string? role,
        string? status,
        Guid? householdId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.UserProfiles
            .Include(up => up.HouseholdMemberships)
                .ThenInclude(hm => hm.Household)
            .Where(up => up.DeletedAt == null)
            .AsQueryable();

        // Search term filter (email, first name, last name)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(up =>
                up.FirstName.ToLower().Contains(lowerSearchTerm) ||
                up.LastName.ToLower().Contains(lowerSearchTerm));
        }

        // Role filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(up => up.HouseholdMemberships.Any(hm => hm.Role == role && hm.DeletedAt == null));
        }

        // Household filter
        if (householdId.HasValue)
        {
            query = query.Where(up => up.HouseholdMemberships.Any(hm => hm.HouseholdId == householdId.Value && hm.DeletedAt == null));
        }

        // Note: Status filtering would require integration with Supabase auth.users table
        // For now, we'll skip status filtering or implement it at service layer

        // Total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination
        var items = await query
            .OrderBy(up => up.FirstName)
            .ThenBy(up => up.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserProfileEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Get users by household ID
    /// </summary>
    public async Task<IEnumerable<UserProfileEntity>> GetUsersByHouseholdAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Context.UserProfiles
            .Include(up => up.HouseholdMemberships)
                .ThenInclude(hm => hm.Household)
            .Where(up => up.HouseholdMemberships.Any(hm => hm.HouseholdId == householdId && hm.DeletedAt == null)
                         && up.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update last active timestamp for user
    /// </summary>
    public async Task UpdateLastActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LastActiveAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(user, cancellationToken);
        }
    }

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Note: This should query Supabase auth.users table via API
        // For now, returning false as placeholder
        // Implementation would require Supabase client integration
        return false;
    }
}
