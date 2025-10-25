using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Models.ViewModels;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class TaskRepository : BaseRepository<TaskEntity, Guid>, ITaskRepository
{
    public TaskRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TaskEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<TaskEntity>> GetHouseholdTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(t => t.HouseholdId == householdId, 
            t => t.Item, 
            t => t.Item!.Category);
    }

    public async Task<IEnumerable<TaskEntity>> GetAssignedTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(t => t.AssignedTo == userId, 
            t => t.Item, 
            t => t.Household);
    }

    public async Task<IEnumerable<TaskEntity>> GetByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(t => t.HouseholdId == householdId && t.Status == status,
            t => t.Item,
            t => t.Item!.Category);
    }

    public async Task<IEnumerable<DashboardUpcomingTaskViewModel>> GetUpcomingTasksAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        
        return await Context.Set<TaskEntity>()
            .Where(t => t.HouseholdId == householdId && 
                       t.Status == DatabaseConstants.TaskStatuses.Pending &&
                       t.DueDate <= endDate &&
                       t.DeletedAt == null)
            .Join(Context.Set<ItemEntity>(),
                t => t.ItemId,
                i => i.Id,
                (t, i) => new { Task = t, Item = i })
            .Join(Context.Set<CategoryEntity>(),
                ti => ti.Item.CategoryId,
                c => c.Id,
                (ti, c) => new { ti.Task, ti.Item, Category = c })
            .Join(Context.Set<CategoryTypeEntity>(),
                tic => tic.Category.CategoryTypeId,
                ct => ct.Id,
                (tic, ct) => new { tic.Task, tic.Item, tic.Category, CategoryType = ct })
            .Join(Context.Set<HouseholdEntity>(),
                data => data.Task.HouseholdId,
                h => h.Id,
                (data, h) => new DashboardUpcomingTaskViewModel
                {
                    Id = data.Task.Id,
                    DueDate = data.Task.DueDate,
                    Title = data.Task.Title,
                    Description = data.Task.Description,
                    Status = data.Task.Status,
                    Priority = data.Task.Priority,
                    AssignedTo = data.Task.AssignedTo,
                    ItemName = data.Item.Name,
                    ItemDescription = data.Item.Description,
                    CategoryName = data.Category.Name,
                    CategoryTypeName = data.CategoryType.Name,
                    HouseholdId = data.Task.HouseholdId,
                    HouseholdName = h.Name,
                    UrgencyStatus = CalculateUrgencyStatus(data.Task.DueDate),
                    DaysUntilDue = data.Task.DueDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber,
                    PriorityScore = CalculatePriorityScore(data.Task.DueDate, data.Task.Priority)
                })
            .OrderBy(t => t.PriorityScore)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetDueTasksAsync(Guid householdId, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken cancellationToken = default)
    {
        fromDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
        toDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        return await GetWhereAsync(t => t.HouseholdId == householdId &&
                                       t.DueDate >= fromDate &&
                                       t.DueDate <= toDate &&
                                       t.Status == DatabaseConstants.TaskStatuses.Pending,
            t => t.Item,
            t => t.Item!.Category);
    }

    public async Task<IEnumerable<TaskEntity>> GetOverdueTasksAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        return await GetWhereAsync(t => t.HouseholdId == householdId &&
                                       t.DueDate < today &&
                                       t.Status == DatabaseConstants.TaskStatuses.Pending,
            t => t.Item,
            t => t.Item!.Category);
    }

    public async Task<IEnumerable<TaskEntity>> GetItemTasksAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(t => t.ItemId == itemId);
    }

    public async Task<TaskEntity?> GetWithDetailsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(taskId,
            t => t.Item,
            t => t.Item!.Category,
            t => t.Household);
    }

    public async Task<bool> CanUserAccessTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(t => t.Id == taskId &&
                          t.Household.HouseholdMembers.Any(hm => hm.UserId == userId && hm.DeletedAt == null),
                cancellationToken);
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    private static string CalculateUrgencyStatus(DateOnly dueDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysDiff = dueDate.DayNumber - today.DayNumber;

        return daysDiff switch
        {
            < 0 => DatabaseConstants.UrgencyStatuses.Overdue,
            0 => DatabaseConstants.UrgencyStatuses.Today,
            <= 7 => DatabaseConstants.UrgencyStatuses.ThisWeek,
            <= 30 => DatabaseConstants.UrgencyStatuses.ThisMonth,
            _ => DatabaseConstants.UrgencyStatuses.Upcoming
        };
    }

    private static int CalculatePriorityScore(DateOnly dueDate, string priority)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysDiff = dueDate.DayNumber - today.DayNumber;

        var priorityValue = priority switch
        {
            DatabaseConstants.PriorityLevels.High => 3,
            DatabaseConstants.PriorityLevels.Medium => 2,
            DatabaseConstants.PriorityLevels.Low => 1,
            _ => 1
        };

        return daysDiff switch
        {
            < 0 => 1000 + priorityValue,  // Overdue
            0 => 500 + priorityValue,     // Today
            <= 7 => 100 + priorityValue,  // This week
            _ => priorityValue             // Future
        };
    }
}
