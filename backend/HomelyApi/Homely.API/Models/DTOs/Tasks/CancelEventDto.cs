using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for cancelling an event
/// </summary>
public class CancelEventDto
{
    /// <summary>
    /// Reason for cancelling the event (required)
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Cancellation reason is required")]
    [MinLength(1, ErrorMessage = "Cancellation reason cannot be empty")]
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
