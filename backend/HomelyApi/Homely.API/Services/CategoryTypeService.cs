using Homely.API.Models.DTOs.Categories;
using Homely.API.Repositories.Base;
using Homely.API.Entities;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for category type management
/// </summary>
public class CategoryTypeService : ICategoryTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryTypeService> _logger;

    public CategoryTypeService(
        IUnitOfWork unitOfWork,
        ILogger<CategoryTypeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryTypeDto>> GetActiveCategoryTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTypes = await _unitOfWork.CategoryTypes.GetActiveCategoryTypesAsync(cancellationToken);
            return categoryTypes.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active category types");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryTypeDto>> GetAllCategoryTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTypes = await _unitOfWork.CategoryTypes.GetOrderedCategoryTypesAsync(cancellationToken);
            return categoryTypes.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all category types");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryTypeDto?> GetCategoryTypeByIdAsync(int categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryType = await _unitOfWork.CategoryTypes.GetByIdAsync(categoryTypeId);

            if (categoryType == null || categoryType.DeletedAt != null)
            {
                return null;
            }

            return MapToDto(categoryType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category type {CategoryTypeId}", categoryTypeId);
            throw;
        }
    }

    /// <summary>
    /// Maps CategoryTypeEntity to CategoryTypeDto
    /// </summary>
    private static CategoryTypeDto MapToDto(CategoryTypeEntity entity)
    {
        return new CategoryTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder
        };
    }
}
