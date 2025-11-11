using Homely.API.Models.DTOs.SystemHouseholds;
using Homely.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// System-level household management controller (System Developer role)
/// </summary>
[ApiController]
[Route("api/system/households")]
//[Authorize(Roles = "system_developer")] // TODO: Uncomment when authentication is implemented
public class SystemHouseholdsController : ControllerBase
{
    private readonly ISystemHouseholdsService _systemHouseholdsService;
    private readonly ILogger<SystemHouseholdsController> _logger;

    public SystemHouseholdsController(
        ISystemHouseholdsService systemHouseholdsService,
        ILogger<SystemHouseholdsController> logger)
    {
        _systemHouseholdsService = systemHouseholdsService;
        _logger = logger;
    }

    /// <summary>
    /// Search households with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search by household name or address</param>
    /// <param name="planTypeId">Filter by plan type ID</param>
    /// <param name="subscriptionStatus">Filter by subscription status</param>
    /// <param name="hasActiveMembers">Filter households with active members</param>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of households</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedHouseholdsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedHouseholdsDto>> SearchHouseholds(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? planTypeId = null,
        [FromQuery] string? subscriptionStatus = null,
        [FromQuery] bool? hasActiveMembers = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Page size must be between 1 and 100" });
            }

            var filters = new HouseholdSearchFiltersDto
            {
                SearchTerm = searchTerm,
                PlanTypeId = planTypeId,
                SubscriptionStatus = subscriptionStatus,
                HasActiveMembers = hasActiveMembers,
                Page = page,
                PageSize = pageSize
            };

            var result = await _systemHouseholdsService.SearchHouseholdsAsync(filters, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching households");
            return StatusCode(500, new { error = "An error occurred while searching households" });
        }
    }

    /// <summary>
    /// Get detailed household information
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Household details</returns>
    [HttpGet("{householdId}")]
    [ProducesResponseType(typeof(SystemHouseholdDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemHouseholdDetailsDto>> GetHouseholdDetails(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var household = await _systemHouseholdsService.GetHouseholdDetailsAsync(householdId, cancellationToken);
            if (household == null)
            {
                return NotFound(new { error = $"Household with ID {householdId} not found" });
            }

            return Ok(household);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting household details for householdId: {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while getting household details" });
        }
    }

    /// <summary>
    /// Get overall system household statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Household statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(HouseholdStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HouseholdStatsDto>> GetHouseholdStats(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _systemHouseholdsService.GetHouseholdStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting household statistics");
            return StatusCode(500, new { error = "An error occurred while getting household statistics" });
        }
    }

    /// <summary>
    /// Create new household
    /// </summary>
    /// <param name="createHouseholdDto">Household creation data. AdminUserId is optional - if not provided, admin can be assigned later via the assign-admin endpoint.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created household</returns>
    /// <remarks>
    /// Sample request without admin:
    ///
    ///     POST /api/system/households
    ///     {
    ///         "name": "My Household",
    ///         "address": "123 Main St",
    ///         "planTypeId": 1
    ///     }
    ///
    /// Sample request with admin:
    ///
    ///     POST /api/system/households
    ///     {
    ///         "name": "My Household",
    ///         "address": "123 Main St",
    ///         "planTypeId": 1,
    ///         "adminUserId": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// To assign admin later, use POST /api/system/households/assign-admin
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SystemHouseholdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemHouseholdDto>> CreateHousehold(
        [FromBody] CreateHouseholdDto createHouseholdDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var household = await _systemHouseholdsService.CreateHouseholdAsync(createHouseholdDto, cancellationToken);
            return CreatedAtAction(
                nameof(GetHouseholdDetails),
                new { householdId = household.Id },
                household);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating household");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating household");
            return StatusCode(500, new { error = "An error occurred while creating household" });
        }
    }

    /// <summary>
    /// Update household information
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="updateHouseholdDto">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated household</returns>
    [HttpPut("{householdId}")]
    [ProducesResponseType(typeof(SystemHouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemHouseholdDto>> UpdateHousehold(
        Guid householdId,
        [FromBody] UpdateHouseholdDto updateHouseholdDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var household = await _systemHouseholdsService.UpdateHouseholdAsync(
                householdId,
                updateHouseholdDto,
                cancellationToken);

            return Ok(household);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating household");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while updating household" });
        }
    }

    /// <summary>
    /// Delete household (soft delete)
    /// </summary>
    /// <param name="householdId">Household ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{householdId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteHousehold(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _systemHouseholdsService.DeleteHouseholdAsync(householdId, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Household with ID {householdId} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Household deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while deleting household" });
        }
    }

    /// <summary>
    /// Assign admin to household
    /// </summary>
    /// <param name="assignAdminDto">Admin assignment data (householdId and userId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated household</returns>
    /// <remarks>
    /// Use this endpoint to assign an admin to a household that was created without an admin,
    /// or to promote an existing member to admin role.
    ///
    /// Sample request:
    ///
    ///     POST /api/system/households/assign-admin
    ///     {
    ///         "householdId": "00000000-0000-0000-0000-000000000000",
    ///         "userId": "00000000-0000-0000-0000-000000000000"
    ///     }
    ///
    /// If the user is already a member, their role will be updated to admin.
    /// If the user is not a member, they will be added as an admin.
    /// </remarks>
    [HttpPost("assign-admin")]
    [ProducesResponseType(typeof(SystemHouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemHouseholdDto>> AssignAdmin(
        [FromBody] AssignAdminDto assignAdminDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var household = await _systemHouseholdsService.AssignAdminAsync(assignAdminDto, cancellationToken);
            return Ok(household);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while assigning admin");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning admin to household");
            return StatusCode(500, new { error = "An error occurred while assigning admin" });
        }
    }
}
