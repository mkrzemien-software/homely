using Homely.API.Models.DTOs.Tasks;
using Homely.API.Repositories.Base;
using Homely.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for task management
/// </summary>
public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IUnitOfWork unitOfWork,
        ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetHouseholdTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetHouseholdTasksAsync(householdId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetAssignedTasksAsync(userId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetByStatusAsync(householdId, status, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with status {Status} for household {HouseholdId}", status, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
            var tasks = await _unitOfWork.Tasks.GetDueTasksAsync(householdId, DateOnly.FromDateTime(DateTime.UtcNow), toDate, cancellationToken);

            return tasks
                .Where(t => t.DeletedAt == null && t.Status == "pending")
                .OrderBy(t => t.DueDate)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetOverdueTasksAsync(householdId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetWithDetailsAsync(taskId, cancellationToken);

            if (task == null || task.DeletedAt != null)
            {
                return null;
            }

            return MapToDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = new TaskEntity
            {
                ItemId = createDto.ItemId,
                HouseholdId = createDto.HouseholdId,
                AssignedTo = createDto.AssignedTo,
                DueDate = createDto.DueDate,
                Title = createDto.Title,
                Description = createDto.Description,
                Status = "pending",
                Priority = createDto.Priority,
                IsRecurring = createDto.IsRecurring,
                CreatedBy = createDto.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task created with ID {TaskId} for household {HouseholdId}", task.Id, task.HouseholdId);

            return MapToDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken);

            if (task == null || task.DeletedAt != null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            task.AssignedTo = updateDto.AssignedTo;
            task.DueDate = updateDto.DueDate;
            task.Title = updateDto.Title;
            task.Description = updateDto.Description;
            task.Status = updateDto.Status;
            task.Priority = updateDto.Priority;
            task.IsRecurring = updateDto.IsRecurring;
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} updated successfully", taskId);

            return MapToDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken);

            if (task == null || task.DeletedAt != null)
            {
                return false;
            }

            // Soft delete
            task.DeletedAt = DateTimeOffset.UtcNow;
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} soft deleted successfully", taskId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TaskDto> CompleteTaskAsync(Guid taskId, CompleteTaskDto completeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Start transaction for atomic completion and recurring task creation
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var task = await _unitOfWork.Tasks.GetWithDetailsAsync(taskId, cancellationToken);

            if (task == null || task.DeletedAt != null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            if (task.Status == "completed")
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new InvalidOperationException($"Task {taskId} is already completed");
            }

            // Mark task as completed
            task.Status = "completed";
            task.CompletionDate = completeDto.CompletionDate;
            task.CompletionNotes = completeDto.CompletionNotes;
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);

            _logger.LogInformation("Task {TaskId} marked as completed", taskId);

            // If task is recurring and has an associated item, create next recurring task
            if (task.IsRecurring && task.ItemId.HasValue)
            {
                await CreateNextRecurringTaskAsync(task, completeDto.CompletionDate, cancellationToken);
            }

            // TODO: Archive to task_history if household has premium plan
            // This would require checking household's plan type and adding to tasks_history table

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TaskDto> PostponeTaskAsync(Guid taskId, PostponeTaskDto postponeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken);

            if (task == null || task.DeletedAt != null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            if (task.Status == "completed")
            {
                throw new InvalidOperationException($"Cannot postpone completed task {taskId}");
            }

            // Validate new due date is in the future
            if (postponeDto.NewDueDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("New due date must be in the future");
            }

            // Save original due date if not already postponed
            if (task.PostponedFromDate == null)
            {
                task.PostponedFromDate = task.DueDate;
            }

            // Update task with new due date and reason
            task.DueDate = postponeDto.NewDueDate;
            task.PostponeReason = postponeDto.PostponeReason;
            task.Status = "postponed";
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} postponed to {NewDueDate}", taskId, postponeDto.NewDueDate);

            return MapToDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing task {TaskId}", taskId);
            throw;
        }
    }

    /// <summary>
    /// Creates next recurring task based on item's interval settings.
    /// This method is called within a transaction context and does not commit changes.
    /// </summary>
    private async Task CreateNextRecurringTaskAsync(TaskEntity completedTask, DateOnly completionDate, CancellationToken cancellationToken)
    {
        try
        {
            if (!completedTask.ItemId.HasValue)
            {
                return;
            }

            // Get item with interval settings
            var item = await _unitOfWork.Items.GetByIdAsync(completedTask.ItemId.Value, cancellationToken);

            if (item == null || item.DeletedAt != null)
            {
                _logger.LogWarning("Cannot create recurring task: Item {ItemId} not found", completedTask.ItemId.Value);
                return;
            }

            // Calculate next due date based on item's interval
            var nextDueDate = CalculateNextDueDate(completionDate, item);

            // Create new task
            var newTask = new TaskEntity
            {
                ItemId = completedTask.ItemId,
                HouseholdId = completedTask.HouseholdId,
                AssignedTo = completedTask.AssignedTo,
                DueDate = nextDueDate,
                Title = completedTask.Title,
                Description = completedTask.Description,
                Status = "pending",
                Priority = item.Priority, // Use item's priority
                IsRecurring = true,
                CreatedBy = completedTask.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(newTask, cancellationToken);
            // Note: SaveChanges is NOT called here - transaction will be committed by the caller (CompleteTaskAsync)

            _logger.LogInformation(
                "Created recurring task {NewTaskId} for item {ItemId} with due date {DueDate}",
                newTask.Id, item.Id, nextDueDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring task for completed task {TaskId}", completedTask.Id);
            throw; // Re-throw to rollback the entire transaction
        }
    }

    /// <summary>
    /// Calculates next due date based on item's interval settings
    /// </summary>
    private static DateOnly CalculateNextDueDate(DateOnly completionDate, ItemEntity item)
    {
        var nextDate = completionDate;

        // Add years
        if (item.YearsValue.HasValue && item.YearsValue.Value > 0)
        {
            nextDate = nextDate.AddYears(item.YearsValue.Value);
        }

        // Add months
        if (item.MonthsValue.HasValue && item.MonthsValue.Value > 0)
        {
            nextDate = nextDate.AddMonths(item.MonthsValue.Value);
        }

        // Add weeks (convert to days)
        if (item.WeeksValue.HasValue && item.WeeksValue.Value > 0)
        {
            nextDate = nextDate.AddDays(item.WeeksValue.Value * 7);
        }

        // Add days
        if (item.DaysValue.HasValue && item.DaysValue.Value > 0)
        {
            nextDate = nextDate.AddDays(item.DaysValue.Value);
        }

        return nextDate;
    }

    /// <summary>
    /// Maps TaskEntity to TaskDto
    /// </summary>
    private static TaskDto MapToDto(TaskEntity entity)
    {
        return new TaskDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            ItemName = entity.Item?.Name,
            HouseholdId = entity.HouseholdId,
            HouseholdName = entity.Household?.Name,
            AssignedTo = entity.AssignedTo,
            DueDate = entity.DueDate,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            CompletionDate = entity.CompletionDate,
            CompletionNotes = entity.CompletionNotes,
            PostponedFromDate = entity.PostponedFromDate,
            PostponeReason = entity.PostponeReason,
            IsRecurring = entity.IsRecurring,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
