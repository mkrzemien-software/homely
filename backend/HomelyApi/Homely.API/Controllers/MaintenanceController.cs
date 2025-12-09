using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Maintenance and system operations controller
/// Used for scheduled jobs, workflows, and administrative tasks
/// </summary>
[ApiController]
[Route("api/maintenance")]
[Produces("application/json")]
public class MaintenanceController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        IEventService eventService,
        ILogger<MaintenanceController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Refill events for a household.
    /// Checks all active tasks and generates more events if below threshold.
    /// This endpoint is typically called by scheduled GitHub Actions workflow.
    /// </summary>
    /// <param name="householdId">Household ID to refill events for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total number of events generated</returns>
    /// <remarks>
    /// This endpoint:
    /// - Iterates through all active recurring tasks in the household
    /// - Counts future pending events for each task
    /// - If count is below MinFutureEventsThreshold (from config), generates more events
    /// - Adds events up to MaxFutureEvents limit
    ///
    /// Typical usage:
    /// - Called by GitHub Actions workflow monthly
    /// - Ensures users always have future events visible
    /// - Prevents need to generate events on-the-fly
    ///
    /// Authentication: This endpoint should be protected with API key or service account in production
    /// </remarks>
    [HttpPost("refill-events")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RefillEventsForHousehold(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            _logger.LogInformation("Starting event refill for household {HouseholdId}", householdId);

            var totalEventsGenerated = await _eventService.RefillEventsForHouseholdAsync(
                householdId,
                cancellationToken);

            _logger.LogInformation(
                "Event refill completed for household {HouseholdId}: {TotalGenerated} events generated",
                householdId, totalEventsGenerated);

            return Ok(new
            {
                success = true,
                householdId = householdId,
                eventsGenerated = totalEventsGenerated,
                message = $"Successfully refilled {totalEventsGenerated} events for household {householdId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refilling events for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while refilling events" });
        }
    }

    /// <summary>
    /// Refill events for all households in the system.
    /// This endpoint is for system-wide event refill.
    /// Should be protected with strong authentication (API key, service account).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary of refill operation</returns>
    /// <remarks>
    /// WARNING: This endpoint processes ALL households and can be resource-intensive.
    /// Recommended for scheduled maintenance windows only.
    ///
    /// Consider implementing:
    /// - API key authentication
    /// - Rate limiting
    /// - Background job processing
    /// </remarks>
    [HttpPost("refill-events/all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RefillEventsForAllHouseholds(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting system-wide event refill");

            // TODO: Implement batch processing for all households
            // This is a placeholder for post-MVP implementation
            // In production, this should:
            // 1. Get all active households from database
            // 2. Process in batches to avoid memory/timeout issues
            // 3. Track progress and failures
            // 4. Send notifications on completion

            return Ok(new
            {
                success = false,
                message = "System-wide refill not yet implemented. Use per-household endpoint instead."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in system-wide event refill");
            return StatusCode(500, new { error = "An error occurred during system-wide refill" });
        }
    }
}
