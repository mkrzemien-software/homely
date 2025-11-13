using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

/// <summary>
/// Event Entity - represents a concrete scheduled occurrence of a task.
/// Maps to the 'events' table in the database.
/// Events are created from Task templates and represent specific scheduled appointments/occurrences.
/// </summary>
[Table("events")]
public class EventEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the task template (TaskEntity).
    /// This is the template that defines "what" and "how often".
    /// </summary>
    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// User responsible for completing this event
    /// </summary>
    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// When this event is due
    /// </summary>
    [Required]
    [Column("due_date")]
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Event title/name (usually inherited from task template)
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Additional description or notes for this specific event
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Event status: pending, completed, postponed, cancelled
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Priority level: low, medium, high
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Column("priority")]
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// When the event was actually completed
    /// </summary>
    [Column("completion_date")]
    public DateOnly? CompletionDate { get; set; }

    /// <summary>
    /// Notes added when completing the event
    /// </summary>
    [Column("completion_notes")]
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// Original due date if the event was postponed
    /// </summary>
    [Column("postponed_from_date")]
    public DateOnly? PostponedFromDate { get; set; }

    /// <summary>
    /// Reason for postponing the event
    /// </summary>
    [Column("postpone_reason")]
    public string? PostponeReason { get; set; }

    /// <summary>
    /// Whether this event should auto-generate the next occurrence when completed
    /// </summary>
    [Column("is_recurring")]
    public bool IsRecurring { get; set; } = true;

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("TaskId")]
    public virtual TaskEntity? Task { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;

    [ForeignKey("AssignedTo")]
    public virtual UserProfileEntity? AssignedToUser { get; set; }

    [ForeignKey("CreatedBy")]
    public virtual UserProfileEntity CreatedByUser { get; set; } = null!;
}
