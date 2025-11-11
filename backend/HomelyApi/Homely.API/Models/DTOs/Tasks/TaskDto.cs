namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// Task data transfer object
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Task ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Item ID (optional - task can exist without item)
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// Item name
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// Household ID
    /// </summary>
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Household name
    /// </summary>
    public string? HouseholdName { get; set; }

    /// <summary>
    /// User ID assigned to this task
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Due date for the task
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Task status: pending, completed, postponed, cancelled
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Task priority: low, medium, high
    /// </summary>
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Completion date (if completed)
    /// </summary>
    public DateOnly? CompletionDate { get; set; }

    /// <summary>
    /// Notes added when task was completed
    /// </summary>
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// Original due date if task was postponed
    /// </summary>
    public DateOnly? PostponedFromDate { get; set; }

    /// <summary>
    /// Reason for postponing the task
    /// </summary>
    public string? PostponeReason { get; set; }

    /// <summary>
    /// Whether task should automatically regenerate when completed
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// User ID who created the task
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Indicates if task is overdue
    /// </summary>
    public bool IsOverdue => Status == "pending" && DueDate < DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Days until due date (negative if overdue)
    /// </summary>
    public int DaysUntilDue => DueDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
}
