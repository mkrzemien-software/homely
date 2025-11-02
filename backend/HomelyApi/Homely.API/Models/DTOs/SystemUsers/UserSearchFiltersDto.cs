namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// Search filters for user queries
/// </summary>
public class UserSearchFiltersDto
{
    /// <summary>
    /// Search term for email, first name, or last name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by role
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Filter by status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by household ID
    /// </summary>
    public Guid? HouseholdId { get; set; }

    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of results per page
    /// </summary>
    public int PageSize { get; set; } = 20;
}
