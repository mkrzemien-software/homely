using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Models.ViewModels;

namespace Homely.API.Repositories.Interfaces;

public interface ITaskRepository : IBaseRepository<TaskEntity, Guid>
{
    Task<IEnumerable<TaskEntity>> GetHouseholdTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskEntity>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskEntity>> GetByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default);

    Task<IEnumerable<DashboardUpcomingTaskViewModel>> GetUpcomingTasksAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskEntity>> GetDueTasksAsync(Guid householdId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskEntity>> GetOverdueTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskEntity>> GetItemTasksAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<TaskEntity?> GetWithDetailsAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<bool> CanUserAccessTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
