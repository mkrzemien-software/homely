using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Item ID (optional - task can exist without item)
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// Household ID
    /// </summary>
    [Required(ErrorMessage = "Household ID is required")]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// User ID to assign this task to (optional)
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Due date for the task
    /// </summary>
    [Required(ErrorMessage = "Due date is required")]
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Task title
    /// </summary>
    [Required(ErrorMessage = "Task title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Task priority: low, medium, high
    /// </summary>
    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Priority must be 'low', 'medium', or 'high'")]
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Whether task should automatically regenerate when completed
    /// </summary>
    public bool IsRecurring { get; set; } = true;

    /// <summary>
    /// User ID creating the task (will be set from authentication context)
    /// </summary>
    [Required(ErrorMessage = "Created by user ID is required")]
    public Guid CreatedBy { get; set; }
}
