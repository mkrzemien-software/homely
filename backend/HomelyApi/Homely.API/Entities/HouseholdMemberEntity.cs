using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("household_members")]
public class HouseholdMemberEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("invited_by")]
    public Guid? InvitedBy { get; set; }

    [MaxLength(100)]
    [Column("invitation_token")]
    public string? InvitationToken { get; set; }

    [Column("invitation_expires_at")]
    public DateTimeOffset? InvitationExpiresAt { get; set; }

    [Column("joined_at")]
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;
}
