using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for creating a new event
/// Events MUST be based on a task template - they don't have their own title.
/// The event's display name comes from the associated task's name.
/// </summary>
public class CreateEventDto
{
    /// <summary>
    /// Task template ID (REQUIRED - events must be based on a task template)
    /// The event will display the task's name as its title
    /// </summary>
    [Required(ErrorMessage = "Task ID is required - events must be based on a task template")]
    public Guid TaskId { get; set; }

    /// <summary>
    /// Household ID
    /// </summary>
    [Required(ErrorMessage = "Household ID is required")]
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// User ID to assign this event to (optional)
    /// </summary>
    public Guid? AssignedTo { get; set; }

    /// <summary>
    /// Due date for the event
    /// </summary>
    [Required(ErrorMessage = "Due date is required")]
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Event priority: low, medium, high (optional - defaults to task's priority if not specified)
    /// </summary>
    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Priority must be 'low', 'medium', or 'high'")]
    public string? Priority { get; set; }

    /// <summary>
    /// Optional notes for this specific event occurrence
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// User ID creating the event (will be set from authentication context)
    /// </summary>
    [Required(ErrorMessage = "Created by user ID is required")]
    public Guid CreatedBy { get; set; }
}
