using Homely.API.Models.DTOs;
using Homely.API.Models.DTOs.Households;
using Homely.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Homely.API.Controllers
{
    /// <summary>
    /// Controller for household management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HouseholdsController : ControllerBase
    {
        private readonly IHouseholdService _householdService;
        private readonly ILogger<HouseholdsController> _logger;

        public HouseholdsController(
            IHouseholdService householdService,
            ILogger<HouseholdsController> logger)
        {
            _householdService = householdService;
            _logger = logger;
        }

        /// <summary>
        /// Get household by ID
        /// </summary>
        /// <param name="id">Household ID</param>
        /// <returns>Household details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<HouseholdDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<HouseholdDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<HouseholdDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<HouseholdDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<HouseholdDto>>> GetById(string id)
        {
            try
            {
                // Parse household ID
                if (!Guid.TryParse(id, out var householdId))
                {
                    return BadRequest(ApiResponseDto<HouseholdDto>.ErrorResponse(
                        "Nieprawidłowy format ID gospodarstwa", 400));
                }

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(ApiResponseDto<HouseholdDto>.ErrorResponse(
                        "Brak lub nieprawidłowe ID użytkownika", 401));
                }

                // Check if user has access to this household
                var hasAccess = await _householdService.CanUserAccessHouseholdAsync(householdId, userId);
                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} attempted to access household {HouseholdId} without permission",
                        userId, householdId);
                    return StatusCode(403, ApiResponseDto<HouseholdDto>.ErrorResponse(
                        "Brak dostępu do tego gospodarstwa", 403));
                }

                // Get household
                var household = await _householdService.GetByIdAsync(householdId);
                if (household == null)
                {
                    return NotFound(ApiResponseDto<HouseholdDto>.ErrorResponse(
                        "Gospodarstwo nie zostało znalezione", 404));
                }

                return Ok(ApiResponseDto<HouseholdDto>.SuccessResponse(household));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving household: {HouseholdId}", id);
                return StatusCode(500, ApiResponseDto<HouseholdDto>.ErrorResponse(
                    "Wystąpił błąd podczas pobierania danych gospodarstwa", 500));
            }
        }

        /// <summary>
        /// Get all households for the current user
        /// </summary>
        /// <returns>List of user's households</returns>
        [HttpGet("my")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<HouseholdDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<HouseholdDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<HouseholdDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<HouseholdDto>>>> GetMyHouseholds()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(ApiResponseDto<IEnumerable<HouseholdDto>>.ErrorResponse(
                        "Brak lub nieprawidłowe ID użytkownika", 401));
                }

                // Get user's households
                var households = await _householdService.GetUserHouseholdsAsync(userId);

                return Ok(ApiResponseDto<IEnumerable<HouseholdDto>>.SuccessResponse(households));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user households");
                return StatusCode(500, ApiResponseDto<IEnumerable<HouseholdDto>>.ErrorResponse(
                    "Wystąpił błąd podczas pobierania listy gospodarstw", 500));
            }
        }
    }
}
