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


    public async Task<TaskEntity> CreateTaskTemplateWithEventExampleAsync(
        Guid householdId,
        Guid createdBy,
        string taskName,
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
                throw new InvalidOperationException("Limit zadań w planie został przekroczony");
            }
        }

        var taskTemplate = new TaskEntity
        {
            HouseholdId = householdId,
            CategoryId = categoryId,
            Name = taskName,
            DaysValue = maintenanceIntervalDays,
            CreatedBy = createdBy,
            IsActive = true
        };

        await _unitOfWork.Tasks.AddAsync(taskTemplate);

        var firstEvent = new EventEntity
        {
            TaskId = taskTemplate.Id,
            HouseholdId = householdId,
            Title = $"Konserwacja: {taskName}",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(maintenanceIntervalDays)),
            CreatedBy = createdBy,
            Priority = DatabaseConstants.PriorityLevels.Medium
        };

        await _unitOfWork.Events.AddAsync(firstEvent);

        await _unitOfWork.SaveChangesAsync();

        return taskTemplate;
    }

    public async Task<bool> CompleteEventExampleAsync(Guid eventId, Guid completedBy, string? notes = null)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null) return false;

        eventEntity.Status = DatabaseConstants.TaskStatuses.Completed;
        eventEntity.CompletionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        eventEntity.CompletionNotes = notes;
        eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Events.UpdateAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostponeEventExampleAsync(Guid eventId, DateOnly newDueDate, string? reason = null)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null) return false;

        eventEntity.PostponedFromDate = eventEntity.DueDate;
        eventEntity.DueDate = newDueDate;
        eventEntity.PostponeReason = reason;
        eventEntity.Status = DatabaseConstants.TaskStatuses.Postponed;
        eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Events.UpdateAsync(eventEntity);
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

        var upcomingEvents = await _unitOfWork.Events.GetUpcomingEventsAsync(householdId, 30);

        var overdueEvents = await _unitOfWork.Events.GetOverdueEventsAsync(householdId);

        var todayEvents = await _unitOfWork.Events.GetDueEventsAsync(
            householdId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        return new DashboardData
        {
            Household = household,
            UpcomingEvents = upcomingEvents.ToList(),
            OverdueEvents = overdueEvents.ToList(),
            TodayEvents = todayEvents.ToList(),
            TotalTasks = household.Tasks.Count(t => t.IsActive && t.DeletedAt == null),
            TotalMembers = household.HouseholdMembers.Count(hm => hm.DeletedAt == null)
        };
    }

    public async Task<IEnumerable<EventEntity>> SearchEventsExampleAsync(
        Guid householdId,
        string? status = null,
        string? priority = null,
        Guid? assignedTo = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        var query = _unitOfWork.Events.Query()
            .Where(e => e.HouseholdId == householdId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(e => e.Priority == priority);

        if (assignedTo.HasValue)
            query = query.Where(e => e.AssignedTo == assignedTo.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.DueDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.DueDate <= toDate.Value);

        return await query
            .OrderBy(e => e.DueDate)
            .ThenBy(e => e.Priority)
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

            var taskTemplate = new TaskEntity
            {
                HouseholdId = householdId,
                Name = "Test Task Template",
                CategoryId = 1,
                CreatedBy = userId,
                IsActive = true,
                DaysValue = 30
            };
            await _unitOfWork.Tasks.AddAsync(taskTemplate);

            var eventOccurrence = new EventEntity
            {
                TaskId = taskTemplate.Id,
                HouseholdId = householdId,
                Title = "Test Event",
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                CreatedBy = userId,
                Priority = DatabaseConstants.PriorityLevels.Medium
            };
            await _unitOfWork.Events.AddAsync(eventOccurrence);

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
    public List<EventEntity> UpcomingEvents { get; set; } = new();
    public List<EventEntity> OverdueEvents { get; set; } = new();
    public List<EventEntity> TodayEvents { get; set; } = new();
    public int TotalTasks { get; set; }
    public int TotalMembers { get; set; }
}
