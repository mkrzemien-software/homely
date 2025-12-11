using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface ICategoryRepository : IBaseRepository<CategoryEntity, Guid>
{
    Task<IEnumerable<CategoryEntity>> GetActiveCategoriesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetByCategoryTypeAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default);

    Task<CategoryEntity?> GetWithCategoryTypeAsync(Guid householdId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryEntity>> GetOrderedCategoriesAsync(Guid householdId, Guid? categoryTypeId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameInCategoryTypeAsync(Guid householdId, Guid categoryTypeId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public interface ICategoryTypeRepository : IBaseRepository<CategoryTypeEntity, Guid>
{
    Task<IEnumerable<CategoryTypeEntity>> GetActiveCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<CategoryTypeEntity?> GetWithCategoriesAsync(Guid householdId, Guid categoryTypeId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CategoryTypeEntity>> GetOrderedCategoryTypesAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(Guid householdId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
