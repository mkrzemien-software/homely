using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for marking an event as completed
/// </summary>
public class CompleteEventDto
{
    /// <summary>
    /// Date when the event was completed
    /// </summary>
    [Required(ErrorMessage = "Completion date is required")]
    public DateOnly CompletionDate { get; set; }

    /// <summary>
    /// Optional notes about the completion
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Completion notes cannot exceed 1000 characters")]
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// User ID who completed the event (will be set from authentication context)
    /// </summary>
    [Required(ErrorMessage = "Completed by user ID is required")]
    public Guid CompletedBy { get; set; }
}
