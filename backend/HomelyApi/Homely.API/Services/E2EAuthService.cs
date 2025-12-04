using Homely.API.Configuration;
using Homely.API.Models.DTOs;
using Homely.API.Repositories.Base;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;

namespace Homely.API.Services
{
    /// <summary>
    /// E2E-specific authentication service that bypasses Supabase Auth
    /// and authenticates directly against the PostgreSQL database
    /// </summary>
    public class E2EAuthService : IAuthService
    {
        private readonly ILogger<E2EAuthService> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _connectionString;

        public E2EAuthService(
            ILogger<E2EAuthService> logger,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("[E2E Auth] Attempting to log in user with email: {Email}", loginRequest.Email);

                // Query auth.users directly (E2E environment)
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT id, email, encrypted_password, email_confirmed_at, created_at
                    FROM auth.users
                    WHERE email = @email
                    LIMIT 1";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("email", loginRequest.Email);

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("[E2E Auth] User not found: {Email}", loginRequest.Email);
                    return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Nieprawidłowy email lub hasło", 401);
                }

                var userId = reader.GetGuid(0);
                var email = reader.GetString(1);
                var encryptedPassword = reader.GetString(2);
                var emailConfirmedAt = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                var createdAt = reader.GetDateTime(4);

                // Verify password using BCrypt
                bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, encryptedPassword);

                if (!isValidPassword)
                {
                    _logger.LogWarning("[E2E Auth] Invalid password for user: {Email}", loginRequest.Email);
                    return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                        "Nieprawidłowy email lub hasło", 401);
                }

                _logger.LogInformation("[E2E Auth] User successfully authenticated: {UserId}", userId);

                // Generate JWT token
                var token = GenerateJwtToken(userId.ToString(), email);

                // Map to UserDto
                var userDto = await MapToUserDtoAsync(userId, email, emailConfirmedAt, createdAt);

                var loginResponse = new LoginResponseDto
                {
                    AccessToken = token,
                    RefreshToken = string.Empty, // Not implemented for E2E
                    ExpiresIn = _jwtSettings.ExpirationInMinutes * 60,
                    User = userDto
                };

                return ApiResponseDto<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[E2E Auth] Unexpected error during login for email: {Email}", loginRequest.Email);
                return ApiResponseDto<LoginResponseDto>.ErrorResponse(
                    "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później", 500);
            }
        }

        /// <inheritdoc/>
        public Task<ApiResponseDto<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogWarning("[E2E Auth] RefreshToken not implemented in E2E environment");
            return Task.FromResult(ApiResponseDto<LoginResponseDto>.ErrorResponse(
                "Refresh token not supported in E2E environment", 501));
        }

        /// <inheritdoc/>
        public Task<ApiResponseDto<bool>> LogoutAsync(string accessToken)
        {
            _logger.LogInformation("[E2E Auth] Logout called (no-op in E2E)");
            // E2E environment doesn't maintain sessions, so logout is a no-op
            return Task.FromResult(ApiResponseDto<bool>.SuccessResponse(true));
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("[E2E Auth] Retrieving current user from token");

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Token dostępu jest wymagany", 400);
                }

                // Validate and extract user ID from JWT token
                var principal = ValidateJwtToken(accessToken);
                if (principal == null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Nieprawidłowy token dostępu", 401);
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim))
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Nieprawidłowy token dostępu", 401);
                }

                var userId = Guid.Parse(userIdClaim);

                // Query user from database
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT email, email_confirmed_at, created_at
                    FROM auth.users
                    WHERE id = @id
                    LIMIT 1";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("id", userId);

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("[E2E Auth] User not found: {UserId}", userId);
                    return ApiResponseDto<UserDto>.ErrorResponse(
                        "Użytkownik nie znaleziony", 404);
                }

                var email = reader.GetString(0);
                var emailConfirmedAt = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                var createdAt = reader.GetDateTime(2);

                var userDto = await MapToUserDtoAsync(userId, email, emailConfirmedAt, createdAt);

                return ApiResponseDto<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[E2E Auth] Error retrieving current user");
                return ApiResponseDto<UserDto>.ErrorResponse(
                    "Wystąpił błąd podczas pobierania danych użytkownika", 500);
            }
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(string userId, string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token and return claims principal
        /// </summary>
        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.ValidIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.ValidAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[E2E Auth] Token validation failed");
                return null;
            }
        }

        /// <summary>
        /// Map database user to UserDto with household membership data
        /// </summary>
        private async Task<UserDto> MapToUserDtoAsync(Guid userId, string email, DateTime? emailConfirmedAt, DateTime createdAt)
        {
            // Get user profile for first name and last name
            var userProfile = await _unitOfWork.UserProfiles.GetByIdAsync(userId);

            // Get user's primary household membership (first active membership)
            var memberships = await _unitOfWork.HouseholdMembers.GetUserMembershipsAsync(userId);
            var primaryMembership = memberships.FirstOrDefault(m => m.DeletedAt == null);

            // Build full name from user profile, fallback to email username
            var fullName = userProfile != null
                ? $"{userProfile.FirstName} {userProfile.LastName}".Trim()
                : email?.Split('@')[0] ?? "Użytkownik";

            return new UserDto
            {
                Id = userId.ToString(),
                Email = email,
                Name = fullName,
                EmailConfirmed = emailConfirmedAt.HasValue,
                CreatedAt = createdAt,
                HouseholdId = primaryMembership?.HouseholdId.ToString() ?? string.Empty,
                Role = primaryMembership?.Role ?? "member"
            };
        }
    }
}
