namespace Homely.API.Models.DTOs.Categories;

/// <summary>
/// Single category sort order item
/// </summary>
public class CategorySortOrderItem
{
    /// <summary>
    /// Category ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// New sort order value
    /// </summary>
    public int SortOrder { get; set; }
}
