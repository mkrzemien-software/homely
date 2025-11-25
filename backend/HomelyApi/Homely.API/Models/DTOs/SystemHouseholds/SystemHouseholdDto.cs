using System.ComponentModel.DataAnnotations;

namespace Homely.API.Models.DTOs.SystemHouseholds;

/// <summary>
/// System-level household information DTO
/// </summary>
public class SystemHouseholdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int PlanTypeId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateOnly? SubscriptionStartDate { get; set; }
    public DateOnly? SubscriptionEndDate { get; set; }
    public int MemberCount { get; set; }
    public int ItemCount { get; set; }
    public int TaskCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Detailed household information with members and stats
/// </summary>
public class SystemHouseholdDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int PlanTypeId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateOnly? SubscriptionStartDate { get; set; }
    public DateOnly? SubscriptionEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Statistics
    public int MemberCount { get; set; }
    public int ItemCount { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTasksCount { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }

    // Plan limits
    public int? MaxMembers { get; set; }
    public int? MaxTasks { get; set; }

    // Members list
    public List<HouseholdMemberSummaryDto> Members { get; set; } = new();
}

/// <summary>
/// Household member summary
/// </summary>
public class HouseholdMemberSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }
}

/// <summary>
/// Paginated households response
/// </summary>
public class PaginatedHouseholdsDto
{
    public List<SystemHouseholdDto> Households { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Household search filters
/// </summary>
public class HouseholdSearchFiltersDto
{
    public string? SearchTerm { get; set; }
    public int? PlanTypeId { get; set; }
    public string? SubscriptionStatus { get; set; }
    public bool? HasActiveMembers { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Create household request
/// </summary>
public class CreateHouseholdDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    public int PlanTypeId { get; set; } = 1; // Default to free plan

    /// <summary>
    /// Optional admin user ID. If not provided, admin can be assigned later using the assign-admin endpoint.
    /// </summary>
    public Guid? AdminUserId { get; set; }
}

/// <summary>
/// Update household request
/// </summary>
public class UpdateHouseholdDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public int? PlanTypeId { get; set; }

    public string? SubscriptionStatus { get; set; }
}

/// <summary>
/// Assign admin request
/// </summary>
public class AssignAdminDto
{
    [Required]
    public Guid HouseholdId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

/// <summary>
/// Household statistics
/// </summary>
public class HouseholdStatsDto
{
    public int TotalHouseholds { get; set; }
    public int ActiveHouseholds { get; set; }
    public int FreeHouseholds { get; set; }
    public int PremiumHouseholds { get; set; }
    public int TotalMembers { get; set; }
    public int TotalItems { get; set; }
    public int TotalTasks { get; set; }
}
