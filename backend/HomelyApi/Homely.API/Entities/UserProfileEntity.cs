using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

/// <summary>
/// User profile entity with extended information beyond Supabase auth.users
/// Maps to user_profiles table in database
/// </summary>
[Table("user_profiles")]
public class UserProfileEntity
{
    /// <summary>
    /// User ID - references auth.users(id) in Supabase
    /// </summary>
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// URL to user's avatar image
    /// </summary>
    [MaxLength(500)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    [MaxLength(20)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's preferred language (ISO 639-1 code)
    /// </summary>
    [MaxLength(5)]
    [Column("preferred_language")]
    public string PreferredLanguage { get; set; } = "pl";

    /// <summary>
    /// User's timezone
    /// </summary>
    [MaxLength(50)]
    [Column("timezone")]
    public string Timezone { get; set; } = "Europe/Warsaw";

    /// <summary>
    /// Last time user was active in the system
    /// </summary>
    [Column("last_active_at")]
    public DateTimeOffset? LastActiveAt { get; set; }

    /// <summary>
    /// Profile creation timestamp
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Profile last update timestamp
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<HouseholdMemberEntity> HouseholdMemberships { get; set; } = new List<HouseholdMemberEntity>();
}
