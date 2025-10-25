using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class CategoryRepository : BaseRepository<CategoryEntity, int>, ICategoryRepository
{
    public CategoryRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<CategoryEntity, bool>> GetIdPredicate(int id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<CategoryEntity>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetByCategoryTypeAsync(int categoryTypeId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(c => c.CategoryTypeId == categoryTypeId && c.IsActive,
            c => c.CategoryType);
    }

    public async Task<CategoryEntity?> GetWithCategoryTypeAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(categoryId, c => c.CategoryType);
    }

    public async Task<IEnumerable<CategoryEntity>> GetOrderedCategoriesAsync(int? categoryTypeId = null, CancellationToken cancellationToken = default)
    {
        var query = Query().Where(c => c.IsActive);
        
        if (categoryTypeId.HasValue)
            query = query.Where(c => c.CategoryTypeId == categoryTypeId.Value);

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Include(c => c.CategoryType)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Implementacja repozytorium dla typów kategorii
/// </summary>
public class CategoryTypeRepository : BaseRepository<CategoryTypeEntity, int>, ICategoryTypeRepository
{
    public CategoryTypeRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<CategoryTypeEntity, bool>> GetIdPredicate(int id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<CategoryTypeEntity>> GetActiveCategoryTypesAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryTypeEntity?> GetWithCategoriesAsync(int categoryTypeId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(categoryTypeId, ct => ct.Categories);
    }

    public async Task<IEnumerable<CategoryTypeEntity>> GetOrderedCategoryTypesAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.Name)
            .Include(ct => ct.Categories.Where(c => c.IsActive && c.DeletedAt == null))
            .ToListAsync(cancellationToken);
    }
}
