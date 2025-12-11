using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class CategoryRepository : BaseRepository<CategoryEntity, Guid>, ICategoryRepository
{
    public CategoryRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<CategoryEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<CategoryEntity>> GetActiveCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(c => c.HouseholdId == householdId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetByCategoryTypeAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(c => c.HouseholdId == householdId && c.CategoryTypeId == categoryTypeId && c.IsActive,
            c => c.CategoryType);
    }

    public async Task<CategoryEntity?> GetWithCategoryTypeAsync(Guid householdId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(c => c.HouseholdId == householdId && c.Id == categoryId && c.DeletedAt == null)
            .Include(c => c.CategoryType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetOrderedCategoriesAsync(Guid householdId, Guid? categoryTypeId = null, CancellationToken cancellationToken = default)
    {
        var query = Query().Where(c => c.HouseholdId == householdId && c.IsActive);

        if (categoryTypeId.HasValue)
            query = query.Where(c => c.CategoryTypeId == categoryTypeId.Value);

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Include(c => c.CategoryType)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameInCategoryTypeAsync(Guid householdId, Guid categoryTypeId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = Query()
            .Where(c => c.HouseholdId == householdId && c.CategoryTypeId == categoryTypeId && c.Name == name && c.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

/// <summary>
/// Implementacja repozytorium dla typów kategorii
/// </summary>
public class CategoryTypeRepository : BaseRepository<CategoryTypeEntity, Guid>, ICategoryTypeRepository
{
    public CategoryTypeRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<CategoryTypeEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<CategoryTypeEntity>> GetActiveCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(ct => ct.HouseholdId == householdId && ct.IsActive)
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryTypeEntity?> GetWithCategoriesAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(ct => ct.HouseholdId == householdId && ct.Id == categoryTypeId && ct.DeletedAt == null)
            .Include(ct => ct.Categories.Where(c => c.IsActive && c.DeletedAt == null))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryTypeEntity>> GetOrderedCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(ct => ct.HouseholdId == householdId && ct.IsActive)
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.Name)
            .Include(ct => ct.Categories.Where(c => c.IsActive && c.DeletedAt == null))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(Guid householdId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = Query()
            .Where(ct => ct.HouseholdId == householdId && ct.Name == name && ct.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(ct => ct.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
