namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// Response wrapper for categories list
/// </summary>
public class CategoriesResponse
{
    /// <summary>
    /// List of categories
    /// </summary>
    public IEnumerable<CategoryDto> Data { get; set; } = [];
}
