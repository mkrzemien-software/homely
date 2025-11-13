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
    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetOrderedCategoriesAsync(null, cancellationToken);
            return categories
                .Where(c => c.IsActive && c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active categories");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryDto>> GetCategoriesByCategoryTypeAsync(int categoryTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetByCategoryTypeAsync(categoryTypeId, cancellationToken);
            return categories
                .Where(c => c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories for category type {CategoryTypeId}", categoryTypeId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetWithCategoryTypeAsync(categoryId, cancellationToken);

            if (category == null || category.DeletedAt != null)
            {
                return null;
            }

            return MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetOrderedCategoriesAsync(null, cancellationToken);
            return categories
                .Where(c => c.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = new CategoryEntity
            {
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

            _logger.LogInformation("Category created with ID {CategoryId}", category.Id);

            return MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);

            if (category == null || category.DeletedAt != null)
            {
                throw new InvalidOperationException($"Category with ID {categoryId} not found");
            }

            category.CategoryTypeId = updateDto.CategoryTypeId;
            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.SortOrder = updateDto.SortOrder;
            category.IsActive = updateDto.IsActive;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} updated successfully", categoryId);

            return MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);

            if (category == null || category.DeletedAt != null)
            {
                return false;
            }

            // Soft delete
            category.DeletedAt = DateTimeOffset.UtcNow;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} soft deleted successfully", categoryId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateCategoriesSortOrderAsync(UpdateCategoriesSortOrderDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all category IDs to update
            var categoryIds = updateDto.Items.Select(i => i.Id).ToList();

            // Fetch all categories in one query
            var categories = await _unitOfWork.Categories
                .GetAllAsync(cancellationToken);

            var categoriesToUpdate = categories
                .Where(c => categoryIds.Contains(c.Id) && c.DeletedAt == null)
                .ToList();

            if (categoriesToUpdate.Count != categoryIds.Count)
            {
                _logger.LogWarning("Some categories not found or already deleted");
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

            _logger.LogInformation("Successfully updated sort order for {Count} categories", categoriesToUpdate.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating categories sort order");
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
