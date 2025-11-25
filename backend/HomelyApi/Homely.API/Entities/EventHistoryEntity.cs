using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

/// <summary>
/// Event History Entity - archival record of completed events (premium feature).
/// Maps to the 'events_history' table in the database.
/// Stores a snapshot of completed event data for analytics and historical tracking.
/// </summary>
[Table("events_history")]
public class EventHistoryEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the event that was completed
    /// </summary>
    [Column("event_id")]
    public Guid? EventId { get; set; }

    /// <summary>
    /// Reference to the task template (if the event was based on a template)
    /// </summary>
    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    [Column("completed_by")]
    public Guid? CompletedBy { get; set; }

    [Required]
    [Column("due_date")]
    public DateOnly DueDate { get; set; }

    [Required]
    [Column("completion_date")]
    public DateOnly CompletionDate { get; set; }

    /// <summary>
    /// Snapshot of task name at completion time
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("task_name")]
    public string TaskName { get; set; } = string.Empty;

    [Column("completion_notes")]
    public string? CompletionNotes { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("EventId")]
    public virtual EventEntity? Event { get; set; }

    [ForeignKey("TaskId")]
    public virtual TaskEntity? Task { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;
}

