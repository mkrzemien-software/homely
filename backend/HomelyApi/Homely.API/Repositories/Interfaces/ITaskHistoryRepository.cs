using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface ITaskHistoryRepository : IBaseRepository<TaskHistoryEntity, Guid>
{
    Task<IEnumerable<TaskHistoryEntity>> GetHouseholdHistoryAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskHistoryEntity>> GetTaskTemplateHistoryAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskHistoryEntity>> GetCompletedByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskHistoryEntity>> GetHistoryInDateRangeAsync(Guid householdId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetCompletionStatsAsync(Guid householdId, int lastDays = 30, CancellationToken cancellationToken = default);
}
