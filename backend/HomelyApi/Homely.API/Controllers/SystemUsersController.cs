using Homely.API.Models.DTOs.SystemUsers;
using Homely.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// System-level user management controller (System Developer role)
/// </summary>
[ApiController]
[Route("api/system/users")]
//[Authorize(Roles = "system_developer")] // TODO: Uncomment when authentication is implemented
public class SystemUsersController : ControllerBase
{
    private readonly ISystemUsersService _systemUsersService;
    private readonly ILogger<SystemUsersController> _logger;

    public SystemUsersController(
        ISystemUsersService systemUsersService,
        ILogger<SystemUsersController> logger)
    {
        _systemUsersService = systemUsersService;
        _logger = logger;
    }

    /// <summary>
    /// Search users with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search by email, first name, or last name</param>
    /// <param name="role">Filter by role</param>
    /// <param name="status">Filter by account status</param>
    /// <param name="householdId">Filter by household ID</param>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedUsersDto>> SearchUsers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? householdId = null,
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

            var filters = new UserSearchFiltersDto
            {
                SearchTerm = searchTerm,
                Role = role,
                Status = status,
                HouseholdId = householdId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _systemUsersService.SearchUsersAsync(filters, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return StatusCode(500, new { error = "An error occurred while searching users" });
        }
    }

    /// <summary>
    /// Get detailed user information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(SystemUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemUserDto>> GetUserDetails(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _systemUsersService.GetUserDetailsAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound(new { error = $"User with ID {userId} not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while getting user details" });
        }
    }

    /// <summary>
    /// Get user activity history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of activity entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user activities</returns>
    [HttpGet("{userId}/activity")]
    [ProducesResponseType(typeof(List<UserActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserActivityDto>>> GetUserActivity(
        Guid userId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest(new { error = "Limit must be between 1 and 100" });
            }

            var activities = await _systemUsersService.GetUserActivityAsync(userId, limit, cancellationToken);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while getting user activity" });
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SystemUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemUserDto>> CreateUser(
        [FromBody] CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _systemUsersService.CreateUserAsync(createUserDto, cancellationToken);
            return CreatedAtAction(
                nameof(GetUserDetails),
                new { userId = user.Id },
                user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating user");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { error = "An error occurred while creating user" });
        }
    }

    /// <summary>
    /// Update user role in household
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Role update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user</returns>
    [HttpPut("{userId}/role")]
    [ProducesResponseType(typeof(SystemUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemUserDto>> UpdateUserRole(
        Guid userId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _systemUsersService.UpdateUserRoleAsync(
                userId,
                request.HouseholdId,
                request.Role,
                cancellationToken);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating user role");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while updating user role" });
        }
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Password reset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{userId}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetPassword(
        Guid userId,
        [FromBody] PasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _systemUsersService.ResetUserPasswordAsync(
                userId,
                request.SendEmail,
                cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = "Failed to reset password" });
            }

            return Ok(new
            {
                success = true,
                message = request.SendEmail
                    ? "Password reset email sent successfully"
                    : "Password reset initiated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while resetting password" });
        }
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{userId}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UnlockAccount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _systemUsersService.UnlockUserAccountAsync(userId, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = "Failed to unlock account" });
            }

            return Ok(new
            {
                success = true,
                message = "Account unlocked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while unlocking account" });
        }
    }

    /// <summary>
    /// Move user to different household
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Move household request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user</returns>
    [HttpPost("{userId}/move")]
    [ProducesResponseType(typeof(SystemUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemUserDto>> MoveToHousehold(
        Guid userId,
        [FromBody] MoveHouseholdRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _systemUsersService.MoveUserToHouseholdAsync(
                userId,
                request.NewHouseholdId,
                cancellationToken);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while moving user to household");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving user {UserId} to household", userId);
            return StatusCode(500, new { error = "An error occurred while moving user to household" });
        }
    }

    /// <summary>
    /// Delete user from system
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _systemUsersService.DeleteUserAsync(userId, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"User with ID {userId} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "User deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while deleting user" });
        }
    }

    /// <summary>
    /// Get all households a user belongs to
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's households</returns>
    [HttpGet("{userId}/households")]
    [ProducesResponseType(typeof(List<UserHouseholdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserHouseholdDto>>> GetUserHouseholds(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var households = await _systemUsersService.GetUserHouseholdsAsync(userId, cancellationToken);
            return Ok(households);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User not found while getting households");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting households for userId: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while getting user households" });
        }
    }

    /// <summary>
    /// Add user to a household
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Add household request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{userId}/households")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddUserToHousehold(
        Guid userId,
        [FromBody] AddUserToHouseholdRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _systemUsersService.AddUserToHouseholdAsync(
                userId,
                request.HouseholdId,
                request.Role,
                cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = "User is already a member of this household" });
            }

            return Ok(new
            {
                success = true,
                message = "User added to household successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while adding user to household");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to household", userId);
            return StatusCode(500, new { error = "An error occurred while adding user to household" });
        }
    }

    /// <summary>
    /// Remove user from a household
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{userId}/households/{householdId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveUserFromHousehold(
        Guid userId,
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _systemUsersService.RemoveUserFromHouseholdAsync(userId, householdId, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = "User is not a member of this household" });
            }

            return Ok(new
            {
                success = true,
                message = "User removed from household successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from household {HouseholdId}", userId, householdId);
            return StatusCode(500, new { error = "An error occurred while removing user from household" });
        }
    }
}

/// <summary>
/// Request model for updating user role
/// </summary>
public class UpdateRoleRequest
{
    public Guid HouseholdId { get; set; }
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Request model for password reset
/// </summary>
public class PasswordResetRequest
{
    public bool SendEmail { get; set; } = true;
}

/// <summary>
/// Request model for moving user to household
/// </summary>
public class MoveHouseholdRequest
{
    public Guid NewHouseholdId { get; set; }
}

/// <summary>
/// Request model for adding user to household
/// </summary>
public class AddUserToHouseholdRequest
{
    public Guid HouseholdId { get; set; }
    public string Role { get; set; } = string.Empty;
}
