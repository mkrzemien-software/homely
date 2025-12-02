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

    /// <inheritdoc/>
    public async Task<CategoryTypeDto> CreateCategoryTypeAsync(CreateCategoryTypeDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if category type with the same name already exists
            var exists = await _unitOfWork.CategoryTypes.ExistsWithNameAsync(createDto.Name, cancellationToken: cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Category type with name '{createDto.Name}' already exists");
            }

            var entity = new CategoryTypeEntity
            {
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
            _logger.LogError(ex, "Error creating category type");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryTypeDto> UpdateCategoryTypeAsync(int categoryTypeId, UpdateCategoryTypeDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _unitOfWork.CategoryTypes.GetByIdAsync(categoryTypeId);

            if (entity == null || entity.DeletedAt != null)
            {
                throw new InvalidOperationException($"Category type with ID {categoryTypeId} not found");
            }

            // Check if another category type with the same name already exists
            var exists = await _unitOfWork.CategoryTypes.ExistsWithNameAsync(updateDto.Name, categoryTypeId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Category type with name '{updateDto.Name}' already exists");
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
            _logger.LogError(ex, "Error updating category type {CategoryTypeId}", categoryTypeId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCategoryTypeAsync(int categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _unitOfWork.CategoryTypes.GetByIdAsync(categoryTypeId);

            if (entity == null || entity.DeletedAt != null)
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
            _logger.LogError(ex, "Error deleting category type {CategoryTypeId}", categoryTypeId);
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
