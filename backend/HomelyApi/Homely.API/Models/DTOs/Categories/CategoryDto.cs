namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// Category data transfer object
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Category ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category type ID
    /// </summary>
    public int? CategoryTypeId { get; set; }

    /// <summary>
    /// Category type name
    /// </summary>
    public string? CategoryTypeName { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display sort order
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether category is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
