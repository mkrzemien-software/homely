using Homely.API.Models.DTOs.Categories;
using Homely.API.Repositories.Base;
using Homely.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for category management
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IUnitOfWork unitOfWork,
        ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetOrderedCategoriesAsync(householdId, null, cancellationToken);
            return categories
                .Where(c => c.IsActive && c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active categories for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryDto>> GetCategoriesByCategoryTypeAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetByCategoryTypeAsync(householdId, categoryTypeId, cancellationToken);
            return categories
                .Where(c => c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories for category type {CategoryTypeId} in household {HouseholdId}", categoryTypeId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetWithCategoryTypeAsync(householdId, categoryId, cancellationToken);

            if (category == null)
            {
                return null;
            }

            return MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId} for household {HouseholdId}", categoryId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetOrderedCategoriesAsync(householdId, null, cancellationToken);
            return categories
                .Where(c => c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> CreateCategoryAsync(Guid householdId, CreateCategoryDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate that CategoryTypeId is provided
            if (!createDto.CategoryTypeId.HasValue)
            {
                throw new InvalidOperationException("Category type ID is required");
            }

            // Check if category with the same name already exists in this category type for this household
            var exists = await _unitOfWork.Categories.ExistsWithNameInCategoryTypeAsync(
                householdId,
                createDto.CategoryTypeId.Value,
                createDto.Name,
                null,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Category with name '{createDto.Name}' already exists in this category type for this household");
            }

            var category = new CategoryEntity
            {
                HouseholdId = householdId,
                CategoryTypeId = createDto.CategoryTypeId,
                Name = createDto.Name,
                Description = createDto.Description,
                SortOrder = createDto.SortOrder,
                IsActive = createDto.IsActive,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category created with ID {CategoryId} for household {HouseholdId}", category.Id, householdId);

            return MapToDto(category);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> UpdateCategoryAsync(Guid householdId, int categoryId, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetWithCategoryTypeAsync(householdId, categoryId, cancellationToken);

            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID {categoryId} not found in household {householdId}");
            }

            // Validate that CategoryTypeId is provided
            if (!updateDto.CategoryTypeId.HasValue)
            {
                throw new InvalidOperationException("Category type ID is required");
            }

            // Check if another category with the same name already exists in this category type for this household
            var exists = await _unitOfWork.Categories.ExistsWithNameInCategoryTypeAsync(
                householdId,
                updateDto.CategoryTypeId.Value,
                updateDto.Name,
                categoryId,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Category with name '{updateDto.Name}' already exists in this category type for this household");
            }

            category.CategoryTypeId = updateDto.CategoryTypeId;
            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.SortOrder = updateDto.SortOrder;
            category.IsActive = updateDto.IsActive;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} updated successfully for household {HouseholdId}", categoryId, householdId);

            return MapToDto(category);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId} for household {HouseholdId}", categoryId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCategoryAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetWithCategoryTypeAsync(householdId, categoryId, cancellationToken);

            if (category == null)
            {
                return false;
            }

            // Soft delete
            category.DeletedAt = DateTimeOffset.UtcNow;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} soft deleted successfully for household {HouseholdId}", categoryId, householdId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId} for household {HouseholdId}", categoryId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateCategoriesSortOrderAsync(Guid householdId, UpdateCategoriesSortOrderDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all category IDs to update
            var categoryIds = updateDto.Items.Select(i => i.Id).ToList();

            // Fetch all categories for this household in one query
            var categories = await _unitOfWork.Categories
                .GetActiveCategoriesAsync(householdId, cancellationToken);

            var categoriesToUpdate = categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToList();

            if (categoriesToUpdate.Count != categoryIds.Count)
            {
                _logger.LogWarning("Some categories not found in household {HouseholdId} or already deleted", householdId);
            }

            // Update sort order for each category
            foreach (var item in updateDto.Items)
            {
                var category = categoriesToUpdate.FirstOrDefault(c => c.Id == item.Id);
                if (category != null)
                {
                    category.SortOrder = item.SortOrder;
                    category.UpdatedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                }
            }

            // Save all changes in a single transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated sort order for {Count} categories in household {HouseholdId}", categoriesToUpdate.Count, householdId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating categories sort order for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <summary>
    /// Maps CategoryEntity to CategoryDto
    /// </summary>
    private static CategoryDto MapToDto(CategoryEntity entity)
    {
        return new CategoryDto
        {
            Id = entity.Id,
            CategoryTypeId = entity.CategoryTypeId,
            CategoryTypeName = entity.CategoryType?.Name,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
