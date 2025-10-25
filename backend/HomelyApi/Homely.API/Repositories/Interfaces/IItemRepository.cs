using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IItemRepository : IBaseRepository<ItemEntity, Guid>
{
    Task<IEnumerable<ItemEntity>> GetHouseholdItemsAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ItemEntity>> GetActiveHouseholdItemsAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ItemEntity>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<ItemEntity?> GetWithDetailsAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ItemEntity>> GetByCreatorAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ItemEntity>> GetByPriorityAsync(Guid householdId, string priority, CancellationToken cancellationToken = default);

    Task<bool> CanUserAccessItemAsync(Guid itemId, Guid userId, CancellationToken cancellationToken = default);
}
