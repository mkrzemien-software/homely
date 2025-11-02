using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class HouseholdRepository : BaseRepository<HouseholdEntity, Guid>, IHouseholdRepository
{
    public HouseholdRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<HouseholdEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<HouseholdEntity>> GetUserHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(h => h.HouseholdMembers.Any(hm => hm.UserId == userId && hm.DeletedAt == null))
            .Include(h => h.PlanType)
            .ToListAsync(cancellationToken);
    }

    public async Task<HouseholdEntity?> GetHouseholdWithMembersAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(householdId, 
            h => h.HouseholdMembers, 
            h => h.PlanType);
    }

    public async Task<HouseholdEntity?> GetHouseholdWithDetailsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(h => h.Id == householdId)
            .Include(h => h.PlanType)
            .Include(h => h.HouseholdMembers)
            .Include(h => h.Items.Where(i => i.DeletedAt == null && i.IsActive))
            .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsUserMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<HouseholdMemberEntity>()
            .AnyAsync(hm => hm.HouseholdId == householdId && 
                           hm.UserId == userId && 
                           hm.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> IsUserAdminAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<HouseholdMemberEntity>()
            .AnyAsync(hm => hm.HouseholdId == householdId &&
                           hm.UserId == userId &&
                           hm.Role == DatabaseConstants.HouseholdRoles.Admin &&
                           hm.DeletedAt == null, cancellationToken);
    }

    public async Task<PagedResult<HouseholdEntity>> SearchHouseholdsAsync(
        string? searchTerm,
        int? planTypeId,
        string? subscriptionStatus,
        bool? hasActiveMembers,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Query()
            .Include(h => h.PlanType)
            .Include(h => h.HouseholdMembers.Where(hm => hm.DeletedAt == null))
            .Include(h => h.Items.Where(i => i.DeletedAt == null))
            .Include(h => h.Tasks.Where(t => t.DeletedAt == null))
            .AsQueryable();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(h =>
                h.Name.Contains(searchTerm) ||
                (h.Address != null && h.Address.Contains(searchTerm)));
        }

        // Apply plan type filter
        if (planTypeId.HasValue)
        {
            query = query.Where(h => h.PlanTypeId == planTypeId.Value);
        }

        // Apply subscription status filter
        if (!string.IsNullOrWhiteSpace(subscriptionStatus))
        {
            query = query.Where(h => h.SubscriptionStatus == subscriptionStatus);
        }

        // Apply active members filter
        if (hasActiveMembers.HasValue && hasActiveMembers.Value)
        {
            query = query.Where(h => h.HouseholdMembers.Any(hm => hm.DeletedAt == null));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<HouseholdEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<HouseholdStatsResult> GetHouseholdStatsAsync(CancellationToken cancellationToken = default)
    {
        var households = await Query()
            .Include(h => h.HouseholdMembers.Where(hm => hm.DeletedAt == null))
            .Include(h => h.Items.Where(i => i.DeletedAt == null))
            .Include(h => h.Tasks.Where(t => t.DeletedAt == null))
            .ToListAsync(cancellationToken);

        return new HouseholdStatsResult
        {
            TotalHouseholds = households.Count,
            ActiveHouseholds = households.Count(h => h.HouseholdMembers.Any(hm => hm.DeletedAt == null)),
            FreeHouseholds = households.Count(h => h.SubscriptionStatus == "free"),
            PremiumHouseholds = households.Count(h => h.SubscriptionStatus == "active"),
            TotalMembers = households.Sum(h => h.HouseholdMembers.Count(hm => hm.DeletedAt == null)),
            TotalItems = households.Sum(h => h.Items.Count(i => i.DeletedAt == null)),
            TotalTasks = households.Sum(h => h.Tasks.Count(t => t.DeletedAt == null))
        };
    }
}
