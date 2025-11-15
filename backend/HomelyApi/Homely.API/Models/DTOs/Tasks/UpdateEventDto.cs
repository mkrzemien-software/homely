using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for updating an existing event
/// NOTE: Events cannot change their associated task - title comes from the task
/// </summary>
public class UpdateEventDto
{
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
    /// Event status: pending, completed, postponed, cancelled
    /// </summary>
    [RegularExpression("^(pending|completed|postponed|cancelled)$", ErrorMessage = "Status must be 'pending', 'completed', 'postponed', or 'cancelled'")]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Event priority: low, medium, high
    /// </summary>
    [RegularExpression("^(low|medium|high)$", ErrorMessage = "Priority must be 'low', 'medium', or 'high'")]
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Optional notes for this event
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
