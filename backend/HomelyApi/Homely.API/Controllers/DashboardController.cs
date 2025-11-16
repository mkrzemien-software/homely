using Homely.API.Models.DTOs;
using Homely.API.Models.DTOs.Dashboard;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Dashboard controller for aggregated household data
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get upcoming events for dashboard with full nested data
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="days">Number of days to look ahead (default: 7, max: 365)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard upcoming events response with data and summary</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/dashboard/upcoming-events?householdId=00000000-0000-0000-0000-000000000000&amp;days=7
    ///
    /// Response includes:
    /// - Full event data with task, category, and assigned user information
    /// - Calculated urgency status (overdue, today, upcoming)
    /// - Summary statistics (overdue count, today count, this week count)
    /// </remarks>
    [HttpGet("upcoming-events")]
    [ProducesResponseType(typeof(ApiResponseDto<DashboardUpcomingEventsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<DashboardUpcomingEventsResponseDto>>> GetUpcomingEvents(
        [FromQuery] Guid householdId,
        [FromQuery] int days = 7,
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

            var response = await _dashboardService.GetUpcomingEventsAsync(householdId, days, cancellationToken);
            return Ok(ApiResponseDto<DashboardUpcomingEventsResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard upcoming events for household {HouseholdId}", householdId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving dashboard upcoming events",
                StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Get dashboard statistics for household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics response</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/dashboard/statistics?householdId=00000000-0000-0000-0000-000000000000
    ///
    /// Response includes:
    /// - Events statistics (pending, overdue, completed this month)
    /// - Tasks statistics (total, by category)
    /// - Plan usage (tasks used/limit, members used/limit)
    /// </remarks>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponseDto<DashboardStatisticsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponseDto<DashboardStatisticsResponseDto>>> GetStatistics(
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

            var response = await _dashboardService.GetStatisticsAsync(householdId, cancellationToken);
            return Ok(ApiResponseDto<DashboardStatisticsResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics for household {HouseholdId}", householdId);
            return StatusCode(500, ApiResponseDto<object>.ErrorResponse(
                "An error occurred while retrieving dashboard statistics",
                StatusCodes.Status500InternalServerError));
        }
    }
}
