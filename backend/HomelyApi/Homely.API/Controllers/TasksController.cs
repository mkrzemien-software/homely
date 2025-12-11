using Homely.API.Models.DTOs.Tasks;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Task templates management controller
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IEventService _eventService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskService taskService,
        IEventService eventService,
        ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Get all task templates for a household with pagination
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="activeOnly">Filter for active tasks only (default: true)</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="recurringOnly">Filter for recurring tasks only</param>
    /// <param name="oneTimeOnly">Filter for one-time tasks only</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of task templates</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TasksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TasksResponse>> GetTasks(
        [FromQuery] Guid householdId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool recurringOnly = false,
        [FromQuery] bool oneTimeOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            if (recurringOnly && oneTimeOnly)
            {
                return BadRequest(new { error = "Cannot filter for both recurring and one-time tasks simultaneously" });
            }

            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (limit < 1 || limit > 100)
            {
                return BadRequest(new { error = "Limit must be between 1 and 100" });
            }

            IEnumerable<TaskDto> allTasks;

            if (recurringOnly)
            {
                allTasks = await _taskService.GetRecurringTasksAsync(householdId, cancellationToken);
            }
            else if (oneTimeOnly)
            {
                allTasks = await _taskService.GetOneTimeTasksAsync(householdId, cancellationToken);
            }
            else if (categoryId.HasValue)
            {
                allTasks = await _taskService.GetTasksByCategoryAsync(householdId, categoryId.Value, cancellationToken);
            }
            else if (activeOnly)
            {
                allTasks = await _taskService.GetActiveTasksAsync(householdId, cancellationToken);
            }
            else
            {
                allTasks = await _taskService.GetHouseholdTasksAsync(householdId, cancellationToken);
            }

            var tasksList = allTasks.ToList();
            var totalItems = tasksList.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / limit);

            // Apply pagination
            var paginatedTasks = tasksList
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var response = new TasksResponse
            {
                Data = paginatedTasks,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    ItemsPerPage = limit
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Get active task templates for a household with pagination
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active task templates</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(TasksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TasksResponse>> GetActiveTasks(
        [FromQuery] Guid householdId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Redirect to main GetTasks method with activeOnly=true
        return await GetTasks(householdId, activeOnly: true, page: page, limit: limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get recurring task templates for a household with pagination
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of recurring task templates</returns>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(TasksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TasksResponse>> GetRecurringTasks(
        [FromQuery] Guid householdId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Redirect to main GetTasks method with recurringOnly=true
        return await GetTasks(householdId, recurringOnly: true, page: page, limit: limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get one-time task templates for a household with pagination
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of one-time task templates</returns>
    [HttpGet("one-time")]
    [ProducesResponseType(typeof(TasksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TasksResponse>> GetOneTimeTasks(
        [FromQuery] Guid householdId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Redirect to main GetTasks method with oneTimeOnly=true
        return await GetTasks(householdId, oneTimeOnly: true, page: page, limit: limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get task template by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task template details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> GetTaskById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);

            if (task == null)
            {
                return NotFound(new { error = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving task" });
        }
    }

    /// <summary>
    /// Create new task template
    /// </summary>
    /// <param name="createDto">Task creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task template</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/tasks
    ///     {
    ///         "householdId": "00000000-0000-0000-0000-000000000000",
    ///         "categoryId": 1,
    ///         "name": "Boiler Inspection",
    ///         "description": "Annual boiler maintenance and safety check",
    ///         "yearsValue": 1,
    ///         "monthsValue": 0,
    ///         "weeksValue": 0,
    ///         "daysValue": 0,
    ///         "priority": "high",
    ///         "notes": "Schedule with certified technician",
    ///         "isActive": true,
    ///         "createdBy": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// For one-time tasks, set all interval values to 0 or null.
    /// For recurring tasks, set at least one interval value greater than 0.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _taskService.CreateTaskAsync(createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = task.Id },
                task);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating task");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating task");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { error = "An error occurred while creating task" });
        }
    }

    /// <summary>
    /// Update existing task template
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateDto">Updated task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task template</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/tasks/00000000-0000-0000-0000-000000000000
    ///     {
    ///         "categoryId": 1,
    ///         "name": "Updated Boiler Inspection",
    ///         "description": "Updated description",
    ///         "yearsValue": 1,
    ///         "monthsValue": 0,
    ///         "weeksValue": 0,
    ///         "daysValue": 0,
    ///         "priority": "medium",
    ///         "notes": "Updated notes",
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _taskService.UpdateTaskAsync(id, updateDto, cancellationToken);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating task");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while updating task" });
        }
    }

    /// <summary>
    /// Delete task template (soft delete)
    /// </summary>
    /// <param name="id">Task ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTask(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _taskService.DeleteTaskAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Task with ID {id} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Task template deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting task" });
        }
    }

    /// <summary>
    /// Count active tasks for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active tasks</returns>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CountActiveTasks(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var count = await _taskService.CountActiveTasksAsync(householdId, cancellationToken);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting active tasks for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while counting active tasks" });
        }
    }

    /// <summary>
    /// Regenerate all future events for a task template.
    /// Deletes all pending future events and creates a new series based on current task interval.
    /// Useful when task interval has been modified or for manual event refresh.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of events generated</returns>
    [HttpPost("{id}/regenerate-events")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> RegenerateEvents(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventsGenerated = await _eventService.RegenerateEventsForTaskAsync(id, cancellationToken);

            return Ok(new
            {
                success = true,
                eventsGenerated = eventsGenerated,
                message = $"Successfully regenerated {eventsGenerated} events for task {id}"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while regenerating events for task {TaskId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating events for task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while regenerating events" });
        }
    }
}
