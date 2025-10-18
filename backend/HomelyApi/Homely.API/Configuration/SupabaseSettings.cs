namespace Homely.API.Configuration
{
    /// <summary>
    /// Configuration settings for Supabase connection
    /// </summary>
    public class SupabaseSettings
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "Supabase";

        /// <summary>
        /// Supabase project URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Supabase anonymous API key
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
