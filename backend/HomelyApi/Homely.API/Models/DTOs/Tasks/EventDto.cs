namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// Event data transfer object - represents a scheduled occurrence of a task
/// </summary>
public class EventDto
{
    /// <summary>
    /// Event ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Task template ID (optional - event can exist without template)
    /// </summary>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Task template name
    /// </summary>
    public string? TaskName { get; set; }

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
    /// Due date for the event
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Event title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    public string? Description { get; set; }

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
    /// Whether event should automatically regenerate when completed
    /// </summary>
    public bool IsRecurring { get; set; }

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
