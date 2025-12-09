namespace Homely.API.Models.Configuration;

/// <summary>
/// Configuration settings for event generation and replenishment
/// </summary>
public class EventGenerationSettings
{
    public const string SectionName = "EventGenerationSettings";

    /// <summary>
    /// Number of years into the future to generate events for.
    /// Events will be generated from today up to (today + FutureYears).
    /// This ensures consistent coverage regardless of task interval.
    /// Default: 2 years
    /// </summary>
    public int FutureYears { get; set; } = 2;

    /// <summary>
    /// Minimum number of months of future events before triggering refill.
    /// Used by the refill workflow to determine if a task needs more events.
    /// If the furthest future event is less than this many months away, generate more.
    /// Default: 6 months
    /// </summary>
    public int MinFutureMonthsThreshold { get; set; } = 6;
}
