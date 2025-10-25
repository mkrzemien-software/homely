using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("items")]
public class ItemEntity
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

    [Column("years_value")]
    public int? YearsValue { get; set; }

    [Column("months_value")]
    public int? MonthsValue { get; set; }

    [Column("weeks_value")]
    public int? WeeksValue { get; set; }

    [Column("days_value")]
    public int? DaysValue { get; set; }

    [Column("last_date")]
    public DateOnly? LastDate { get; set; }

    [MaxLength(10)]
    [Column("priority")]
    public string Priority { get; set; } = "medium";

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public virtual CategoryEntity? Category { get; set; }

    public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    public virtual ICollection<TaskHistoryEntity> TasksHistory { get; set; } = new List<TaskHistoryEntity>();
}
