using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Models.DTOs.SystemHouseholds;
using Homely.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service for system-level household management operations
/// </summary>
public class SystemHouseholdsService : ISystemHouseholdsService
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IHouseholdMemberRepository _householdMemberRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<SystemHouseholdsService> _logger;

    public SystemHouseholdsService(
        IHouseholdRepository householdRepository,
        IHouseholdMemberRepository householdMemberRepository,
        IUserProfileRepository userProfileRepository,
        ILogger<SystemHouseholdsService> logger)
    {
        _householdRepository = householdRepository;
        _householdMemberRepository = householdMemberRepository;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<PaginatedHouseholdsDto> SearchHouseholdsAsync(
        HouseholdSearchFiltersDto filters,
        CancellationToken cancellationToken = default)
    {
        var result = await _householdRepository.SearchHouseholdsAsync(
            filters.SearchTerm,
            filters.PlanTypeId,
            filters.SubscriptionStatus,
            filters.HasActiveMembers,
            filters.Page,
            filters.PageSize,
            cancellationToken);

        return new PaginatedHouseholdsDto
        {
            Households = result.Items.Select(MapToSystemHouseholdDto).ToList(),
            Total = result.TotalCount,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    public async Task<SystemHouseholdDetailsDto?> GetHouseholdDetailsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var household = await _householdRepository.GetHouseholdWithDetailsAsync(householdId, cancellationToken);
        if (household == null)
        {
            return null;
        }

        // Get member details with user profiles
        var memberDetails = new List<HouseholdMemberSummaryDto>();
        foreach (var member in household.HouseholdMembers.Where(hm => hm.DeletedAt == null))
        {
            var userProfile = await _userProfileRepository.GetByIdAsync(member.UserId, cancellationToken);
            if (userProfile != null)
            {
                // TODO: Email should be fetched from Supabase Auth API (auth.users table)
                // For now, we'll use a placeholder or the user ID as identifier
                memberDetails.Add(new HouseholdMemberSummaryDto
                {
                    Id = member.Id,
                    UserId = member.UserId,
                    Email = $"user-{member.UserId}@placeholder.com", // TODO: Fetch from Supabase Auth
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Role = member.Role,
                    JoinedAt = member.JoinedAt,
                    LastActiveAt = userProfile.LastActiveAt
                });
            }
        }

        return new SystemHouseholdDetailsDto
        {
            Id = household.Id,
            Name = household.Name,
            Address = household.Address,
            PlanTypeId = household.PlanTypeId,
            PlanName = household.PlanType.Name,
            SubscriptionStatus = household.SubscriptionStatus,
            SubscriptionStartDate = household.SubscriptionStartDate,
            SubscriptionEndDate = household.SubscriptionEndDate,
            CreatedAt = household.CreatedAt,
            UpdatedAt = household.UpdatedAt,
            MemberCount = household.HouseholdMembers.Count(hm => hm.DeletedAt == null),
            ItemCount = household.Items.Count(i => i.DeletedAt == null),
            TaskCount = household.Tasks.Count(t => t.DeletedAt == null),
            CompletedTasksCount = household.TasksHistory.Count(th => th.DeletedAt == null),
            LastActivityAt = household.HouseholdMembers
                .Where(hm => hm.DeletedAt == null)
                .Select(hm => hm.UpdatedAt)
                .DefaultIfEmpty()
                .Max(),
            MaxMembers = household.PlanType.MaxHouseholdMembers,
            MaxItems = household.PlanType.MaxItems,
            Members = memberDetails
        };
    }

    public async Task<HouseholdStatsDto> GetHouseholdStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var stats = await _householdRepository.GetHouseholdStatsAsync(cancellationToken);

        return new HouseholdStatsDto
        {
            TotalHouseholds = stats.TotalHouseholds,
            ActiveHouseholds = stats.ActiveHouseholds,
            FreeHouseholds = stats.FreeHouseholds,
            PremiumHouseholds = stats.PremiumHouseholds,
            TotalMembers = stats.TotalMembers,
            TotalItems = stats.TotalItems,
            TotalTasks = stats.TotalTasks
        };
    }

    public async Task<SystemHouseholdDto> CreateHouseholdAsync(
        CreateHouseholdDto createDto,
        CancellationToken cancellationToken = default)
    {
        // Verify admin user exists
        var adminUser = await _userProfileRepository.GetByIdAsync(createDto.AdminUserId, cancellationToken);
        if (adminUser == null)
        {
            throw new InvalidOperationException($"User with ID {createDto.AdminUserId} not found");
        }

        // Create household
        var household = new HouseholdEntity
        {
            Name = createDto.Name,
            Address = createDto.Address,
            PlanTypeId = createDto.PlanTypeId,
            SubscriptionStatus = "free",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        household = await _householdRepository.AddAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Add admin member
        var adminMember = new HouseholdMemberEntity
        {
            HouseholdId = household.Id,
            UserId = createDto.AdminUserId,
            Role = DatabaseConstants.HouseholdRoles.Admin,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _householdMemberRepository.AddAsync(adminMember, cancellationToken);
        await _householdMemberRepository.SaveChangesAsync(cancellationToken);

        // Reload household with details
        var createdHousehold = await _householdRepository.GetHouseholdWithMembersAsync(household.Id, cancellationToken);
        return MapToSystemHouseholdDto(createdHousehold!);
    }

    public async Task<SystemHouseholdDto> UpdateHouseholdAsync(
        Guid householdId,
        UpdateHouseholdDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var household = await _householdRepository.GetByIdAsync(householdId, cancellationToken);
        if (household == null)
        {
            throw new InvalidOperationException($"Household with ID {householdId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
        {
            household.Name = updateDto.Name;
        }

        if (updateDto.Address != null)
        {
            household.Address = updateDto.Address;
        }

        if (updateDto.PlanTypeId.HasValue)
        {
            household.PlanTypeId = updateDto.PlanTypeId.Value;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.SubscriptionStatus))
        {
            household.SubscriptionStatus = updateDto.SubscriptionStatus;
        }

        household.UpdatedAt = DateTimeOffset.UtcNow;

        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Reload with details
        var updatedHousehold = await _householdRepository.GetHouseholdWithMembersAsync(householdId, cancellationToken);
        return MapToSystemHouseholdDto(updatedHousehold!);
    }

    public async Task<bool> DeleteHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var household = await _householdRepository.GetByIdAsync(householdId, cancellationToken);
        if (household == null)
        {
            return false;
        }

        // Soft delete
        household.DeletedAt = DateTimeOffset.UtcNow;
        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<SystemHouseholdDto> AssignAdminAsync(
        AssignAdminDto assignDto,
        CancellationToken cancellationToken = default)
    {
        var household = await _householdRepository.GetByIdAsync(assignDto.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new InvalidOperationException($"Household with ID {assignDto.HouseholdId} not found");
        }

        var user = await _userProfileRepository.GetByIdAsync(assignDto.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {assignDto.UserId} not found");
        }

        // Check if user is already a member
        var existingMember = await _householdMemberRepository.GetFirstAsync(
            hm => hm.HouseholdId == assignDto.HouseholdId &&
                  hm.UserId == assignDto.UserId &&
                  hm.DeletedAt == null,
            cancellationToken);

        if (existingMember != null)
        {
            // Update role to admin
            existingMember.Role = DatabaseConstants.HouseholdRoles.Admin;
            existingMember.UpdatedAt = DateTimeOffset.UtcNow;
            await _householdMemberRepository.UpdateAsync(existingMember, cancellationToken);
        }
        else
        {
            // Add as new admin member
            var newMember = new HouseholdMemberEntity
            {
                HouseholdId = assignDto.HouseholdId,
                UserId = assignDto.UserId,
                Role = DatabaseConstants.HouseholdRoles.Admin,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _householdMemberRepository.AddAsync(newMember, cancellationToken);
        }

        await _householdMemberRepository.SaveChangesAsync(cancellationToken);

        // Reload household with details
        var updatedHousehold = await _householdRepository.GetHouseholdWithMembersAsync(assignDto.HouseholdId, cancellationToken);
        return MapToSystemHouseholdDto(updatedHousehold!);
    }

    private static SystemHouseholdDto MapToSystemHouseholdDto(HouseholdEntity household)
    {
        return new SystemHouseholdDto
        {
            Id = household.Id,
            Name = household.Name,
            Address = household.Address,
            PlanTypeId = household.PlanTypeId,
            PlanName = household.PlanType.Name,
            SubscriptionStatus = household.SubscriptionStatus,
            SubscriptionStartDate = household.SubscriptionStartDate,
            SubscriptionEndDate = household.SubscriptionEndDate,
            MemberCount = household.HouseholdMembers.Count(hm => hm.DeletedAt == null),
            ItemCount = household.Items.Count(i => i.DeletedAt == null),
            TaskCount = household.Tasks.Count(t => t.DeletedAt == null),
            CreatedAt = household.CreatedAt,
            UpdatedAt = household.UpdatedAt,
            IsDeleted = household.DeletedAt.HasValue
        };
    }
}
