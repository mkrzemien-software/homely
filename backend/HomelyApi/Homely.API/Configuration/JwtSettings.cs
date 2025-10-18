namespace Homely.API.Configuration
{
    /// <summary>
    /// JWT authentication settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// JWT token issuer
        /// </summary>
        public string ValidIssuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT token audience
        /// </summary>
        public string ValidAudience { get; set; } = string.Empty;

        /// <summary>
        /// JWT signing secret
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in minutes (default: 60 minutes according to Supabase)
        /// </summary>
        public int ExpirationInMinutes { get; set; } = 60;
    }
}
