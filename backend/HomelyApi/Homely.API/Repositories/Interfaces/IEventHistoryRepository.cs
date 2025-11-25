using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IEventHistoryRepository : IBaseRepository<EventHistoryEntity, Guid>
{
    Task<IEnumerable<EventHistoryEntity>> GetHouseholdHistoryAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventHistoryEntity>> GetTaskTemplateHistoryAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventHistoryEntity>> GetCompletedByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventHistoryEntity>> GetHistoryInDateRangeAsync(Guid householdId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetCompletionStatsAsync(Guid householdId, int lastDays = 30, CancellationToken cancellationToken = default);
}

