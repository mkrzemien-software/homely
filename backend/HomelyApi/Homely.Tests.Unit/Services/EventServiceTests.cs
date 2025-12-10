using FluentAssertions;
using Homely.API.Entities;
using Homely.API.Models.Configuration;
using Homely.API.Models.DTOs.Tasks;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;
using Homely.API.Services;
using Homely.Tests.Unit.Base;
using Homely.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace Homely.Tests.Unit.Services;

/// <summary>
/// Unit tests for EventService - focusing on CompleteEventAsync workflow
/// Tests verify: event completion, recurring event generation, premium features
/// </summary>
public class EventServiceTests : UnitTestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<IHouseholdRepository> _mockHouseholdRepository;
    private readonly Mock<IEventHistoryRepository> _mockEventHistoryRepository;
    private readonly Mock<ILogger<EventService>> _mockLogger;
    private readonly Mock<IOptions<EventGenerationSettings>> _mockEventSettings;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        // Initialize mocks
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockHouseholdRepository = new Mock<IHouseholdRepository>();
        _mockEventHistoryRepository = new Mock<IEventHistoryRepository>();
        _mockLogger = CreateMockLogger<EventService>();
        _mockEventSettings = new Mock<IOptions<EventGenerationSettings>>();

        // Setup event generation settings with default values
        _mockEventSettings.Setup(x => x.Value).Returns(new EventGenerationSettings
        {
            FutureYears = 2,
            MinFutureMonthsThreshold = 6
        });

        // Setup UnitOfWork to return repository mocks
        _mockUnitOfWork.Setup(u => u.Events).Returns(_mockEventRepository.Object);
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepository.Object);
        _mockUnitOfWork.Setup(u => u.Households).Returns(_mockHouseholdRepository.Object);
        _mockUnitOfWork.Setup(u => u.EventsHistory).Returns(_mockEventHistoryRepository.Object);

        // Setup ExecuteInTransactionAsync to execute the operation directly
        // This simulates transaction behavior in unit tests
        _mockUnitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<EventDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<EventDto>>, CancellationToken>(
                async (operation, ct) => await operation(ct));

        // Create service instance
        _eventService = new EventService(_mockUnitOfWork.Object, _mockLogger.Object, _mockEventSettings.Object);
    }

    #region CompleteEventAsync Tests

    [Fact]
    public async Task CompleteEventAsync_ShouldMarkEventAsCompleted()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var completedBy = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            householdId: householdId,
            status: "pending",
            dueDate: CreateDate(2024, 1, 15)
        );

        var completeDto = new CompleteEventDto
        {
            CompletionDate = "2024-01-20",
            Notes = "Completed successfully"
        };

        // Mock event repository
        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        var result = await _eventService.CompleteEventAsync(eventId, completeDto, completedBy);

        // Assert
        result.Should().NotBeNull();
        existingEvent.Status.Should().Be("completed");
        existingEvent.CompletionDate.Should().Be(DateOnly.Parse("2024-01-20"));
        existingEvent.CompletionNotes.Should().Be("Completed successfully");

        // Verify repository interactions
        _mockEventRepository.Verify(
            r => r.UpdateAsync(It.Is<EventEntity>(e => e.Status == "completed"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteEventAsync_WithRecurringTask_ShouldCreateNextEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var completedBy = Guid.NewGuid();

        // Create event linked to a task with monthly interval
        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId,
            householdId: householdId,
            status: "pending",
            dueDate: CreateDate(2024, 1, 15)
        );

        // Create task template with monthly interval
        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            householdId: householdId,
            name: "Monthly car service",
            monthsValue: 1, // Recurring every month
            priority: "high"
        );

        var completeDto = new CompleteEventDto
        {
            CompletionDate = "2024-01-20"
        };

        // Mock repositories
        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        EventEntity? capturedNewEvent = null;
        _mockEventRepository
            .Setup(r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EventEntity, CancellationToken>((e, ct) => capturedNewEvent = e)
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, completedBy);

        // Assert
        existingEvent.Status.Should().Be("completed");

        // Verify new recurring event was created
        _mockEventRepository.Verify(
            r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "A new recurring event should be created");

        capturedNewEvent.Should().NotBeNull();
        capturedNewEvent!.TaskId.Should().Be(taskId);
        capturedNewEvent.HouseholdId.Should().Be(householdId);
        capturedNewEvent.Status.Should().Be("pending");
        capturedNewEvent.Priority.Should().Be("high"); // Inherited from task template

        // Next due date should be original due date + 1 month (not completion date!)
        var expectedNextDate = CreateDate(2024, 1, 15).AddMonths(1); // 2024-02-15
        capturedNewEvent.DueDate.Should().Be(expectedNextDate);
    }

    [Fact]
    public async Task CompleteEventAsync_WithYearsInterval_ShouldCalculateCorrectNextDate()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId,
            householdId: householdId,
            dueDate: CreateDate(2024, 3, 15)
        );

        // Task with yearly interval (e.g., annual car inspection)
        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            yearsValue: 1
        );

        var completeDto = new CompleteEventDto
        {
            CompletionDate = "2024-03-20"
        };

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        EventEntity? capturedNewEvent = null;
        _mockEventRepository
            .Setup(r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EventEntity, CancellationToken>((e, ct) => capturedNewEvent = e)
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid());

        // Assert
        capturedNewEvent.Should().NotBeNull();
        // Next date is based on original due date, not completion date
        var expectedNextDate = CreateDate(2025, 3, 15); // DueDate (2024-03-15) + 1 year
        capturedNewEvent!.DueDate.Should().Be(expectedNextDate);
    }

    [Fact]
    public async Task CompleteEventAsync_WithCombinedIntervals_ShouldAddAllIntervals()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId,
            dueDate: CreateDate(2024, 1, 1)
        );

        // Task with combined interval: 1 year, 2 months, 1 week, 3 days
        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            yearsValue: 1,
            monthsValue: 2,
            weeksValue: 1,
            daysValue: 3
        );

        var completeDto = new CompleteEventDto
        {
            CompletionDate = "2024-01-01"
        };

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        EventEntity? capturedNewEvent = null;
        _mockEventRepository
            .Setup(r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EventEntity, CancellationToken>((e, ct) => capturedNewEvent = e)
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid());

        // Assert
        capturedNewEvent.Should().NotBeNull();
        // 2024-01-01 + 1 year = 2025-01-01
        // + 2 months = 2025-03-01
        // + 1 week (7 days) = 2025-03-08
        // + 3 days = 2025-03-11
        var expectedNextDate = CreateDate(2025, 3, 11);
        capturedNewEvent!.DueDate.Should().Be(expectedNextDate);
    }

    [Fact]
    public async Task CompleteEventAsync_WithPremiumPlan_ShouldArchiveToHistory()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var completedBy = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId,
            householdId: householdId,
            status: "pending"
        );

        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            name: "Car service"
        );

        var premiumPlan = TestDataBuilder.CreatePremiumPlanType("Premium");
        var household = TestDataBuilder.CreateHousehold(
            id: householdId,
            planType: premiumPlan
        );

        var completeDto = new CompleteEventDto
        {
            CompletionDate = "2024-02-15",
            Notes = "All done"
        };

        // Mock repositories
        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync(household);

        EventHistoryEntity? capturedHistory = null;
        _mockEventHistoryRepository
            .Setup(r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EventHistoryEntity, CancellationToken>((h, ct) => capturedHistory = h)
            .ReturnsAsync((EventHistoryEntity h, CancellationToken ct) => h);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, completedBy);

        // Assert
        _mockEventHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Premium households should archive to events_history");

        capturedHistory.Should().NotBeNull();
        capturedHistory!.EventId.Should().Be(eventId);
        capturedHistory.TaskId.Should().Be(taskId);
        capturedHistory.HouseholdId.Should().Be(householdId);
        capturedHistory.CompletedBy.Should().Be(completedBy);
        capturedHistory.TaskName.Should().Be("Car service");
        capturedHistory.CompletionNotes.Should().Be("All done");
        capturedHistory.CompletionDate.Should().Be(DateOnly.Parse("2024-02-15"));
    }

    [Fact]
    public async Task CompleteEventAsync_WithRodzinnyPlan_ShouldArchiveToHistory()
    {
        // Arrange - Test that "Rodzinny" plan (family plan) also gets history
        var eventId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var completedBy = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            householdId: householdId
        );

        var rodzinnyPlan = TestDataBuilder.CreatePremiumPlanType("Rodzinny");
        var household = TestDataBuilder.CreateHousehold(
            id: householdId,
            planType: rodzinnyPlan
        );

        var completeDto = new CompleteEventDto();

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync(household);

        _mockEventHistoryRepository
            .Setup(r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventHistoryEntity h, CancellationToken ct) => h);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, completedBy);

        // Assert
        _mockEventHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Rodzinny plan should also archive to events_history");
    }

    [Fact]
    public async Task CompleteEventAsync_WithFreePlan_ShouldNotArchiveToHistory()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            householdId: householdId
        );

        var freePlan = TestDataBuilder.CreateFreePlanType();
        var household = TestDataBuilder.CreateHousehold(
            id: householdId,
            planType: freePlan
        );

        var completeDto = new CompleteEventDto();

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync(household);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid());

        // Assert
        _mockEventHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Free plan should NOT archive to events_history");
    }

    [Fact]
    public async Task CompleteEventAsync_NonexistentEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var completeDto = new CompleteEventDto();

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CompleteEventAsync_SoftDeletedEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var completeDto = new CompleteEventDto();

        var deletedEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            deletedAt: DateTimeOffset.UtcNow
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedEvent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CompleteEventAsync_AlreadyCompletedEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var completeDto = new CompleteEventDto();

        var completedEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            status: "completed",
            completionDate: CreateDate(2024, 1, 15)
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedEvent);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid()));

        exception.Message.Should().Contain("already completed");
    }

    [Fact]
    public async Task CompleteEventAsync_NoCompletionDate_ShouldUseToday()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = TestDataBuilder.CreateEvent(id: eventId);

        var completeDto = new CompleteEventDto
        {
            CompletionDate = null // No date provided
        };

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid());

        // Assert
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        existingEvent.CompletionDate.Should().Be(today);
    }

    [Fact]
    public async Task CompleteEventAsync_WithoutRecurringInterval_ShouldNotCreateNextEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId
        );

        // Task without any interval values
        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            yearsValue: null,
            monthsValue: null,
            weeksValue: null,
            daysValue: null
        );

        var completeDto = new CompleteEventDto();

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, completeDto, Guid.NewGuid());

        // Assert
        _mockEventRepository.Verify(
            r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "No recurring event should be created when task has no interval");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task CompleteEventAsync_ShouldLogInformation_WhenEventIsCompleted()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = TestDataBuilder.CreateEvent(id: eventId);

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Information,
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task CompleteEventAsync_WithRecurringTask_ShouldLogRecurringEventCreation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId
        );

        var taskTemplate = TestDataBuilder.CreateTask(
            id: taskId,
            monthsValue: 1
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        _mockEventRepository
            .Setup(r => r.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert - Should log both event completion and recurring event creation
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Information,
            Times.AtLeast(2)); // At least: event completed + recurring event created
    }

    [Fact]
    public async Task CompleteEventAsync_WithDeletedTaskTemplate_ShouldLogWarning()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId
        );

        // Task template is soft-deleted
        var deletedTask = TestDataBuilder.CreateTask(
            id: taskId,
            monthsValue: 1,
            deletedAt: DateTimeOffset.UtcNow
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedTask);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert - Should log warning about missing task template
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Warning,
            Times.Once());
    }

    [Fact]
    public async Task CompleteEventAsync_WithPremiumPlan_ShouldLogEventHistoryCreation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            taskId: taskId,
            householdId: householdId
        );

        var taskTemplate = TestDataBuilder.CreateTask(id: taskId);
        var premiumPlan = TestDataBuilder.CreatePremiumPlanType("Premium");
        var household = TestDataBuilder.CreateHousehold(
            id: householdId,
            planType: premiumPlan
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockTaskRepository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskTemplate);

        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync(household);

        _mockEventHistoryRepository
            .Setup(r => r.AddAsync(It.IsAny<EventHistoryEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventHistoryEntity h, CancellationToken ct) => h);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert - Should log event history creation
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Information,
            Times.AtLeast(2)); // Event completed + event history created
    }

    [Fact]
    public async Task CompleteEventAsync_WithFreePlan_ShouldLogDebug_SkippingEventHistory()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            householdId: householdId
        );

        var freePlan = TestDataBuilder.CreateFreePlanType();
        var household = TestDataBuilder.CreateHousehold(
            id: householdId,
            planType: freePlan
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync(household);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert - Should log debug message about skipping event history
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Debug,
            Times.Once());
    }

    [Fact]
    public async Task CompleteEventAsync_WithNullHousehold_ShouldLogWarning()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var existingEvent = TestDataBuilder.CreateEvent(
            id: eventId,
            householdId: householdId
        );

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEvent);

        // Household not found (returns null)
        _mockHouseholdRepository
            .Setup(r => r.GetByIdAsync(
                householdId,
                It.IsAny<Expression<Func<HouseholdEntity, object>>>()))
            .ReturnsAsync((HouseholdEntity?)null);

        _mockEventRepository
            .Setup(r => r.UpdateAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity e, CancellationToken ct) => e);

        // Act
        await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid());

        // Assert - Should log warning about missing household
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Warning,
            Times.Once());
    }

    [Fact]
    public async Task CompleteEventAsync_NonexistentEvent_ShouldLogError()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _mockEventRepository
            .Setup(r => r.GetWithDetailsAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _eventService.CompleteEventAsync(eventId, new CompleteEventDto(), Guid.NewGuid()));

        // Verify error was logged
        VerifyLoggerWasCalled(
            _mockLogger,
            LogLevel.Error,
            Times.Once());
    }

    #endregion
}
