using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.SystemUsers;

/// <summary>
/// DTO for creating new user
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// User email - must be unique
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password - minimum 8 characters
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Initial role for the user
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "member";

    /// <summary>
    /// Household ID to assign user to (optional - user can be created without a household)
    /// </summary>
    public Guid? HouseholdId { get; set; }

    /// <summary>
    /// Optional phone number
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Send welcome email to user
    /// </summary>
    public bool SendWelcomeEmail { get; set; } = true;
}
