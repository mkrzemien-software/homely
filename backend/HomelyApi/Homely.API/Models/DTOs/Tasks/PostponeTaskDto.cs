using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// DTO for postponing a task to a new date
/// </summary>
public class PostponeTaskDto
{
    /// <summary>
    /// New due date for the task
    /// </summary>
    [Required(ErrorMessage = "New due date is required")]
    public DateOnly NewDueDate { get; set; }

    /// <summary>
    /// Reason for postponing the task
    /// </summary>
    [Required(ErrorMessage = "Postpone reason is required")]
    [MaxLength(500, ErrorMessage = "Postpone reason cannot exceed 500 characters")]
    public string PostponeReason { get; set; } = string.Empty;
}
