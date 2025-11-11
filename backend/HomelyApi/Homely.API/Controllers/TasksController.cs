using Homely.API.Models.DTOs.Tasks;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Tasks management controller
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskService taskService,
        ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tasks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        [FromQuery] Guid householdId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            IEnumerable<TaskDto> tasks;

            if (!string.IsNullOrEmpty(status))
            {
                tasks = await _taskService.GetTasksByStatusAsync(householdId, status, cancellationToken);
            }
            else
            {
                tasks = await _taskService.GetHouseholdTasksAsync(householdId, cancellationToken);
            }

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Get upcoming tasks for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="days">Number of days to look ahead (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of upcoming tasks</returns>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetUpcomingTasks(
        [FromQuery] Guid householdId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            if (days < 1 || days > 365)
            {
                return BadRequest(new { error = "Days must be between 1 and 365" });
            }

            var tasks = await _taskService.GetUpcomingTasksAsync(householdId, days, cancellationToken);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming tasks for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving upcoming tasks" });
        }
    }

    /// <summary>
    /// Get overdue tasks for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of overdue tasks</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var tasks = await _taskService.GetOverdueTasksAsync(householdId, cancellationToken);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving overdue tasks" });
        }
    }

    /// <summary>
    /// Get tasks assigned to a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assigned tasks</returns>
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAssignedTasks(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { error = "User ID is required" });
            }

            var tasks = await _taskService.GetAssignedTasksAsync(userId, cancellationToken);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving assigned tasks" });
        }
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task details</returns>
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
    /// Create new task
    /// </summary>
    /// <param name="createDto">Task creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/tasks
    ///     {
    ///         "itemId": "00000000-0000-0000-0000-000000000000",
    ///         "householdId": "00000000-0000-0000-0000-000000000000",
    ///         "assignedTo": "00000000-0000-0000-0000-000000000000",
    ///         "dueDate": "2025-12-31",
    ///         "title": "Annual Boiler Inspection",
    ///         "description": "Schedule yearly maintenance check",
    ///         "priority": "high",
    ///         "isRecurring": true,
    ///         "createdBy": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { error = "An error occurred while creating task" });
        }
    }

    /// <summary>
    /// Update existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateDto">Updated task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/tasks/00000000-0000-0000-0000-000000000000
    ///     {
    ///         "assignedTo": "00000000-0000-0000-0000-000000000000",
    ///         "dueDate": "2025-12-31",
    ///         "title": "Updated Task Title",
    ///         "description": "Updated description",
    ///         "status": "pending",
    ///         "priority": "high",
    ///         "isRecurring": true
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
    /// Delete task (soft delete)
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
                message = "Task deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting task" });
        }
    }

    /// <summary>
    /// Mark task as completed. If task is recurring and linked to an item,
    /// automatically creates next recurring task based on item's interval.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="completeDto">Completion data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/tasks/00000000-0000-0000-0000-000000000000/complete
    ///     {
    ///         "completionDate": "2025-11-15",
    ///         "completionNotes": "Boiler inspection completed successfully. All systems working properly.",
    ///         "completedBy": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// If task is recurring and has an associated item, a new task will be automatically
    /// created with due date calculated from item's interval (years/months/weeks/days).
    /// </remarks>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> CompleteTask(
        Guid id,
        [FromBody] CompleteTaskDto completeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _taskService.CompleteTaskAsync(id, completeDto, cancellationToken);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while completing task");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while completing task" });
        }
    }

    /// <summary>
    /// Postpone task to a new due date with a reason
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="postponeDto">Postpone data with new due date and reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Postponed task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/tasks/00000000-0000-0000-0000-000000000000/postpone
    ///     {
    ///         "newDueDate": "2025-12-31",
    ///         "postponeReason": "Waiting for replacement parts to arrive"
    ///     }
    ///
    /// The original due date will be preserved in postponedFromDate field.
    /// Task status will be changed to 'postponed'.
    /// </remarks>
    [HttpPost("{id}/postpone")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> PostponeTask(
        Guid id,
        [FromBody] PostponeTaskDto postponeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await _taskService.PostponeTaskAsync(id, postponeDto, cancellationToken);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while postponing task");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing task {TaskId}", id);
            return StatusCode(500, new { error = "An error occurred while postponing task" });
        }
    }
}
