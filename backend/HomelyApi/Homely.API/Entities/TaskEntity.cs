using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("tasks")]
public class TaskEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("item_id")]
    public Guid? ItemId { get; set; }

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    [Required]
    [Column("due_date")]
    public DateOnly DueDate { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [MaxLength(10)]
    [Column("priority")]
    public string Priority { get; set; } = "medium";

    [Column("completion_date")]
    public DateOnly? CompletionDate { get; set; }

    [Column("completion_notes")]
    public string? CompletionNotes { get; set; }

    [Column("postponed_from_date")]
    public DateOnly? PostponedFromDate { get; set; }

    [Column("postpone_reason")]
    public string? PostponeReason { get; set; }

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

    [ForeignKey("ItemId")]
    public virtual ItemEntity? Item { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;

    public virtual ICollection<TaskHistoryEntity> TasksHistory { get; set; } = new List<TaskHistoryEntity>();
}
