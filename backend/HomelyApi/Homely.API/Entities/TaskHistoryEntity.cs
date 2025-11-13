using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

/// <summary>
/// Task History Entity - archival record of completed events (premium feature).
/// Maps to the 'tasks_history' table in the database.
/// </summary>
[Table("tasks_history")]
public class TaskHistoryEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the event that was completed (maps to task_id column for backward compatibility)
    /// </summary>
    [Column("task_id")]
    public Guid? EventId { get; set; }

    /// <summary>
    /// Reference to the task template (maps to item_id column for backward compatibility)
    /// </summary>
    [Column("item_id")]
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

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

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
