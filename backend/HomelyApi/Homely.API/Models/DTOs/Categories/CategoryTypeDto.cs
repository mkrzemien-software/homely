namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// Category type data transfer object
/// </summary>
public class CategoryTypeDto
{
    /// <summary>
    /// Category type ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category type name
    /// Example: "Przeglądy techniczne", "Wywóz śmieci", "Wizyty medyczne"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category type description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display sort order
    /// </summary>
    public int SortOrder { get; set; }
}
