using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for marking an event as completed
/// </summary>
public class CompleteEventDto
{
    /// <summary>
    /// Date when the event was completed (optional, defaults to today)
    /// </summary>
    public string? CompletionDate { get; set; }

    /// <summary>
    /// Optional notes about the completion
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Attachment URL (optional)
    /// </summary>
    public string? AttachmentUrl { get; set; }
}
