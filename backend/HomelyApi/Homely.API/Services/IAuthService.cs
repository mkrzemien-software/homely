using Homely.API.Models.DTOs;

namespace Homely.API.Services
{
    /// <summary>
    /// Interface for authentication service handling user login, logout and token management
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user with email and password using Supabase Auth
        /// </summary>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>Login response with JWT tokens and user data</returns>
        Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="refreshToken">Valid refresh token</param>
        /// <returns>New access token and user data</returns>
        Task<ApiResponseDto<LoginResponseDto>> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        /// <param name="accessToken">User's access token</param>
        /// <returns>Success/failure result</returns>
        Task<ApiResponseDto<bool>> LogoutAsync(string accessToken);

        /// <summary>
        /// Get current user information from access token
        /// </summary>
        /// <param name="accessToken">Valid access token</param>
        /// <returns>User information</returns>
        Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(string accessToken);
    }
}
