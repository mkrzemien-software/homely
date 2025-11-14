namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// Paginated response for tasks list
/// </summary>
public class TasksResponse
{
    /// <summary>
    /// List of tasks for current page
    /// </summary>
    public IEnumerable<TaskDto> Data { get; set; } = new List<TaskDto>();

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public PaginationMetadata Pagination { get; set; } = new PaginationMetadata();
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMetadata
{
    /// <summary>
    /// Current page number (1-indexed)
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; } = 1;

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; } = 0;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int ItemsPerPage { get; set; } = 20;
}
