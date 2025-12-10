using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

/// <summary>
/// Task Entity - represents a task template that defines "what" and "how often".
/// Maps to the 'tasks' table in the database.
/// These are templates from which Events (concrete occurrences) are created.
/// </summary>
[Table("tasks")]
public class TaskEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Interval in years for recurring events
    /// </summary>
    [Column("years_value")]
    public int? YearsValue { get; set; }

    /// <summary>
    /// Interval in months for recurring events
    /// </summary>
    [Column("months_value")]
    public int? MonthsValue { get; set; }

    /// <summary>
    /// Interval in weeks for recurring events
    /// </summary>
    [Column("weeks_value")]
    public int? WeeksValue { get; set; }

    /// <summary>
    /// Interval in days for recurring events
    /// </summary>
    [Column("days_value")]
    public int? DaysValue { get; set; }

    /// <summary>
    /// Last date this task was completed (optional)
    /// </summary>
    [Column("last_date")]
    public DateOnly? LastDate { get; set; }

    /// <summary>
    /// Priority level: low, medium, high
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Column("priority")]
    public string Priority { get; set; } = "medium";

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default user assignment for events generated from this task template (optional)
    /// </summary>
    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

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
    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public virtual CategoryEntity? Category { get; set; }

    [ForeignKey("CreatedBy")]
    public virtual UserProfileEntity CreatedByUser { get; set; } = null!;

    [ForeignKey("AssignedTo")]
    public virtual UserProfileEntity? AssignedToUser { get; set; }

    /// <summary>
    /// Events (concrete occurrences) generated from this task template
    /// </summary>
    public virtual ICollection<EventEntity> Events { get; set; } = new List<EventEntity>();
}
