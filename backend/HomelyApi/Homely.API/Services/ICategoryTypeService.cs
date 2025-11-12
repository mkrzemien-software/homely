using Homely.API.Models.DTOs.Categories;

namespace Homely.API.Services;

/// <summary>
/// Service interface for category type management
/// </summary>
public interface ICategoryTypeService
{
    /// <summary>
    /// Get all active category types
    /// </summary>
    Task<IEnumerable<CategoryTypeDto>> GetActiveCategoryTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all category types (including inactive)
    /// </summary>
    Task<IEnumerable<CategoryTypeDto>> GetAllCategoryTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category type by ID
    /// </summary>
    Task<CategoryTypeDto?> GetCategoryTypeByIdAsync(int categoryTypeId, CancellationToken cancellationToken = default);
}
