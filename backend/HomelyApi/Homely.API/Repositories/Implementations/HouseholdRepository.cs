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
}
