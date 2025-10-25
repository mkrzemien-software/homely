using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Examples;

public class RepositoryUsageExamples
{
    private readonly IUnitOfWork _unitOfWork;

    public RepositoryUsageExamples(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// </summary>
    public async Task<HouseholdEntity> CreateHouseholdExampleAsync(string name, Guid createdBy)
    {
        // 1. Utworzenie gospodarstwa domowego
        var household = new HouseholdEntity
        {
            Name = name,
            PlanTypeId = 1  
        };

        await _unitOfWork.Households.AddAsync(household);

        var member = new HouseholdMemberEntity
        {
            HouseholdId = household.Id,
            UserId = createdBy,
            Role = DatabaseConstants.HouseholdRoles.Admin
        };

        await _unitOfWork.HouseholdMembers.AddAsync(member);

        await _unitOfWork.SaveChangesAsync();

        return household;
    }


    public async Task<ItemEntity> CreateItemWithTaskExampleAsync(
        Guid householdId,
        Guid createdBy,
        string itemName,
        int categoryId,
        int maintenanceIntervalDays)
    {
        var household = await _unitOfWork.Households.GetByIdAsync(householdId);
        if (household?.PlanType != null)
        {
            var canAdd = await _unitOfWork.PlanTypes.CheckPlanLimitsAsync(
                household.PlanTypeId,
                householdId,
                DatabaseConstants.UsageTypes.Items);

            if (!canAdd)
            {
                throw new InvalidOperationException("Limit przedmiotów w planie został przekroczony");
            }
        }

        var item = new ItemEntity
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Name = itemName,
            DaysValue = maintenanceIntervalDays,
            CreatedBy = createdBy,
            LastDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _unitOfWork.Items.AddAsync(item);

        var firstTask = new TaskEntity
        {
            ItemId = item.Id,
            HouseholdId = householdId,
            Title = $"Konserwacja: {itemName}",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(maintenanceIntervalDays)),
            CreatedBy = createdBy,
            Priority = DatabaseConstants.PriorityLevels.Medium
        };

        await _unitOfWork.Tasks.AddAsync(firstTask);

        await _unitOfWork.SaveChangesAsync();

        return item;
    }

    public async Task<bool> CompleteTaskExampleAsync(Guid taskId, Guid completedBy, string? notes = null)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) return false;

        task.Status = DatabaseConstants.TaskStatuses.Completed;
        task.CompletionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        task.CompletionNotes = notes;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostponeTaskExampleAsync(Guid taskId, DateOnly newDueDate, string? reason = null)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) return false;

        task.PostponedFromDate = task.DueDate;
        task.DueDate = newDueDate;
        task.PostponeReason = reason;
        task.Status = DatabaseConstants.TaskStatuses.Postponed;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DashboardData> GetDashboardDataExampleAsync(Guid householdId)
    {
        var household = await _unitOfWork.Households.GetHouseholdWithDetailsAsync(householdId);
        if (household == null)
        {
            throw new UnauthorizedAccessException("Brak dostępu do gospodarstwa domowego");
        }

        var upcomingTasks = await _unitOfWork.Tasks.GetUpcomingTasksAsync(householdId, 30);

        var overdueTasks = await _unitOfWork.Tasks.GetOverdueTasksAsync(householdId);

        var todayTasks = await _unitOfWork.Tasks.GetDueTasksAsync(
            householdId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        return new DashboardData
        {
            Household = household,
            UpcomingTasks = upcomingTasks.ToList(),
            OverdueTasks = overdueTasks.ToList(),
            TodayTasks = todayTasks.ToList(),
            TotalItems = household.Items.Count(i => i.IsActive && i.DeletedAt == null),
            TotalMembers = household.HouseholdMembers.Count(hm => hm.DeletedAt == null)
        };
    }

    public async Task<IEnumerable<TaskEntity>> SearchTasksExampleAsync(
        Guid householdId,
        string? status = null,
        string? priority = null,
        Guid? assignedTo = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        var query = _unitOfWork.Tasks.Query()
            .Where(t => t.HouseholdId == householdId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority);

        if (assignedTo.HasValue)
            query = query.Where(t => t.AssignedTo == assignedTo.Value);

        if (fromDate.HasValue)
            query = query.Where(t => t.DueDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.DueDate <= toDate.Value);

        return await query
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<string> InviteMemberExampleAsync(Guid householdId, Guid invitedBy, string role = DatabaseConstants.HouseholdRoles.Member)
    {
        var isAdmin = await _unitOfWork.Households.IsUserAdminAsync(householdId, invitedBy);
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Tylko administratorzy mogą zapraszać nowych członków");
        }

        var household = await _unitOfWork.Households.GetByIdAsync(householdId);
        if (household?.PlanType != null)
        {
            var canAdd = await _unitOfWork.PlanTypes.CheckPlanLimitsAsync(
                household.PlanTypeId,
                householdId,
                DatabaseConstants.UsageTypes.HouseholdMembers);

            if (!canAdd)
            {
                throw new InvalidOperationException("Limit członków w planie został przekroczony");
            }
        }

        var invitationToken = Guid.NewGuid().ToString("N");
        var member = new HouseholdMemberEntity
        {
            HouseholdId = householdId,
            UserId = Guid.Empty, 
            Role = role,
            InvitedBy = invitedBy,
            InvitationToken = invitationToken,
            InvitationExpiresAt = DateTimeOffset.UtcNow.AddDays(7) 
        };

        await _unitOfWork.HouseholdMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return invitationToken;
    }

    public async Task<bool> ComplexTransactionExampleAsync(Guid householdId, Guid userId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var item = new ItemEntity
            {
                HouseholdId = householdId,
                Name = "Test Item",
                CategoryId = 1,
                CreatedBy = userId,
                LastDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            await _unitOfWork.Items.AddAsync(item);

            var task = new TaskEntity
            {
                ItemId = item.Id,
                HouseholdId = householdId,
                Title = "Test Task",
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                CreatedBy = userId,
                Priority = DatabaseConstants.PriorityLevels.Medium
            };
            await _unitOfWork.Tasks.AddAsync(task);

            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

public class DashboardData
{
    public HouseholdEntity Household { get; set; } = null!;
    public List<Models.ViewModels.DashboardUpcomingTaskViewModel> UpcomingTasks { get; set; } = new();
    public List<TaskEntity> OverdueTasks { get; set; } = new();
    public List<TaskEntity> TodayTasks { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalMembers { get; set; }
}
