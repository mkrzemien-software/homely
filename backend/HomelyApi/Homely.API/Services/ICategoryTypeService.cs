using Homely.API.Models.DTOs.Categories;

namespace Homely.API.Services;

/// <summary>
/// Service interface for category type management
/// </summary>
public interface ICategoryTypeService
{
    /// <summary>
    /// Get all active category types for a household
    /// </summary>
    Task<IEnumerable<CategoryTypeDto>> GetActiveCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all category types (including inactive) for a household
    /// </summary>
    Task<IEnumerable<CategoryTypeDto>> GetAllCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category type by ID for a household
    /// </summary>
    Task<CategoryTypeDto?> GetCategoryTypeByIdAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new category type for a household
    /// </summary>
    Task<CategoryTypeDto> CreateCategoryTypeAsync(Guid householdId, CreateCategoryTypeDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing category type for a household
    /// </summary>
    Task<CategoryTypeDto> UpdateCategoryTypeAsync(Guid householdId, int categoryTypeId, UpdateCategoryTypeDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete category type (soft delete) for a household
    /// </summary>
    Task<bool> DeleteCategoryTypeAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default);
}
