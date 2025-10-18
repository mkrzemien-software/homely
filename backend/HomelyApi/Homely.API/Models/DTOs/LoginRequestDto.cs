using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs
{
    /// <summary>
    /// DTO for login request containing user credentials
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// User's email address (required)
        /// </summary>
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Niepoprawny format email")]
        [MaxLength(100, ErrorMessage = "Email nie może być dłuższy niż 100 znaków")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password (required)
        /// </summary>
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [MinLength(8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Remember me option for extended session
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
