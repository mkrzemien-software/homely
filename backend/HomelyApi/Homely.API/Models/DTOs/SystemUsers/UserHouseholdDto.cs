namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// DTO representing a household that a user belongs to
/// </summary>
public class UserHouseholdDto
{
    /// <summary>
    /// Household ID
    /// </summary>
    public Guid HouseholdId { get; set; }

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

    /// <summary>
    /// Plan type name
    /// </summary>
    public string PlanTypeName { get; set; } = string.Empty;
}
