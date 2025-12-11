using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homely.API.Entities;

[Table("category_types")]
public class CategoryTypeEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("household_id")]
    public Guid HouseholdId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [ForeignKey("HouseholdId")]
    public virtual HouseholdEntity Household { get; set; } = null!;

    public virtual ICollection<CategoryEntity> Categories { get; set; } = new List<CategoryEntity>();
}
