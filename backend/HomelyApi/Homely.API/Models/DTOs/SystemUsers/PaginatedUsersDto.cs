namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// Paginated response for user list
/// </summary>
public class PaginatedUsersDto
{
    /// <summary>
    /// List of users
    /// </summary>
    public List<SystemUserDto> Users { get; set; } = new();

    /// <summary>
    /// Total number of users matching filters
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Current page number (1-indexed)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of users per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
}
