using Homely.API.Models.DTOs;
using Homely.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers
{
    /// <summary>
    /// Authentication controller handling user login, logout and token management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login user with email and password
        /// </summary>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>JWT tokens and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);

                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value?.Errors ?? [])
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    var validationResponse = ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Dane logowania są nieprawidłowe", 400, errors);

                    return BadRequest(validationResponse);
                }

                // Call authentication service
                var result = await _authService.LoginAsync(loginRequest);

                // Return appropriate status code
                return result.StatusCode switch
                {
                    200 => Ok(result),
                    401 => Unauthorized(result),
                    _ => StatusCode(result.StatusCode, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in login endpoint");
                var errorResponse = ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd serwera", 500);
                
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshTokenRequest">Refresh token request</param>
        /// <returns>New access token and user information</returns>
        /// <response code="200">Token refresh successful</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid refresh token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                if (string.IsNullOrWhiteSpace(refreshTokenRequest.RefreshToken))
                {
                    var validationResponse = ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Token odświeżania jest wymagany", 400);
                    return BadRequest(validationResponse);
                }

                var result = await _authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);

                return result.StatusCode switch
                {
                    200 => Ok(result),
                    401 => Unauthorized(result),
                    _ => StatusCode(result.StatusCode, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in refresh token endpoint");
                var errorResponse = ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd serwera", 500);
                
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Logout current user and invalidate tokens
        /// </summary>
        /// <returns>Logout confirmation</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                _logger.LogInformation("Logout attempt for user");

                // Get access token from authorization header
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    var errorResponse = ApiResponseDto<bool>.ErrorResponse(
                        "Token dostępu nie został znaleziony", 401);
                    return Unauthorized(errorResponse);
                }

                var result = await _authService.LogoutAsync(accessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in logout endpoint");
                var errorResponse = ApiResponseDto<bool>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd serwera", 500);
                
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user data</returns>
        /// <response code="200">User data retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                _logger.LogInformation("Get current user attempt");

                // Get access token from authorization header
                var accessToken = GetAccessTokenFromHeader();
                if (string.IsNullOrEmpty(accessToken))
                {
                    var errorResponse = ApiResponseDto<UserDto>.ErrorResponse(
                        "Token dostępu nie został znaleziony", 401);
                    return Unauthorized(errorResponse);
                }

                var result = await _authService.GetCurrentUserAsync(accessToken);

                return result.StatusCode switch
                {
                    200 => Ok(result),
                    401 => Unauthorized(result),
                    _ => StatusCode(result.StatusCode, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in get current user endpoint");
                var errorResponse = ApiResponseDto<UserDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd serwera", 500);
                
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Extract access token from Authorization header
        /// </summary>
        private string? GetAccessTokenFromHeader()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader["Bearer ".Length..].Trim();
        }
    }

    /// <summary>
    /// DTO for refresh token request
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
