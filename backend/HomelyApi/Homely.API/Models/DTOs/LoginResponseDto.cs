namespace Homely.API.Models.DTOs
{
    /// <summary>
    /// DTO for login response containing authentication data
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for getting new access tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// User information
        /// </summary>
        public UserDto User { get; set; } = new();
    }

    /// <summary>
    /// DTO containing basic user information
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// User ID from Supabase
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's display name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email verification status
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// User creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User's household ID (primary household) - DEPRECATED, use Households list instead
        /// </summary>
        [Obsolete("Use Households list instead")]
        public string HouseholdId { get; set; } = string.Empty;

        /// <summary>
        /// User's role in their primary household - DEPRECATED, use Households list instead
        /// </summary>
        [Obsolete("Use Households list instead")]
        public string Role { get; set; } = "member";

        /// <summary>
        /// List of households the user belongs to
        /// </summary>
        public List<UserHouseholdDto> Households { get; set; } = new();
    }

    /// <summary>
    /// DTO representing a household that a user belongs to
    /// </summary>
    public class UserHouseholdDto
    {
        /// <summary>
        /// Household ID
        /// </summary>
        public string HouseholdId { get; set; } = string.Empty;

        /// <summary>
        /// Household name
        /// </summary>
        public string HouseholdName { get; set; } = string.Empty;

        /// <summary>
        /// User's role in this household
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Date when user joined this household
        /// </summary>
        public DateTimeOffset JoinedAt { get; set; }
    }
}
