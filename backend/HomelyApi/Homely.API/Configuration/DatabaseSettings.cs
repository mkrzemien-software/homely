namespace Homely.API.Configuration;

/// <summary>
/// Database connection settings for different environments
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";
    
    /// <summary>
    /// Primary database connection string
    /// Can be overridden by environment variable: ConnectionStrings__DefaultConnection
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;
}

