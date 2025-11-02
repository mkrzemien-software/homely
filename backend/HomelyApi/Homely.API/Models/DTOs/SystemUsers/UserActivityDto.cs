namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// User activity log entry
/// </summary>
public class UserActivityDto
{
    /// <summary>
    /// Activity ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Action type/name
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the action
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// IP address from which action was performed
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }
}
