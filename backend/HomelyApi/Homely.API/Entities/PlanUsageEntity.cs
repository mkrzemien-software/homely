using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("plan_usage")]
public class PlanUsageEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("usage_type")]
    public string UsageType { get; set; } = string.Empty;

    [Column("current_value")]
    public int CurrentValue { get; set; } = 0;

    [Column("max_value")]
    public int? MaxValue { get; set; }

    [Column("usage_date")]
    public DateOnly UsageDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;
}
