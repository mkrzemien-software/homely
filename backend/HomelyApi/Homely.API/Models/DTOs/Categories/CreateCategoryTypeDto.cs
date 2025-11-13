using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// DTO for creating a new category type
/// </summary>
public class CreateCategoryTypeDto
{
    /// <summary>
    /// Category type name
    /// Example: "Przeglądy techniczne", "Wywóz śmieci", "Wizyty medyczne"
    /// </summary>
    [Required(ErrorMessage = "Category type name is required")]
    [MaxLength(100, ErrorMessage = "Category type name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category type description
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Display sort order
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a positive number")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Whether category type is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
