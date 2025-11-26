namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// Event data transfer object - represents a scheduled occurrence of a task
/// NOTE: Events do NOT have their own title - they display the associated task's name
/// </summary>
public class EventDto
{
    /// <summary>
    /// Event ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Task template ID (required - events must be based on a task)
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Task template name - this is displayed as the event's title
    /// Events do NOT have their own title field
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// Household ID
    /// </summary>
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Household name
    /// </summary>
    public string? HouseholdName { get; set; }

    /// <summary>
    /// User ID assigned to this event
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Assigned user first name
    /// </summary>
    public string? AssignedToFirstName { get; set; }

    /// <summary>
    /// Assigned user last name
    /// </summary>
    public string? AssignedToLastName { get; set; }

    /// <summary>
    /// Category ID from the task
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Category name from the task
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Category type name from the task
    /// </summary>
    public string? CategoryTypeName { get; set; }

    /// <summary>
    /// Due date for the event
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Event status: pending, completed, postponed, cancelled
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Event priority: low, medium, high
    /// </summary>
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Completion date (if completed)
    /// </summary>
    public DateOnly? CompletionDate { get; set; }

    /// <summary>
    /// Notes added when event was completed
    /// </summary>
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// Original due date if event was postponed
    /// </summary>
    public DateOnly? PostponedFromDate { get; set; }

    /// <summary>
    /// Reason for postponing the event
    /// </summary>
    public string? PostponeReason { get; set; }

    /// <summary>
    /// Optional notes for this event
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User ID who created the event
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
    /// Indicates if event is overdue
    /// </summary>
    public bool IsOverdue => Status == "pending" && DueDate < DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Days until due date (negative if overdue)
    /// </summary>
    public int DaysUntilDue => DueDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
}
