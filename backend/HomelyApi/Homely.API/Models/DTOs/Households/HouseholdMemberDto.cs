namespace Homely.API.Models.DTOs.Households;

/// <summary>
/// Household member DTO
/// </summary>
public class HouseholdMemberDto
{
    /// <summary>
    /// Member ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Role in household (admin, member, dashboard)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Date when user joined the household
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }
}
