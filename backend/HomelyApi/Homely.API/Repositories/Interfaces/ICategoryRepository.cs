using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface ICategoryRepository : IBaseRepository<CategoryEntity, int>
{
    Task<IEnumerable<CategoryEntity>> GetActiveCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetByCategoryTypeAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default);

    Task<CategoryEntity?> GetWithCategoryTypeAsync(Guid householdId, int categoryId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetOrderedCategoriesAsync(Guid householdId, int? categoryTypeId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameInCategoryTypeAsync(Guid householdId, int categoryTypeId, string name, int? excludeId = null, CancellationToken cancellationToken = default);
}

public interface ICategoryTypeRepository : IBaseRepository<CategoryTypeEntity, int>
{
    Task<IEnumerable<CategoryTypeEntity>> GetActiveCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<CategoryTypeEntity?> GetWithCategoriesAsync(Guid householdId, int categoryTypeId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryTypeEntity>> GetOrderedCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(Guid householdId, string name, int? excludeId = null, CancellationToken cancellationToken = default);
}
