using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for postponing an event to a new date
/// </summary>
public class PostponeEventDto
{
    /// <summary>
    /// New due date for the event (ISO 8601 format)
    /// </summary>
    [Required(ErrorMessage = "New due date is required")]
    public string NewDueDate { get; set; } = string.Empty;

    /// <summary>
    /// Reason for postponing the event (required)
    /// </summary>
    [Required(ErrorMessage = "Postponement reason is required")]
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
