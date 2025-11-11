using Homely.API.Models.DTOs.Tasks;

namespace Homely.API.Services;

/// <summary>
/// Service interface for task management
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Get all tasks for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetHouseholdTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks assigned to a specific user
    /// </summary>
    Task<IEnumerable<TaskDto>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks by status for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming tasks for a household (within specified days)
    /// </summary>
    Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue tasks for a household
    /// </summary>
    Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get task by ID
    /// </summary>
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new task
    /// </summary>
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing task
    /// </summary>
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete task (soft delete)
    /// </summary>
    Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark task as completed. If task is recurring and has an associated item,
    /// automatically creates the next recurring task based on item's interval.
    /// </summary>
    Task<TaskDto> CompleteTaskAsync(Guid taskId, CompleteTaskDto completeDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Postpone task to a new due date with a reason
    /// </summary>
    Task<TaskDto> PostponeTaskAsync(Guid taskId, PostponeTaskDto postponeDto, CancellationToken cancellationToken = default);
}
