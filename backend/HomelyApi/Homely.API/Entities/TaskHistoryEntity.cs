using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("tasks_history")]
public class TaskHistoryEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("task_id")]
    public Guid? TaskId { get; set; }

    [Column("item_id")]
    public Guid? ItemId { get; set; }

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

    [ForeignKey("TaskId")]
    public virtual TaskEntity? Task { get; set; }

    [ForeignKey("ItemId")]
    public virtual ItemEntity? Item { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;
}
