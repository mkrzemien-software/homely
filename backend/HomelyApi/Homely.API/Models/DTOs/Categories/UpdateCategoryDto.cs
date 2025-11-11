using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Category type ID
    /// </summary>
    public int? CategoryTypeId { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Display sort order
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a positive number")]
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether category is active
    /// </summary>
    public bool IsActive { get; set; }
}
