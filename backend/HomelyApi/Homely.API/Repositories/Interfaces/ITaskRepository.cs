using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

/// <summary>
/// Repository for managing Tasks (task templates).
/// Tasks represent templates that define "what" and "how often" (if recurring).
/// Events (concrete occurrences) are created from these templates.
/// </summary>
public interface ITaskRepository : IBaseRepository<TaskEntity, Guid>
{
    /// <summary>
    /// Get all tasks (templates) for a specific household
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetHouseholdTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks by category
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks by category for a specific household
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetByCategoryAsync(
        Guid householdId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active tasks only
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetActiveTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get task with full details including category and category type
    /// </summary>
    Task<TaskEntity?> GetWithDetailsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has access to the task (through household membership)
    /// </summary>
    Task<bool> CanUserAccessTaskAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count active tasks for a household (for plan limit checking)
    /// </summary>
    Task<int> CountActiveTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks with interval (recurring tasks)
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetRecurringTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks without interval (one-time tasks)
    /// </summary>
    Task<IEnumerable<TaskEntity>> GetOneTimeTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);
}
