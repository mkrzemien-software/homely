using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("households")]
public class HouseholdEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [Column("plan_type_id")]
    public int PlanTypeId { get; set; } = 1;

    [Required]
    [MaxLength(20)]
    [Column("subscription_status")]
    public string SubscriptionStatus { get; set; } = "free";

    [Column("subscription_start_date")]
    public DateOnly? SubscriptionStartDate { get; set; }

    [Column("subscription_end_date")]
    public DateOnly? SubscriptionEndDate { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [ForeignKey("PlanTypeId")]
    public virtual PlanTypeEntity PlanType { get; set; } = null!;

    public virtual ICollection<HouseholdMemberEntity> HouseholdMembers { get; set; } = new List<HouseholdMemberEntity>();
    public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    public virtual ICollection<EventEntity> Events { get; set; } = new List<EventEntity>();
    public virtual ICollection<EventHistoryEntity> EventsHistory { get; set; } = new List<EventHistoryEntity>();
    public virtual ICollection<PlanUsageEntity> PlanUsages { get; set; } = new List<PlanUsageEntity>();
}
