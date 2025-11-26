using Homely.API.Repositories.Base;
using Homely.API.Models.Constants;

namespace Homely.API.Services;

/// <summary>
/// Service for tracking and managing household subscription plan usage.
/// Replaces database triggers for plan usage tracking.
/// </summary>
public class PlanUsageService : IPlanUsageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlanUsageService> _logger;

    public PlanUsageService(
        IUnitOfWork _unitOfWork,
        ILogger<PlanUsageService> logger)
    {
        this._unitOfWork = _unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task UpdateTasksUsageAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Count active tasks for the household
            var taskCount = await _unitOfWork.Tasks.CountActiveTasksAsync(householdId, cancellationToken);

            // Get household with plan details
            var household = await _unitOfWork.Households.GetByIdAsync(
                householdId,
                h => h.PlanType);

            if (household == null)
            {
                _logger.LogWarning("Cannot update tasks usage: Household {HouseholdId} not found", householdId);
                return;
            }

            var maxTasks = household.PlanType?.MaxTasks;

            // Update or create plan usage record
            await _unitOfWork.PlanUsages.UpdateOrCreateUsageAsync(
                householdId,
                DatabaseConstants.PlanUsageTypes.Tasks,
                taskCount,
                maxTasks,
                cancellationToken);

            _logger.LogInformation(
                "Updated tasks usage for household {HouseholdId}: {TaskCount}/{MaxTasks}",
                householdId,
                taskCount,
                maxTasks?.ToString() ?? "unlimited");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tasks usage for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateMembersUsageAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all household members and count active ones
            var members = await _unitOfWork.HouseholdMembers.GetHouseholdMembersAsync(householdId, cancellationToken);
            var memberCount = members.Count(m => m.DeletedAt == null);

            // Get household with plan details
            var household = await _unitOfWork.Households.GetByIdAsync(
                householdId,
                h => h.PlanType);

            if (household == null)
            {
                _logger.LogWarning("Cannot update members usage: Household {HouseholdId} not found", householdId);
                return;
            }

            var maxMembers = household.PlanType?.MaxHouseholdMembers;

            // Update or create plan usage record
            await _unitOfWork.PlanUsages.UpdateOrCreateUsageAsync(
                householdId,
                DatabaseConstants.PlanUsageTypes.HouseholdMembers,
                memberCount,
                maxMembers,
                cancellationToken);

            _logger.LogInformation(
                "Updated members usage for household {HouseholdId}: {MemberCount}/{MaxMembers}",
                householdId,
                memberCount,
                maxMembers?.ToString() ?? "unlimited");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating members usage for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsLimitExceededAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.PlanUsages.IsLimitExceededAsync(householdId, usageType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if limit exceeded for household {HouseholdId}, usage type {UsageType}", householdId, usageType);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> WouldExceedLimitAsync(Guid householdId, string usageType, CancellationToken cancellationToken = default)
    {
        try
        {
            var usage = await _unitOfWork.PlanUsages.GetUsageByTypeAsync(householdId, usageType, cancellationToken: cancellationToken);

            // If no usage record exists or no max value (unlimited), cannot exceed
            if (usage == null || usage.MaxValue == null)
            {
                return false;
            }

            // Check if adding one more would exceed the limit
            return (usage.CurrentValue + 1) > usage.MaxValue.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if would exceed limit for household {HouseholdId}, usage type {UsageType}", householdId, usageType);
            throw;
        }
    }
}
