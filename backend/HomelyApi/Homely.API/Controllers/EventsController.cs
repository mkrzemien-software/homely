using Homely.API.Models.DTOs;
using Homely.API.Models.DTOs.Tasks;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Homely.API.Controllers;

/// <summary>
/// Events management controller
/// </summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Get all events for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="startDate">Optional start date filter (YYYY-MM-DD)</param>
    /// <param name="endDate">Optional end date filter (YYYY-MM-DD)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EventDto>>>> GetEvents(
        [FromQuery] Guid householdId,
        [FromQuery] string? status = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Household ID is required",
                    StatusCodes.Status400BadRequest));
            }

            IEnumerable<EventDto> events;

            // Parse dates if provided
            DateOnly? parsedStartDate = null;
            DateOnly? parsedEndDate = null;

            if (!string.IsNullOrEmpty(startDate))
            {
                if (!DateOnly.TryParse(startDate, out var sd))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid start date format. Use YYYY-MM-DD",
                        StatusCodes.Status400BadRequest));
                }
                parsedStartDate = sd;
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                if (!DateOnly.TryParse(endDate, out var ed))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid end date format. Use YYYY-MM-DD",
                        StatusCodes.Status400BadRequest));
                }
                parsedEndDate = ed;
            }

            if (!string.IsNullOrEmpty(status))
            {
                events = await _eventService.GetEventsByStatusAsync(householdId, status, cancellationToken);
                
                // Apply date filtering if specified
                if (parsedStartDate.HasValue || parsedEndDate.HasValue)
                {
                    events = events.Where(e => 
                        (!parsedStartDate.HasValue || e.DueDate >= parsedStartDate.Value) &&
                        (!parsedEndDate.HasValue || e.DueDate <= parsedEndDate.Value));
                }
            }
            else
            {
                events = await _eventService.GetHouseholdEventsAsync(householdId, parsedStartDate, parsedEndDate, cancellationToken);
            }

            return Ok(ApiResponseDto<IEnumerable<EventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for household {HouseholdId}", householdId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving events",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get upcoming events for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="days">Number of days to look ahead (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of upcoming events</returns>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EventDto>>>> GetUpcomingEvents(
        [FromQuery] Guid householdId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Household ID is required",
                    StatusCodes.Status400BadRequest));
            }

            if (days < 1 || days > 365)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Days must be between 1 and 365",
                    StatusCodes.Status400BadRequest));
            }

            var events = await _eventService.GetUpcomingEventsAsync(householdId, days, cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<EventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming events for household {HouseholdId}", householdId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving upcoming events",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get overdue events for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of overdue events</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EventDto>>>> GetOverdueEvents(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Household ID is required",
                    StatusCodes.Status400BadRequest));
            }

            var events = await _eventService.GetOverdueEventsAsync(householdId, cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<EventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue events for household {HouseholdId}", householdId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving overdue events",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get events assigned to a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assigned tasks</returns>
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<EventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EventDto>>>> GetAssignedEvents(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "User ID is required",
                    StatusCodes.Status400BadRequest));
            }

            var events = await _eventService.GetAssignedEventsAsync(userId, cancellationToken);
            return Ok(ApiResponseDto<IEnumerable<EventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving assigned tasks",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> GetEventById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventDto = await _eventService.GetEventByIdAsync(id, cancellationToken);

            if (@eventDto == null)
            {
                return NotFound(ApiResponseDto<object>.ErrorResponse(
                    $"Event with ID {id} not found",
                    StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponseDto<EventDto>.SuccessResponse(eventDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Create new event
    /// </summary>
    /// <param name="createDto">Task creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events
    ///     {
    ///         "taskId": "00000000-0000-0000-0000-000000000000",
    ///         "householdId": "00000000-0000-0000-0000-000000000000",
    ///         "assignedTo": "00000000-0000-0000-0000-000000000000",
    ///         "dueDate": "2025-12-31",
    ///         "priority": "high",
    ///         "notes": "Optional notes for this event",
    ///         "createdBy": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// NOTE: Events do NOT have their own title - they display the task's name.
    /// taskId is REQUIRED - events must be based on a task template.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> CreateEvent(
        [FromBody] CreateEventDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    errors));
            }

            var eventDto = await _eventService.CreateEventAsync(createDto, cancellationToken);
            var response = ApiResponseDto<EventDto>.SuccessResponse(eventDto, StatusCodes.Status201Created);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = eventDto.Id },
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while creating event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Update existing event
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateDto">Updated task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/events/00000000-0000-0000-0000-000000000000
    ///     {
    ///         "assignedTo": "00000000-0000-0000-0000-000000000000",
    ///         "dueDate": "2025-12-31",
    ///         "status": "pending",
    ///         "priority": "high",
    ///         "notes": "Updated notes"
    ///     }
    ///
    /// NOTE: Events cannot change their task association - title comes from the task.
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    errors));
            }

            var eventDto = await _eventService.UpdateEventAsync(id, updateDto, cancellationToken);
            return Ok(ApiResponseDto<EventDto>.SuccessResponse(eventDto));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating event");
            return NotFound(ApiResponseDto<object>.ErrorResponse(
                ex.Message,
                StatusCodes.Status404NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while updating event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Delete task (soft delete)
    /// </summary>
    /// <param name="id">Task ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteEvent(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _eventService.DeleteEventAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(ApiResponseDto<object>.ErrorResponse(
                    $"Event with ID {id} not found",
                    StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(new
            {
                message = "Task deleted successfully"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while deleting event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Mark event as completed. If task is recurring and linked to an item,
    /// automatically creates next recurring task based on item's interval.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="completeDto">Completion data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/00000000-0000-0000-0000-000000000000/complete
    ///     {
    ///         "completionDate": "2025-11-15",
    ///         "completionNotes": "Boiler inspection completed successfully. All systems working properly.",
    ///         "completedBy": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// If task is recurring and has an associated item, a new event will be automatically
    /// created with due date calculated from item's interval (years/months/weeks/days).
    /// </remarks>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> CompleteEvent(
        Guid id,
        [FromBody] CompleteEventDto completeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    errors));
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var completedBy))
            {
                return Unauthorized(ApiResponseDto<object>.ErrorResponse(
                    "User not authenticated",
                    StatusCodes.Status401Unauthorized));
            }

            var eventDto = await _eventService.CompleteEventAsync(id, completeDto, completedBy, cancellationToken);
            return Ok(ApiResponseDto<EventDto>.SuccessResponse(eventDto));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while completing event");
            return BadRequest(ApiResponseDto<object>.ErrorResponse(
                ex.Message,
                StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while completing event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Postpone event to a new due date with a reason
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="postponeDto">Postpone data with new due date and reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Postponed task</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/00000000-0000-0000-0000-000000000000/postpone
    ///     {
    ///         "newDueDate": "2025-12-31",
    ///         "postponeReason": "Waiting for replacement parts to arrive"
    ///     }
    ///
    /// The original due date will be preserved in postponedFromDate field.
    /// Task status will be changed to 'postponed'.
    /// </remarks>
    [HttpPost("{id}/postpone")]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> PostponeEvent(
        Guid id,
        [FromBody] PostponeEventDto postponeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    errors));
            }

            var eventDto = await _eventService.PostponeEventAsync(id, postponeDto, cancellationToken);
            return Ok(ApiResponseDto<EventDto>.SuccessResponse(eventDto));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while postponing event");
            return BadRequest(ApiResponseDto<object>.ErrorResponse(
                ex.Message,
                StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while postponing event",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Cancel an event with a reason
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="cancelDto">Cancellation data with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled event</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/events/00000000-0000-0000-0000-000000000000/cancel
    ///     {
    ///         "cancelReason": "Appointment no longer needed"
    ///     }
    ///
    /// Event status will be changed to 'cancelled'.
    /// Cancel reason will be stored in the notes field with [CANCELLED] prefix.
    /// </remarks>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponseDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> CancelEvent(
        Guid id,
        [FromBody] CancelEventDto cancelDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    errors));
            }

            var eventDto = await _eventService.CancelEventAsync(id, cancelDto, cancellationToken);
            return Ok(ApiResponseDto<EventDto>.SuccessResponse(eventDto));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while cancelling event");
            return BadRequest(ApiResponseDto<object>.ErrorResponse(
                ex.Message,
                StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling event {EventId}", id);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while cancelling event",
                StatusCodes.Status500InternalServerError));
        }
    }
}
