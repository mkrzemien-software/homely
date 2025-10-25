using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IPlanTypeRepository : IBaseRepository<PlanTypeEntity, int>
{
    Task<IEnumerable<PlanTypeEntity>> GetActivePlansAsync(CancellationToken cancellationToken = default);

    Task<PlanTypeEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> CheckPlanLimitsAsync(int planTypeId, Guid householdId, string limitType, CancellationToken cancellationToken = default);
}
