using Homely.API.Models.DTOs.Categories;

namespace Homely.API.Services;

/// <summary>
/// Service interface for category management
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Get all active categories
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get categories by category type
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetCategoriesByCategoryTypeAsync(int categoryTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all categories (including inactive)
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new category
    /// </summary>
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing category
    /// </summary>
    Task<CategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete category (soft delete)
    /// </summary>
    Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
}
