using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Homely.API.Entities;
using Homely.API.Models.DTOs.Tasks;
using Homely.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Homely.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for TasksController - POST /api/tasks endpoint
/// Tests the complete flow: HTTP request -> Controller -> Service -> Database
/// </summary>
public class TasksControllerTests : IntegrationTestBase
{
    private readonly Faker _faker;

    public TasksControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldCreateTaskAndReturnCreated()
    {
        // Arrange
        await using var dbContext = GetDbContext();

        // Create test user profile
        var userId = Guid.NewGuid();
        var userProfile = new UserProfileEntity
        {
            UserId = userId,
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            PreferredLanguage = "pl",
            Timezone = "Europe/Warsaw",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.UserProfiles.Add(userProfile);

        // Create test household with free plan
        var householdId = Guid.NewGuid();
        var household = new HouseholdEntity
        {
            Id = householdId,
            Name = _faker.Company.CompanyName(),
            PlanTypeId = 1, // Free plan (seeded data: max 5 tasks)
            SubscriptionStatus = "free",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Households.Add(household);

        // Add user as household member
        var householdMember = new HouseholdMemberEntity
        {
            HouseholdId = householdId,
            UserId = userId,
            Role = "admin",
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.HouseholdMembers.Add(householdMember);

        await dbContext.SaveChangesAsync();

        // Create valid CreateTaskDto
        var createTaskDto = new CreateTaskDto
        {
            HouseholdId = householdId,
            CategoryId = 1, // "Przegląd samochodu" from seeded data
            Name = "Car Annual Inspection",
            Description = "Annual mandatory car inspection",
            YearsValue = 1,
            MonthsValue = 0,
            WeeksValue = 0,
            DaysValue = 0,
            Priority = "high",
            Notes = "Book appointment 2 weeks in advance",
            IsActive = true,
            CreatedBy = userId
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert - HTTP Response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull("Location header should contain created task URL");

        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        createdTask.Should().NotBeNull();
        createdTask!.Name.Should().Be("Car Annual Inspection");
        createdTask.Description.Should().Be("Annual mandatory car inspection");
        createdTask.Priority.Should().Be("high");
        createdTask.IsActive.Should().BeTrue();
        createdTask.HouseholdId.Should().Be(householdId.ToString());
        createdTask.Category.Should().NotBeNull();
        createdTask.Category.Id.Should().Be(1);
        createdTask.Interval.Should().NotBeNull();
        createdTask.Interval!.Years.Should().Be(1);
        createdTask.Interval.Months.Should().Be(0);
        createdTask.Interval.Weeks.Should().Be(0);
        createdTask.Interval.Days.Should().Be(0);
        createdTask.Notes.Should().Be("Book appointment 2 weeks in advance");

        // Assert - Database Verification
        await using var verifyDbContext = GetDbContext();
        var taskInDb = await verifyDbContext.Tasks
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Name == "Car Annual Inspection");

        taskInDb.Should().NotBeNull("Task should exist in database");
        taskInDb!.HouseholdId.Should().Be(householdId);
        taskInDb.CategoryId.Should().Be(1);
        taskInDb.Description.Should().Be("Annual mandatory car inspection");
        taskInDb.YearsValue.Should().Be(1);
        taskInDb.MonthsValue.Should().Be(0);
        taskInDb.WeeksValue.Should().Be(0);
        taskInDb.DaysValue.Should().Be(0);
        taskInDb.Priority.Should().Be("high");
        taskInDb.Notes.Should().Be("Book appointment 2 weeks in advance");
        taskInDb.IsActive.Should().BeTrue();
        taskInDb.CreatedBy.Should().Be(userId);
        taskInDb.DeletedAt.Should().BeNull("Task should not be soft-deleted");
        taskInDb.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
        taskInDb.Category.Should().NotBeNull();
        taskInDb.Category!.Name.Should().Be("Przegląd samochodu");
        taskInDb.CreatedByUser.Should().NotBeNull();
        taskInDb.CreatedByUser!.FirstName.Should().Be(userProfile.FirstName);
        taskInDb.CreatedByUser!.LastName.Should().Be(userProfile.LastName);

        // Assert - Plan Usage Tracking
        var planUsage = await verifyDbContext.PlanUsages
            .FirstOrDefaultAsync(pu =>
                pu.HouseholdId == householdId &&
                pu.UsageType == "tasks");

        planUsage.Should().NotBeNull("Plan usage should be tracked");
        planUsage!.CurrentValue.Should().Be(1, "First task created");
        planUsage.MaxValue.Should().Be(5, "Free plan limit is 5 tasks");
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - CreateTaskDto with missing required fields
        var invalidDto = new CreateTaskDto
        {
            HouseholdId = Guid.Empty, // Invalid
            Name = "", // Invalid - required
            CreatedBy = Guid.Empty // Invalid
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_WhenExceedingFreePlanLimit_ShouldReturnBadRequest()
    {
        // Arrange
        await using var dbContext = GetDbContext();

        var userId = Guid.NewGuid();
        var userProfile = new UserProfileEntity
        {
            UserId = userId,
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            PreferredLanguage = "pl",
            Timezone = "Europe/Warsaw",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.UserProfiles.Add(userProfile);

        var householdId = Guid.NewGuid();
        var household = new HouseholdEntity
        {
            Id = householdId,
            Name = _faker.Company.CompanyName(),
            PlanTypeId = 1, // Free plan: max 5 tasks
            SubscriptionStatus = "free",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Households.Add(household);

        var householdMember = new HouseholdMemberEntity
        {
            HouseholdId = householdId,
            UserId = userId,
            Role = "admin",
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.HouseholdMembers.Add(householdMember);

        // Create 5 tasks (free plan limit)
        for (int i = 1; i <= 5; i++)
        {
            var task = new TaskEntity
            {
                HouseholdId = householdId,
                CategoryId = 1,
                Name = $"Task {i}",
                Description = $"Test task {i}",
                Priority = "medium",
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            dbContext.Tasks.Add(task);
        }

        // Set plan usage to limit
        var planUsage = new PlanUsageEntity
        {
            HouseholdId = householdId,
            UsageType = "tasks",
            CurrentValue = 5,
            MaxValue = 5,
            UsageDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        dbContext.PlanUsages.Add(planUsage);

        await dbContext.SaveChangesAsync();

        // Try to create 6th task (exceeds limit)
        var createTaskDto = new CreateTaskDto
        {
            HouseholdId = householdId,
            CategoryId = 1,
            Name = "Task 6 - Should Fail",
            Description = "This should exceed the free plan limit",
            Priority = "high",
            IsActive = true,
            CreatedBy = userId
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorResponse = await response.Content.ReadAsStringAsync();
        errorResponse.Should().Contain("limit", "Error message should mention plan limit");
    }
}
