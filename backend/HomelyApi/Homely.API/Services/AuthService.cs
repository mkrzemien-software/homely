using Homely.API.Configuration;
using Homely.API.Models.DTOs;
using Homely.API.Repositories.Base;
using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Client = Supabase.Client;

namespace Homely.API.Services
{
    /// <summary>
    /// Authentication service implementation using Supabase Auth
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly Client _supabaseClient;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            Client supabaseClient,
            ILogger<AuthService> logger,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("Attempting to log in user with email: {Email}", loginRequest.Email);

                // Sign in with Supabase Auth
                var session = await _supabaseClient.Auth.SignIn(loginRequest.Email, loginRequest.Password);

                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                {
                    _logger.LogWarning("Login failed - no session or access token returned for email: {Email}", loginRequest.Email);
                    return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Nieprawidłowy email lub hasło", 401);
                }

                _logger.LogInformation("User successfully logged in: {UserId}", session.User.Id);

                // Map to response DTO
                var loginResponse = new LoginResponseDto
                {
                    AccessToken = session.AccessToken,
                    RefreshToken = session.RefreshToken ?? string.Empty,
                    ExpiresIn = session.ExpiresIn != 0 ? session.ExpiresIn : _jwtSettings.ExpirationInMinutes * 60,
                    User = await MapToUserDtoAsync(session.User)
                };

                return ApiResponseDto<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (GotrueException ex)
            {
                _logger.LogWarning(ex, "Supabase authentication error for email: {Email}", loginRequest.Email);
                
                var errorMessage = ex.Message switch
                {
                    var msg when msg.Contains("Invalid login credentials") => "Nieprawidłowy email lub hasło",
                    var msg when msg.Contains("Email not confirmed") => "Adres email nie został potwierdzony",
                    var msg when msg.Contains("Too many requests") => "Zbyt wiele prób logowania. Spróbuj ponownie później",
                    _ => "Wystąpił błąd podczas logowania"
                };

                return ApiResponseDto<LoginResponseDto>.ErrorResponse(errorMessage, 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", loginRequest.Email);
                return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później", 500);
            }
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token");

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Token odświeżania jest wymagany", 400);
                }

                var session = await _supabaseClient.Auth.RefreshSession();

                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                {
                    _logger.LogWarning("Token refresh failed - no session returned");
                    return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Nieprawidłowy token odświeżania", 401);
                }

                _logger.LogInformation("Token successfully refreshed for user: {UserId}", session.User.Id);

                var loginResponse = new LoginResponseDto
                {
                    AccessToken = session.AccessToken,
                    RefreshToken = session.RefreshToken ?? refreshToken,
                    ExpiresIn = session.ExpiresIn != 0 ? session.ExpiresIn : _jwtSettings.ExpirationInMinutes * 60,
                    User = await MapToUserDtoAsync(session.User)
                };

                return ApiResponseDto<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (GotrueException ex)
            {
                _logger.LogWarning(ex, "Supabase token refresh error");
                return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Nie udało się odświeżyć tokena. Zaloguj się ponownie", 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd podczas odświeżania tokena", 500);
            }
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<bool>> LogoutAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Attempting to logout user");

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return ApiResponseDto<bool>.ErrorResponse(
                        "Token dostępu jest wymagany", 400);
                }

                // Sign out from Supabase
                await _supabaseClient.Auth.SignOut();

                _logger.LogInformation("User successfully logged out");
                return ApiResponseDto<bool>.SuccessResponse(true);
            }
            catch (GotrueException ex)
            {
                _logger.LogWarning(ex, "Supabase logout error");
                // Even if logout fails on Supabase side, we can consider it successful from client perspective
                return ApiResponseDto<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return ApiResponseDto<bool>.ErrorResponse(
                    "Wystąpił błąd podczas wylogowania", 500);
            }
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Retrieving current user information");

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Token dostępu jest wymagany", 400);
                }

                // Set the session with the provided access token
                await _supabaseClient.Auth.RetrieveSessionAsync();
                
                var user = _supabaseClient.Auth.CurrentUser;

                if (user == null)
                {
                    _logger.LogWarning("No current user found with provided access token");
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Nieprawidłowy token dostępu", 401);
                }

                _logger.LogInformation("Current user retrieved: {UserId}", user.Id);
                return ApiResponseDto<UserDto>.SuccessResponse(await MapToUserDtoAsync(user));
            }
            catch (GotrueException ex)
            {
                _logger.LogWarning(ex, "Supabase error retrieving current user");
                return ApiResponseDto<UserDto>.ErrorResponse(
                    "Nieprawidłowy token dostępu", 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving current user");
                return ApiResponseDto<UserDto>.ErrorResponse(
                    "Wystąpił błąd podczas pobierania danych użytkownika", 500);
            }
        }

        /// <summary>
        /// Map Supabase User to UserDto with household membership data
        /// </summary>
        private async Task<UserDto> MapToUserDtoAsync(User user)
        {
            var userId = Guid.Parse(user.Id);

            // Get user profile for first name and last name
            var userProfile = await _unitOfWork.UserProfiles.GetByIdAsync(userId);

            // Get all user's active household memberships
            var memberships = await _unitOfWork.HouseholdMembers.GetUserMembershipsAsync(userId);
            var activeMemberships = memberships.Where(m => m.DeletedAt == null).ToList();

            // Map memberships to household DTOs
            var households = new List<UserHouseholdDto>();
            foreach (var membership in activeMemberships)
            {
                var household = await _unitOfWork.Households.GetByIdAsync(membership.HouseholdId);
                if (household != null)
                {
                    households.Add(new UserHouseholdDto
                    {
                        HouseholdId = membership.HouseholdId.ToString(),
                        HouseholdName = household.Name,
                        Role = membership.Role,
                        JoinedAt = membership.JoinedAt
                    });
                }
            }

            // Get primary membership for backwards compatibility
            var primaryMembership = activeMemberships.FirstOrDefault();

            // Build full name from user profile, fallback to email username
            var fullName = userProfile != null
                ? $"{userProfile.FirstName} {userProfile.LastName}".Trim()
                : user.Email?.Split('@')[0] ?? "Użytkownik";

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = fullName,
                EmailConfirmed = user.EmailConfirmedAt.HasValue,
                CreatedAt = user.CreatedAt,
                // Deprecated fields - kept for backwards compatibility
#pragma warning disable CS0618 // Type or member is obsolete
                HouseholdId = primaryMembership?.HouseholdId.ToString() ?? string.Empty,
                Role = primaryMembership?.Role ?? "member",
#pragma warning restore CS0618 // Type or member is obsolete
                // New households list
                Households = households
            };
        }
    }
}
