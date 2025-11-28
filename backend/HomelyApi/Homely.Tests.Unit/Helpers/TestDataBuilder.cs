using Bogus;
using Homely.API.Entities;

namespace Homely.Tests.Unit.Helpers;

/// <summary>
/// Test data builder using Bogus library for generating realistic test data
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a test EventEntity with required properties set
    /// </summary>
    public static EventEntity CreateEvent(
        Guid? id = null,
        Guid? taskId = null,
        Guid? householdId = null,
        Guid? assignedTo = null,
        DateOnly? dueDate = null,
        string status = "pending",
        string priority = "medium",
        DateOnly? completionDate = null,
        string? completionNotes = null,
        Guid? createdBy = null,
        DateTimeOffset? deletedAt = null)
    {
        return new EventEntity
        {
            Id = id ?? Guid.NewGuid(),
            TaskId = taskId,
            HouseholdId = householdId ?? Guid.NewGuid(),
            AssignedTo = assignedTo,
            DueDate = dueDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = status,
            Priority = priority,
            CompletionDate = completionDate,
            CompletionNotes = completionNotes,
            CreatedBy = createdBy ?? Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deletedAt
        };
    }

    /// <summary>
    /// Creates a test TaskEntity with interval settings
    /// </summary>
    public static TaskEntity CreateTask(
        Guid? id = null,
        Guid? householdId = null,
        string? name = null,
        int? yearsValue = null,
        int? monthsValue = null,
        int? weeksValue = null,
        int? daysValue = null,
        string priority = "medium",
        Guid? createdBy = null,
        DateTimeOffset? deletedAt = null)
    {
        return new TaskEntity
        {
            Id = id ?? Guid.NewGuid(),
            HouseholdId = householdId ?? Guid.NewGuid(),
            Name = name ?? Faker.Lorem.Sentence(3),
            Description = Faker.Lorem.Paragraph(),
            YearsValue = yearsValue,
            MonthsValue = monthsValue,
            WeeksValue = weeksValue,
            DaysValue = daysValue,
            Priority = priority,
            IsActive = true,
            CreatedBy = createdBy ?? Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deletedAt
        };
    }

    /// <summary>
    /// Creates a test HouseholdEntity with plan type
    /// </summary>
    public static HouseholdEntity CreateHousehold(
        Guid? id = null,
        string? name = null,
        int? planTypeId = null,
        PlanTypeEntity? planType = null)
    {
        return new HouseholdEntity
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? Faker.Company.CompanyName(),
            PlanTypeId = planTypeId ?? 1,
            PlanType = planType,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a test PlanTypeEntity
    /// </summary>
    public static PlanTypeEntity CreatePlanType(
        int? id = null,
        string? name = null,
        int? maxHouseholdMembers = null,
        int? maxTasks = null)
    {
        return new PlanTypeEntity
        {
            Id = id ?? 1,
            Name = name ?? "Darmowy",
            MaxHouseholdMembers = maxHouseholdMembers ?? 3,
            MaxTasks = maxTasks ?? 5,
            PriceMonthly =0
        };
    }

    /// <summary>
    /// Creates a premium plan type (Premium or Rodzinny)
    /// </summary>
    public static PlanTypeEntity CreatePremiumPlanType(string planName = "Premium")
    {
        return new PlanTypeEntity
        {
            Id = 2,
            Name = planName,
            MaxHouseholdMembers = null, // Unlimited
            MaxTasks = null, // Unlimited
            PriceMonthly =29.99m
        };
    }

    /// <summary>
    /// Creates a free plan type
    /// </summary>
    public static PlanTypeEntity CreateFreePlanType()
    {
        return new PlanTypeEntity
        {
            Id = 1,
            Name = "Darmowy",
            MaxHouseholdMembers = 3,
            MaxTasks = 5,
            PriceMonthly =0
        };
    }

    /// <summary>
    /// Creates a test EventHistoryEntity
    /// </summary>
    public static EventHistoryEntity CreateEventHistory(
        Guid? id = null,
        Guid? eventId = null,
        Guid? taskId = null,
        Guid? householdId = null,
        string? taskName = null)
    {
        return new EventHistoryEntity
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            TaskId = taskId,
            HouseholdId = householdId ?? Guid.NewGuid(),
            TaskName = taskName ?? Faker.Lorem.Sentence(3),
            CompletionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
