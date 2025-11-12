namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// Response wrapper for category types list
/// </summary>
public class CategoryTypesResponse
{
    /// <summary>
    /// List of category types
    /// </summary>
    public IEnumerable<CategoryTypeDto> Data { get; set; } = [];
}
