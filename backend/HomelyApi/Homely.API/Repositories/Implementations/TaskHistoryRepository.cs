using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class TaskHistoryRepository : BaseRepository<TaskHistoryEntity, Guid>, ITaskHistoryRepository
{
    public TaskHistoryRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TaskHistoryEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<TaskHistoryEntity>> GetHouseholdHistoryAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(th => th.HouseholdId == householdId)
            .OrderByDescending(th => th.CompletionDate)
            .Include(th => th.Item)
            .ThenInclude(i => i!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskHistoryEntity>> GetItemHistoryAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(th => th.ItemId == itemId,
            th => th.Item,
            th => th.Task);
    }

    public async Task<IEnumerable<TaskHistoryEntity>> GetCompletedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(th => th.CompletedBy == userId,
            th => th.Item,
            th => th.Household);
    }

    public async Task<IEnumerable<TaskHistoryEntity>> GetHistoryInDateRangeAsync(Guid householdId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(th => th.HouseholdId == householdId &&
                        th.CompletionDate >= fromDate &&
                        th.CompletionDate <= toDate)
            .OrderByDescending(th => th.CompletionDate)
            .Include(th => th.Item)
            .ThenInclude(i => i!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetCompletionStatsAsync(Guid householdId, int lastDays = 30, CancellationToken cancellationToken = default)
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-lastDays));
        
        var stats = await Query()
            .Where(th => th.HouseholdId == householdId && th.CompletionDate >= fromDate)
            .Join(Context.Set<ItemEntity>(),
                th => th.ItemId,
                i => i.Id,
                (th, i) => new { TaskHistory = th, Item = i })
            .Join(Context.Set<CategoryEntity>(),
                thi => thi.Item.CategoryId,
                c => c.Id,
                (thi, c) => new { thi.TaskHistory, Category = c })
            .GroupBy(x => x.Category.Name)
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return stats.ToDictionary(s => s.CategoryName, s => s.Count);
    }
}
