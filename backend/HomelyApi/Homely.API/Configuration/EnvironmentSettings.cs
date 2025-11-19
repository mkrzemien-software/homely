namespace Homely.API.Configuration;

/// <summary>
/// Environment configuration settings
/// </summary>
public class EnvironmentSettings
{
    public const string SectionName = "Environment";
    
    /// <summary>
    /// Environment name (Local, Development, Production)
    /// </summary>
    public string Name { get; set; } = "Unknown";
    
    /// <summary>
    /// Environment description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Determines if the environment is production
    /// </summary>
    public bool IsProduction => Name?.Equals("Production", StringComparison.OrdinalIgnoreCase) ?? false;
    
    /// <summary>
    /// Determines if the environment is development
    /// </summary>
    public bool IsDevelopment => Name?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
    
    /// <summary>
    /// Determines if the environment is local
    /// </summary>
    public bool IsLocal => Name?.Equals("Local", StringComparison.OrdinalIgnoreCase) ?? false;
}

