namespace Homely.API.Services;

/// <summary>
/// Service for tracking and managing household subscription plan usage
/// </summary>
public interface IPlanUsageService
{
    /// <summary>
    /// Updates the tasks usage count for a household
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateTasksUsageAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the household members usage count for a household
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateMembersUsageAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a household has exceeded its usage limit for a specific usage type
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="usageType">The usage type (e.g., "tasks", "household_members")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the limit is exceeded, false otherwise</returns>
    Task<bool> IsLimitExceededAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if adding one more item would exceed the limit
    /// </summary>
    /// <param name="householdId">The household ID</param>
    /// <param name="usageType">The usage type (e.g., "tasks", "household_members")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if adding one more would exceed limit, false otherwise</returns>
    Task<bool> WouldExceedLimitAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default);
}
