using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IPlanUsageRepository : IBaseRepository<PlanUsageEntity, Guid>
{
    Task<IEnumerable<PlanUsageEntity>> GetCurrentUsageAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<PlanUsageEntity?> GetUsageByTypeAsync(Guid householdId, string usageType, DateOnly? date = null, CancellationToken cancellationToken = default);

    Task<PlanUsageEntity> UpdateOrCreateUsageAsync(Guid householdId, string usageType, int currentValue, int? maxValue = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<PlanUsageEntity>> GetUsageHistoryAsync(Guid householdId, string usageType, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<bool> IsLimitExceededAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default);

    Task<Dictionary<string, double>> GetUsagePercentageAsync(Guid householdId, CancellationToken cancellationToken = default);
}
