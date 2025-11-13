using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

/// <summary>
/// Repository for managing Tasks (task templates).
/// Tasks represent templates that define "what" and "how often" (if recurring).
/// </summary>
public class TaskRepository : BaseRepository<TaskEntity, Guid>, ITaskRepository
{
    public TaskRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<TaskEntity, bool>> GetIdPredicate(Guid id)
    {
        return t => t.Id == id;
    }

    public async Task<IEnumerable<TaskEntity>> GetHouseholdTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.HouseholdId == householdId,
            t => t.Category!,
            t => t.Category!.CategoryType);
    }

    public async Task<IEnumerable<TaskEntity>> GetByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.CategoryId == categoryId,
            t => t.Category!);
    }

    public async Task<IEnumerable<TaskEntity>> GetByCategoryAsync(
        Guid householdId,
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.HouseholdId == householdId && t.CategoryId == categoryId,
            t => t.Category!,
            t => t.Category!.CategoryType);
    }

    public async Task<IEnumerable<TaskEntity>> GetActiveTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.HouseholdId == householdId && t.IsActive,
            t => t.Category!,
            t => t.Category!.CategoryType);
    }

    public async Task<TaskEntity?> GetWithDetailsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(
            taskId,
            t => t.Category!,
            t => t.Category!.CategoryType,
            t => t.Household);
    }

    public async Task<bool> CanUserAccessTaskAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(t => t.Id == taskId &&
                          t.Household.HouseholdMembers.Any(hm => hm.UserId == userId && hm.DeletedAt == null),
                cancellationToken);
    }

    public async Task<int> CountActiveTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await CountAsync(
            t => t.HouseholdId == householdId && t.IsActive,
            cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetRecurringTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.HouseholdId == householdId &&
                 t.IsActive &&
                 (t.YearsValue > 0 || t.MonthsValue > 0 || t.WeeksValue > 0 || t.DaysValue > 0),
            t => t.Category!,
            t => t.Category!.CategoryType);
    }

    public async Task<IEnumerable<TaskEntity>> GetOneTimeTasksAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            t => t.HouseholdId == householdId &&
                 t.IsActive &&
                 (t.YearsValue == null || t.YearsValue == 0) &&
                 (t.MonthsValue == null || t.MonthsValue == 0) &&
                 (t.WeeksValue == null || t.WeeksValue == 0) &&
                 (t.DaysValue == null || t.DaysValue == 0),
            t => t.Category!,
            t => t.Category!.CategoryType);
    }
}
