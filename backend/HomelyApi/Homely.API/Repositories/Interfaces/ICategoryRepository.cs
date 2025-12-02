using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface ICategoryRepository : IBaseRepository<CategoryEntity, int>
{
    Task<IEnumerable<CategoryEntity>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetByCategoryTypeAsync(int categoryTypeId, CancellationToken cancellationToken = default);

    Task<CategoryEntity?> GetWithCategoryTypeAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetOrderedCategoriesAsync(int? categoryTypeId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameInCategoryTypeAsync(int categoryTypeId, string name, int? excludeId = null, CancellationToken cancellationToken = default);
}

public interface ICategoryTypeRepository : IBaseRepository<CategoryTypeEntity, int>
{
    Task<IEnumerable<CategoryTypeEntity>> GetActiveCategoryTypesAsync(CancellationToken cancellationToken = default);

    Task<CategoryTypeEntity?> GetWithCategoriesAsync(int categoryTypeId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryTypeEntity>> GetOrderedCategoryTypesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
}
