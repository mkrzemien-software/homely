using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class PlanTypeRepository : BaseRepository<PlanTypeEntity, int>, IPlanTypeRepository
{
    public PlanTypeRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<PlanTypeEntity, bool>> GetIdPredicate(int id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<PlanTypeEntity>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceMonthly ?? 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanTypeEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await GetFirstAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<bool> CheckPlanLimitsAsync(int planTypeId, Guid householdId, string limitType, CancellationToken cancellationToken = default)
    {
        var planType = await GetByIdAsync(planTypeId, cancellationToken);
        if (planType == null) return false;

        return limitType switch
        {
            DatabaseConstants.PlanUsageTypes.Tasks => await CheckItemsLimitAsync(planType, householdId, cancellationToken),
            DatabaseConstants.PlanUsageTypes.HouseholdMembers => await CheckMembersLimitAsync(planType, householdId, cancellationToken),
            _ => true
        };
    }

    private async Task<bool> CheckItemsLimitAsync(PlanTypeEntity planType, Guid householdId, CancellationToken cancellationToken)
    {
        if (planType.MaxTasks == null) return true; // Unlimited

        var currentCount = await Context.Set<TaskEntity>()
            .CountAsync(t => t.HouseholdId == householdId &&
                           t.DeletedAt == null &&
                           t.IsActive, cancellationToken);

        return currentCount < planType.MaxTasks.Value;
    }

    private async Task<bool> CheckMembersLimitAsync(PlanTypeEntity planType, Guid householdId, CancellationToken cancellationToken)
    {
        if (planType.MaxHouseholdMembers == null) return true; // Unlimited

        var currentCount = await Context.Set<HouseholdMemberEntity>()
            .CountAsync(hm => hm.HouseholdId == householdId && 
                             hm.DeletedAt == null, cancellationToken);

        return currentCount < planType.MaxHouseholdMembers.Value;
    }
}
