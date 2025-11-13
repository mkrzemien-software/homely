using System.Collections.Generic;

namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// DTO for updating multiple categories sort order
/// </summary>
public class UpdateCategoriesSortOrderDto
{
    /// <summary>
    /// List of category ID and sort order updates
    /// </summary>
    public List<CategorySortOrderItem> Items { get; set; } = new();
}
