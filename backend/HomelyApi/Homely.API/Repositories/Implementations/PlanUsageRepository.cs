using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class PlanUsageRepository : BaseRepository<PlanUsageEntity, Guid>, IPlanUsageRepository
{
    public PlanUsageRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<PlanUsageEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<PlanUsageEntity>> GetCurrentUsageAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        return await GetWhereAsync(pu => pu.HouseholdId == householdId && pu.UsageDate == today,
            pu => pu.Household,
            pu => pu.Household.PlanType);
    }

    public async Task<PlanUsageEntity?> GetUsageByTypeAsync(Guid householdId, string usageType, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        date ??= DateOnly.FromDateTime(DateTime.UtcNow);
        
        return await GetFirstAsync(pu => pu.HouseholdId == householdId && 
                                         pu.UsageType == usageType && 
                                         pu.UsageDate == date.Value, cancellationToken);
    }

    public async Task<PlanUsageEntity> UpdateOrCreateUsageAsync(Guid householdId, string usageType, int currentValue, int? maxValue = null, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var existingUsage = await GetUsageByTypeAsync(householdId, usageType, today, cancellationToken);
        
        if (existingUsage != null)
        {
            existingUsage.CurrentValue = currentValue;
            existingUsage.MaxValue = maxValue ?? existingUsage.MaxValue;
            existingUsage.UpdatedAt = DateTimeOffset.UtcNow;
            
            await UpdateAsync(existingUsage, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            
            return existingUsage;
        }
        else
        {
            var newUsage = new PlanUsageEntity
            {
                HouseholdId = householdId,
                UsageType = usageType,
                CurrentValue = currentValue,
                MaxValue = maxValue,
                UsageDate = today
            };
            
            await AddAsync(newUsage, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            
            return newUsage;
        }
    }

    public async Task<IEnumerable<PlanUsageEntity>> GetUsageHistoryAsync(Guid householdId, string usageType, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(pu => pu.HouseholdId == householdId &&
                        pu.UsageType == usageType &&
                        pu.UsageDate >= fromDate &&
                        pu.UsageDate <= toDate)
            .OrderBy(pu => pu.UsageDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLimitExceededAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default)
    {
        var usage = await GetUsageByTypeAsync(householdId, usageType, cancellationToken: cancellationToken);
        
        if (usage?.MaxValue == null) return false; // Unlimited
        
        return usage.CurrentValue >= usage.MaxValue.Value;
    }

    public async Task<Dictionary<string, double>> GetUsagePercentageAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        var currentUsages = await GetCurrentUsageAsync(householdId, cancellationToken);
        var percentages = new Dictionary<string, double>();
        
        foreach (var usage in currentUsages)
        {
            if (usage.MaxValue.HasValue && usage.MaxValue.Value > 0)
            {
                var percentage = (double)usage.CurrentValue / usage.MaxValue.Value * 100;
                percentages[usage.UsageType] = Math.Round(percentage, 2);
            }
            else
            {
                percentages[usage.UsageType] = 0; // Unlimited = 0%
            }
        }
        
        return percentages;
    }
}
