using Homely.API.Models.DTOs.Categories;

namespace Homely.API.Services;

/// <summary>
/// Service interface for category management
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Get all active categories for a household
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get categories by category type for a household
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetCategoriesByCategoryTypeAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category by ID for a household
    /// </summary>
    Task<CategoryDto?> GetCategoryByIdAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all categories (including inactive) for a household
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new category for a household
    /// </summary>
    Task<CategoryDto> CreateCategoryAsync(Guid householdId, CreateCategoryDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing category for a household
    /// </summary>
    Task<CategoryDto> UpdateCategoryAsync(Guid householdId, int categoryId, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete category (soft delete) for a household
    /// </summary>
    Task<bool> DeleteCategoryAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update sort order for multiple categories for a household
    /// </summary>
    Task UpdateCategoriesSortOrderAsync(Guid householdId, UpdateCategoriesSortOrderDto updateDto, CancellationToken cancellationToken = default);
}
