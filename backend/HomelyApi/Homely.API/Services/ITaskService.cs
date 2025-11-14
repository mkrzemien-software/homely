using Homely.API.Models.DTOs.Tasks;

namespace Homely.API.Services;

/// <summary>
/// Service interface for task template management
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Get all tasks for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetHouseholdTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active tasks only for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetActiveTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks by category
    /// </summary>
    Task<IEnumerable<TaskDto>> GetTasksByCategoryAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recurring tasks for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetRecurringTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get one-time tasks for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetOneTimeTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get task by ID
    /// </summary>
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new task template
    /// </summary>
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing task template
    /// </summary>
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete task template (soft delete)
    /// </summary>
    Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has access to task (through household membership)
    /// </summary>
    Task<bool> CanUserAccessTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count active tasks for a household (for plan limit checking)
    /// </summary>
    Task<int> CountActiveTasksAsync(Guid householdId, CancellationToken cancellationToken = default);
}
