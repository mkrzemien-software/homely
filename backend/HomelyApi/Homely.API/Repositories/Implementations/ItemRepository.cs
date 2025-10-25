using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class ItemRepository : BaseRepository<ItemEntity, Guid>, IItemRepository
{
    public ItemRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<ItemEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<ItemEntity>> GetHouseholdItemsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(i => i.HouseholdId == householdId,
            i => i.Category,
            i => i.Category!.CategoryType);
    }

    public async Task<IEnumerable<ItemEntity>> GetActiveHouseholdItemsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(i => i.HouseholdId == householdId && i.IsActive,
            i => i.Category,
            i => i.Category!.CategoryType);
    }

    public async Task<IEnumerable<ItemEntity>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(i => i.CategoryId == categoryId && i.IsActive,
            i => i.Household,
            i => i.Category);
    }

    public async Task<ItemEntity?> GetWithDetailsAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(itemId,
            i => i.Category,
            i => i.Category!.CategoryType,
            i => i.Tasks,
            i => i.Household);
    }

    public async Task<IEnumerable<ItemEntity>> GetByCreatorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(i => i.CreatedBy == userId,
            i => i.Household,
            i => i.Category);
    }

    public async Task<IEnumerable<ItemEntity>> GetByPriorityAsync(Guid householdId, string priority, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(i => i.HouseholdId == householdId && 
                                       i.Priority == priority && 
                                       i.IsActive,
            i => i.Category);
    }

    public async Task<bool> CanUserAccessItemAsync(Guid itemId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(i => i.Id == itemId &&
                          i.Household.HouseholdMembers.Any(hm => hm.UserId == userId && hm.DeletedAt == null),
                cancellationToken);
    }
}
