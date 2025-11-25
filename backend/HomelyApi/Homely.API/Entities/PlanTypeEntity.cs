using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Homely.API.Entities;

[Table("plan_types")]
public class PlanTypeEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("max_household_members")]
    public int? MaxHouseholdMembers { get; set; }

    [Column("max_tasks")]
    public int? MaxTasks { get; set; }

    [Column("price_monthly", TypeName = "decimal(10,2)")]
    public decimal? PriceMonthly { get; set; }

    [Column("price_yearly", TypeName = "decimal(10,2)")]
    public decimal? PriceYearly { get; set; }

    [Column("features", TypeName = "jsonb")]
    public string Features { get; set; } = "[]";

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual ICollection<HouseholdEntity> Households { get; set; } = new List<HouseholdEntity>();

    [NotMapped]
    public List<string> FeaturesList
    {
        get => JsonSerializer.Deserialize<List<string>>(Features) ?? new List<string>();
        set => Features = JsonSerializer.Serialize(value);
    }
}
