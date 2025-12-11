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
    public async Task<IEnumerable<CategoryTypeDto>> GetActiveCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTypes = await _unitOfWork.CategoryTypes.GetActiveCategoryTypesAsync(householdId, cancellationToken);
            return categoryTypes.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active category types for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryTypeDto>> GetAllCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTypes = await _unitOfWork.CategoryTypes.GetOrderedCategoryTypesAsync(householdId, cancellationToken);
            return categoryTypes.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all category types for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryTypeDto?> GetCategoryTypeByIdAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryType = await _unitOfWork.CategoryTypes.GetWithCategoriesAsync(householdId, categoryTypeId, cancellationToken);

            if (categoryType == null)
            {
                return null;
            }

            return MapToDto(categoryType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category type {CategoryTypeId} for household {HouseholdId}", categoryTypeId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryTypeDto> CreateCategoryTypeAsync(Guid householdId, CreateCategoryTypeDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if category type with the same name already exists in this household
            var exists = await _unitOfWork.CategoryTypes.ExistsWithNameAsync(householdId, createDto.Name, cancellationToken: cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Category type with name '{createDto.Name}' already exists in this household");
            }

            var entity = new CategoryTypeEntity
            {
                HouseholdId = householdId,
                Name = createDto.Name,
                Description = createDto.Description,
                SortOrder = createDto.SortOrder,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CategoryTypes.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category type for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryTypeDto> UpdateCategoryTypeAsync(Guid householdId, Guid categoryTypeId, UpdateCategoryTypeDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _unitOfWork.CategoryTypes.GetWithCategoriesAsync(householdId, categoryTypeId, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"Category type with ID {categoryTypeId} not found in household {householdId}");
            }

            // Check if another category type with the same name already exists in this household
            var exists = await _unitOfWork.CategoryTypes.ExistsWithNameAsync(householdId, updateDto.Name, categoryTypeId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Category type with name '{updateDto.Name}' already exists in this household");
            }

            entity.Name = updateDto.Name;
            entity.Description = updateDto.Description;
            entity.SortOrder = updateDto.SortOrder;
            entity.IsActive = updateDto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CategoryTypes.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category type {CategoryTypeId} for household {HouseholdId}", categoryTypeId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCategoryTypeAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _unitOfWork.CategoryTypes.GetWithCategoriesAsync(householdId, categoryTypeId, cancellationToken);

            if (entity == null)
            {
                return false;
            }

            // Soft delete
            entity.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.CategoryTypes.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category type {CategoryTypeId} for household {HouseholdId}", categoryTypeId, householdId);
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
