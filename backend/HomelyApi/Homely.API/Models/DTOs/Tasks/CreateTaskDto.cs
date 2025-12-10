using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for creating a new task template
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Household ID
    /// </summary>
    [Required(ErrorMessage = "Household ID is required")]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Category ID (optional)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Task name
    /// </summary>
    [Required(ErrorMessage = "Task name is required")]
    [MaxLength(100, ErrorMessage = "Task name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Interval in years for recurring events
    /// </summary>
    [Range(0, 100, ErrorMessage = "Years value must be between 0 and 100")]
    public int? YearsValue { get; set; }

    /// <summary>
    /// Interval in months for recurring events
    /// </summary>
    [Range(0, 1200, ErrorMessage = "Months value must be between 0 and 1200")]
    public int? MonthsValue { get; set; }

    /// <summary>
    /// Interval in weeks for recurring events
    /// </summary>
    [Range(0, 520, ErrorMessage = "Weeks value must be between 0 and 520")]
    public int? WeeksValue { get; set; }

    /// <summary>
    /// Interval in days for recurring events
    /// </summary>
    [Range(0, 3650, ErrorMessage = "Days value must be between 0 and 3650")]
    public int? DaysValue { get; set; }

    /// <summary>
    /// Last date this task was completed (optional)
    /// </summary>
    public DateOnly? LastDate { get; set; }

    /// <summary>
    /// Priority level: low, medium, high
    /// </summary>
    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Priority must be 'low', 'medium', or 'high'")]
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Whether task is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default user assignment for events generated from this task template (optional)
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// User ID creating the task (will be set from authentication context)
    /// </summary>
    [Required(ErrorMessage = "Created by user ID is required")]
    public Guid CreatedBy { get; set; }
}
