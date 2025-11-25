using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class EventHistoryRepository : BaseRepository<EventHistoryEntity, Guid>, IEventHistoryRepository
{
    public EventHistoryRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<EventHistoryEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<EventHistoryEntity>> GetHouseholdHistoryAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(th => th.HouseholdId == householdId)
            .OrderByDescending(th => th.CompletionDate)
            .Include(th => th.Task)
            .ThenInclude(t => t!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventHistoryEntity>> GetTaskTemplateHistoryAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(th => th.TaskId == taskId,
            th => th.Task,
            th => th.Event);
    }

    public async Task<IEnumerable<EventHistoryEntity>> GetCompletedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(th => th.CompletedBy == userId,
            th => th.Task,
            th => th.Household);
    }

    public async Task<IEnumerable<EventHistoryEntity>> GetHistoryInDateRangeAsync(Guid householdId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(th => th.HouseholdId == householdId &&
                        th.CompletionDate >= fromDate &&
                        th.CompletionDate <= toDate)
            .OrderByDescending(th => th.CompletionDate)
            .Include(th => th.Task)
            .ThenInclude(t => t!.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetCompletionStatsAsync(Guid householdId, int lastDays = 30, CancellationToken cancellationToken = default)
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-lastDays));

        var stats = await Query()
            .Where(th => th.HouseholdId == householdId && th.CompletionDate >= fromDate)
            .Join(Context.Set<TaskEntity>(),
                th => th.TaskId,
                t => t.Id,
                (th, t) => new { EventHistory = th, Task = t })
            .Join(Context.Set<CategoryEntity>(),
                tht => tht.Task.CategoryId,
                c => c.Id,
                (tht, c) => new { tht.EventHistory, Category = c })
            .GroupBy(x => x.Category.Name)
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return stats.ToDictionary(s => s.CategoryName, s => s.Count);
    }
}

