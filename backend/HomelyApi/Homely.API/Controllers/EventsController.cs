using Homely.API.Models.DTOs.Tasks;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
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

            IEnumerable<EventDto> events;

            if (!string.IsNullOrEmpty(status))
            {
                events = await _eventService.GetEventsByStatusAsync(householdId, status, cancellationToken);
            }
            else
            {
                events = await _eventService.GetHouseholdEventsAsync(householdId, cancellationToken);
            }

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving events" });
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
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcomingEvents(
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

            var events = await _eventService.GetUpcomingEventsAsync(householdId, days, cancellationToken);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming events for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving upcoming events" });
        }
    }

    /// <summary>
    /// Get overdue events for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of overdue events</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetOverdueEvents(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var events = await _eventService.GetOverdueEventsAsync(householdId, cancellationToken);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue events for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving overdue events" });
        }
    }

    /// <summary>
    /// Get events assigned to a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assigned tasks</returns>
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAssignedEvents(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { error = "User ID is required" });
            }

            var events = await _eventService.GetAssignedEventsAsync(userId, cancellationToken);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
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
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventDto>> GetEventById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventDto = await _eventService.GetEventByIdAsync(id, cancellationToken);

            if (@eventDto == null)
            {
                return NotFound(new { error = $"Event with ID {id} not found" });
            }

            return Ok(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving event" });
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
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventDto>> CreateEvent(
        [FromBody] CreateEventDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDto = await _eventService.CreateEventAsync(createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = eventDto.Id },
                eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(500, new { error = "An error occurred while creating event" });
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
    ///         "title": "Updated Task Title",
    ///         "description": "Updated description",
    ///         "status": "pending",
    ///         "priority": "high",
    ///         "isRecurring": true
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventDto>> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDto = await _eventService.UpdateEventAsync(id, updateDto, cancellationToken);
            return Ok(eventDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating event");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred while updating event" });
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
    public async Task<ActionResult> DeleteEvent(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _eventService.DeleteEventAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Event with ID {id} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Task deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting event" });
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
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventDto>> CompleteEvent(
        Guid id,
        [FromBody] CompleteEventDto completeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDto = await _eventService.CompleteEventAsync(id, completeDto, cancellationToken);
            return Ok(eventDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while completing event");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred while completing event" });
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
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EventDto>> PostponeEvent(
        Guid id,
        [FromBody] PostponeEventDto postponeDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDto = await _eventService.PostponeEventAsync(id, postponeDto, cancellationToken);
            return Ok(eventDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while postponing event");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred while postponing event" });
        }
    }
}
