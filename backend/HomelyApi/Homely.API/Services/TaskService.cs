using Homely.API.Models.DTOs.Tasks;
using Homely.API.Repositories.Base;
using Homely.API.Entities;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for task template management
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
    public async Task<IEnumerable<TaskDto>> GetActiveTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetActiveTasksAsync(householdId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetTasksByCategoryAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetByCategoryAsync(householdId, categoryId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for household {HouseholdId} and category {CategoryId}", householdId, categoryId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetRecurringTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetRecurringTasksAsync(householdId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recurring tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TaskDto>> GetOneTimeTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _unitOfWork.Tasks.GetOneTimeTasksAsync(householdId, cancellationToken);
            return tasks
                .Where(t => t.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving one-time tasks for household {HouseholdId}", householdId);
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
            // Validate that createdBy is not empty GUID
            if (createDto.CreatedBy == Guid.Empty)
            {
                throw new ArgumentException("CreatedBy cannot be an empty GUID. A valid user ID is required.");
            }

            // Verify that the user exists in user_profiles
            var userExists = await _unitOfWork.UserProfiles.GetByIdAsync(createDto.CreatedBy, cancellationToken);
            if (userExists == null)
            {
                throw new InvalidOperationException($"User with ID {createDto.CreatedBy} does not exist in user_profiles. Please ensure the user profile is created before creating tasks.");
            }

            var task = new TaskEntity
            {
                HouseholdId = createDto.HouseholdId,
                CategoryId = createDto.CategoryId,
                Name = createDto.Name,
                Description = createDto.Description,
                YearsValue = createDto.YearsValue,
                MonthsValue = createDto.MonthsValue,
                WeeksValue = createDto.WeeksValue,
                DaysValue = createDto.DaysValue,
                LastDate = createDto.LastDate,
                Priority = createDto.Priority,
                Notes = createDto.Notes,
                IsActive = createDto.IsActive,
                CreatedBy = createDto.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task created with ID {TaskId} for household {HouseholdId}", task.Id, task.HouseholdId);

            // Reload with details
            var createdTask = await _unitOfWork.Tasks.GetWithDetailsAsync(task.Id, cancellationToken);
            return MapToDto(createdTask!);
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

            task.CategoryId = updateDto.CategoryId;
            task.Name = updateDto.Name;
            task.Description = updateDto.Description;
            task.YearsValue = updateDto.YearsValue;
            task.MonthsValue = updateDto.MonthsValue;
            task.WeeksValue = updateDto.WeeksValue;
            task.DaysValue = updateDto.DaysValue;
            task.LastDate = updateDto.LastDate;
            task.Priority = updateDto.Priority;
            task.Notes = updateDto.Notes;
            task.IsActive = updateDto.IsActive;
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} updated successfully", taskId);

            // Reload with details
            var updatedTask = await _unitOfWork.Tasks.GetWithDetailsAsync(taskId, cancellationToken);
            return MapToDto(updatedTask!);
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
    public async Task<bool> CanUserAccessTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.Tasks.CanUserAccessTaskAsync(taskId, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access for task {TaskId} and user {UserId}", taskId, userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountActiveTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.Tasks.CountActiveTasksAsync(householdId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting active tasks for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <summary>
    /// Maps TaskEntity to TaskDto
    /// </summary>
    private static TaskDto MapToDto(TaskEntity entity)
    {
        // Create interval object if task has any interval values
        TaskIntervalDto? interval = null;
        if ((entity.YearsValue ?? 0) > 0 || (entity.MonthsValue ?? 0) > 0 ||
            (entity.WeeksValue ?? 0) > 0 || (entity.DaysValue ?? 0) > 0)
        {
            interval = new TaskIntervalDto
            {
                Years = entity.YearsValue ?? 0,
                Months = entity.MonthsValue ?? 0,
                Weeks = entity.WeeksValue ?? 0,
                Days = entity.DaysValue ?? 0
            };
        }

        // Create category object
        var category = new TaskCategoryDto
        {
            Id = entity.Category?.Id ?? 0,
            Name = entity.Category?.Name ?? string.Empty,
            CategoryType = new TaskCategoryTypeDto
            {
                Id = entity.Category?.CategoryType?.Id ?? 0,
                Name = entity.Category?.CategoryType?.Name ?? string.Empty
            }
        };

        // Create created by user object
        var createdByUser = new TaskUserDto
        {
            Id = entity.CreatedBy.ToString(),
            FirstName = entity.CreatedByUser?.FirstName ?? string.Empty,
            LastName = entity.CreatedByUser?.LastName ?? string.Empty
        };

        return new TaskDto
        {
            Id = entity.Id.ToString(),
            HouseholdId = entity.HouseholdId.ToString(),
            Category = category,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            Interval = interval,
            Priority = entity.Priority,
            Notes = entity.Notes,
            IsActive = entity.IsActive,
            CreatedBy = createdByUser,
            CreatedAt = entity.CreatedAt.ToString("o"), // ISO 8601 format
            UpdatedAt = entity.UpdatedAt.ToString("o") // ISO 8601 format
        };
    }
}
