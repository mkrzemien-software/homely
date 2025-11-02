namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// System user DTO for admin views
/// </summary>
public class SystemUserDto
{
    /// <summary>
    /// User ID (from auth.users)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Current role in the context of household
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Account status (from auth.users or computed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Household ID (primary household)
    /// </summary>
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Household name
    /// </summary>
    public string HouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTimeOffset? LastLogin { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Account last update timestamp
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// User's avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
}
