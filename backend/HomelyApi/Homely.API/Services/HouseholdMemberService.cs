using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;

namespace Homely.API.Services;

/// <summary>
/// Service for household member management.
/// Integrates with PlanUsageService to track membership usage.
/// </summary>
public class HouseholdMemberService : IHouseholdMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPlanUsageService _planUsageService;
    private readonly ILogger<HouseholdMemberService> _logger;

    public HouseholdMemberService(
        IUnitOfWork unitOfWork,
        IPlanUsageService planUsageService,
        ILogger<HouseholdMemberService> logger)
    {
        _unitOfWork = unitOfWork;
        _planUsageService = planUsageService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<HouseholdMemberEntity> AddMemberAsync(Guid householdId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate role
            if (!DatabaseConstants.HouseholdRoles.AllRoles.Contains(role))
            {
                throw new ArgumentException($"Invalid role: {role}. Must be one of: {string.Join(", ", DatabaseConstants.HouseholdRoles.AllRoles)}");
            }

            // Check if member already exists
            var existingMember = await _unitOfWork.HouseholdMembers.GetMembershipAsync(householdId, userId, cancellationToken);
            if (existingMember != null && existingMember.DeletedAt == null)
            {
                throw new InvalidOperationException($"User {userId} is already a member of household {householdId}");
            }

            // Check if adding this member would exceed plan limit
            if (await _planUsageService.WouldExceedLimitAsync(householdId, DatabaseConstants.PlanUsageTypes.HouseholdMembers, cancellationToken))
            {
                throw new InvalidOperationException($"Cannot add member: Household {householdId} has reached its plan limit for members. Please upgrade to a premium plan.");
            }

            // If member was previously deleted, restore them
            if (existingMember != null && existingMember.DeletedAt != null)
            {
                existingMember.DeletedAt = null;
                existingMember.Role = role;
                existingMember.JoinedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.HouseholdMembers.UpdateAsync(existingMember, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Restored member {UserId} to household {HouseholdId} with role {Role}", userId, householdId, role);

                // Update plan usage tracking (replaces database trigger)
                await _planUsageService.UpdateMembersUsageAsync(householdId, cancellationToken);

                return existingMember;
            }

            // Create new member
            var member = new HouseholdMemberEntity
            {
                HouseholdId = householdId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.HouseholdMembers.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added member {UserId} to household {HouseholdId} with role {Role}", userId, householdId, role);

            // Update plan usage tracking (replaces database trigger)
            await _planUsageService.UpdateMembersUsageAsync(householdId, cancellationToken);

            return member;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member {UserId} to household {HouseholdId}", userId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _unitOfWork.HouseholdMembers.GetMembershipAsync(householdId, userId, cancellationToken);

            if (member == null || member.DeletedAt != null)
            {
                _logger.LogWarning("Cannot remove member: User {UserId} is not a member of household {HouseholdId}", userId, householdId);
                return false;
            }

            // Soft delete
            member.DeletedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.HouseholdMembers.UpdateAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed member {UserId} from household {HouseholdId}", userId, householdId);

            // Update plan usage tracking (replaces database trigger)
            await _planUsageService.UpdateMembersUsageAsync(householdId, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member {UserId} from household {HouseholdId}", userId, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<HouseholdMemberEntity> UpdateMemberRoleAsync(Guid householdId, Guid userId, string newRole, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate role
            if (!DatabaseConstants.HouseholdRoles.AllRoles.Contains(newRole))
            {
                throw new ArgumentException($"Invalid role: {newRole}. Must be one of: {string.Join(", ", DatabaseConstants.HouseholdRoles.AllRoles)}");
            }

            var member = await _unitOfWork.HouseholdMembers.GetMembershipAsync(householdId, userId, cancellationToken);

            if (member == null || member.DeletedAt != null)
            {
                throw new InvalidOperationException($"User {userId} is not a member of household {householdId}");
            }

            member.Role = newRole;

            await _unitOfWork.HouseholdMembers.UpdateAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated role for member {UserId} in household {HouseholdId} to {Role}", userId, householdId, newRole);

            return member;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for member {UserId} in household {HouseholdId}", userId, householdId);
            throw;
        }
    }
}
