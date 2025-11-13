using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for postponing an event to a new date
/// </summary>
public class PostponeEventDto
{
    /// <summary>
    /// New due date for the event
    /// </summary>
    [Required(ErrorMessage = "New due date is required")]
    public DateOnly NewDueDate { get; set; }

    /// <summary>
    /// Reason for postponing the event
    /// </summary>
    [Required(ErrorMessage = "Postpone reason is required")]
    [MaxLength(500, ErrorMessage = "Postpone reason cannot exceed 500 characters")]
    public string PostponeReason { get; set; } = string.Empty;
}
