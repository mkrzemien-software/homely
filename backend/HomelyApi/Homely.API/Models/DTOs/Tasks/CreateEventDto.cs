using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for creating a new event
/// </summary>
public class CreateEventDto
{
    /// <summary>
    /// Task template ID (optional - event can exist without template)
    /// </summary>
    public Guid? TaskId { get; set; }

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
    /// Event title
    /// </summary>
    [Required(ErrorMessage = "Event title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Event priority: low, medium, high
    /// </summary>
    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Priority must be 'low', 'medium', or 'high'")]
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Whether event should automatically regenerate when completed
    /// </summary>
    public bool IsRecurring { get; set; } = true;

    /// <summary>
    /// User ID creating the event (will be set from authentication context)
    /// </summary>
    [Required(ErrorMessage = "Created by user ID is required")]
    public Guid CreatedBy { get; set; }
}
