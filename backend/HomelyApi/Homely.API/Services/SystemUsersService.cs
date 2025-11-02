using Homely.API.Entities;
using Homely.API.Models.DTOs.SystemUsers;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for system-level user management
/// </summary>
public class SystemUsersService : ISystemUsersService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IHouseholdMemberRepository _householdMemberRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly ISupabaseAuthService _supabaseAuthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemUsersService> _logger;

    public SystemUsersService(
        IUserProfileRepository userProfileRepository,
        IHouseholdMemberRepository householdMemberRepository,
        IHouseholdRepository householdRepository,
        ISupabaseAuthService supabaseAuthService,
        IUnitOfWork unitOfWork,
        ILogger<SystemUsersService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _householdMemberRepository = householdMemberRepository;
        _householdRepository = householdRepository;
        _supabaseAuthService = supabaseAuthService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Search users with filtering and pagination
    /// </summary>
    public async Task<PaginatedUsersDto> SearchUsersAsync(UserSearchFiltersDto filters, CancellationToken cancellationToken = default)
    {
        try
        {
            var pagedResult = await _userProfileRepository.SearchUsersAsync(
                filters.SearchTerm,
                filters.Role,
                filters.Status,
                filters.HouseholdId,
                filters.Page,
                filters.PageSize,
                cancellationToken);

            var users = pagedResult.Items.Select(MapToSystemUserDto).ToList();

            return new PaginatedUsersDto
            {
                Users = users,
                Total = pagedResult.TotalCount,
                Page = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with filters: {Filters}", filters);
            throw;
        }
    }

    /// <summary>
    /// Get detailed user information
    /// </summary>
    public async Task<SystemUserDto?> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            return MapToSystemUserDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for userId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get user activity history
    /// </summary>
    public async Task<List<UserActivityDto>> GetUserActivityAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement activity logging table and repository
            // For now, return empty list
            _logger.LogWarning("User activity tracking not yet implemented");
            return new List<UserActivityDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for userId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Create new user with Supabase auth and profile
    /// </summary>
    public async Task<SystemUserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Create user in Supabase auth.users (via Supabase Admin API)
            _logger.LogInformation("Creating user in Supabase Auth for email: {Email}", createUserDto.Email);
            var userId = await _supabaseAuthService.CreateUserAsync(
                createUserDto.Email,
                createUserDto.Password,
                emailConfirm: true,
                cancellationToken);

            // Step 2: Create user profile
            var userProfile = new UserProfileEntity
            {
                UserId = userId,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PhoneNumber = createUserDto.PhoneNumber,
                PreferredLanguage = "pl",
                Timezone = "Europe/Warsaw",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userProfileRepository.AddAsync(userProfile, cancellationToken);

            // Step 3: Add user to household
            var householdMember = new HouseholdMemberEntity
            {
                HouseholdId = createUserDto.HouseholdId,
                UserId = userId,
                Role = createUserDto.Role,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _householdMemberRepository.AddAsync(householdMember, cancellationToken);

            // Step 4: Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Step 5: Send welcome email if requested
            if (createUserDto.SendWelcomeEmail)
            {
                // TODO: Implement email service
                _logger.LogInformation("Welcome email should be sent to {Email}", createUserDto.Email);
            }

            // Return created user
            var createdUser = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            return MapToSystemUserDto(createdUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", createUserDto.Email);
            throw;
        }
    }

    /// <summary>
    /// Update user role in household
    /// </summary>
    public async Task<SystemUserDto> UpdateUserRoleAsync(Guid userId, Guid householdId, string newRole, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _householdMemberRepository.GetFirstAsync(
                hm => hm.UserId == userId && hm.HouseholdId == householdId && hm.DeletedAt == null,
                cancellationToken);

            if (member == null)
            {
                throw new InvalidOperationException($"User {userId} is not a member of household {householdId}");
            }

            member.Role = newRole;
            member.UpdatedAt = DateTimeOffset.UtcNow;

            await _householdMemberRepository.UpdateAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedUser = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            return MapToSystemUserDto(updatedUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for userId: {UserId}, householdId: {HouseholdId}, newRole: {NewRole}",
                userId, householdId, newRole);
            throw;
        }
    }

    /// <summary>
    /// Reset user password (sends reset email)
    /// </summary>
    public async Task<bool> ResetUserPasswordAsync(Guid userId, bool sendEmail = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Integrate with Supabase Auth API to reset password
            _logger.LogInformation("Password reset requested for userId: {UserId}, sendEmail: {SendEmail}", userId, sendEmail);

            // Would call Supabase Admin API here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for userId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    public async Task<bool> UnlockUserAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Integrate with Supabase Auth API to unlock account
            _logger.LogInformation("Account unlock requested for userId: {UserId}", userId);

            // Would call Supabase Admin API here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for userId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Move user to different household
    /// </summary>
    public async Task<SystemUserDto> MoveUserToHouseholdAsync(Guid userId, Guid newHouseholdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userProfile = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            if (userProfile == null)
            {
                throw new InvalidOperationException($"User {userId} not found");
            }

            // Get current primary membership (first active one)
            var currentMembership = userProfile.HouseholdMemberships
                .FirstOrDefault(hm => hm.DeletedAt == null);

            if (currentMembership == null)
            {
                throw new InvalidOperationException($"User {userId} has no active household membership");
            }

            // Soft delete current membership
            currentMembership.DeletedAt = DateTimeOffset.UtcNow;
            await _householdMemberRepository.UpdateAsync(currentMembership, cancellationToken);

            // Create new membership
            var newMembership = new HouseholdMemberEntity
            {
                HouseholdId = newHouseholdId,
                UserId = userId,
                Role = currentMembership.Role, // Keep same role
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _householdMemberRepository.AddAsync(newMembership, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedUser = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            return MapToSystemUserDto(updatedUser!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving user {UserId} to household {NewHouseholdId}", userId, newHouseholdId);
            throw;
        }
    }

    /// <summary>
    /// Delete user from system (soft delete profile, hard delete from Supabase Auth)
    /// </summary>
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user {UserId} from system", userId);

            // Step 1: Get user profile
            var userProfile = await _userProfileRepository.GetUserWithHouseholdsAsync(userId, cancellationToken);
            if (userProfile == null)
            {
                _logger.LogWarning("User {UserId} not found in database", userId);
                return false;
            }

            // Step 2: Soft delete all household memberships
            foreach (var membership in userProfile.HouseholdMemberships.Where(hm => hm.DeletedAt == null))
            {
                membership.DeletedAt = DateTimeOffset.UtcNow;
                await _householdMemberRepository.UpdateAsync(membership, cancellationToken);
            }

            // Step 3: Soft delete user profile
            userProfile.DeletedAt = DateTimeOffset.UtcNow;
            userProfile.UpdatedAt = DateTimeOffset.UtcNow;
            await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);

            // Step 4: Save changes to database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Step 5: Delete user from Supabase Auth (hard delete)
            var authDeleted = await _supabaseAuthService.DeleteUserAsync(userId, cancellationToken);
            if (!authDeleted)
            {
                _logger.LogWarning("Failed to delete user {UserId} from Supabase Auth, but profile was soft deleted", userId);
            }

            _logger.LogInformation("User {UserId} deleted successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Map UserProfileEntity to SystemUserDto
    /// </summary>
    private SystemUserDto MapToSystemUserDto(UserProfileEntity user)
    {
        var primaryMembership = user.HouseholdMemberships
            .FirstOrDefault(hm => hm.DeletedAt == null);

        return new SystemUserDto
        {
            Id = user.UserId,
            Email = $"user_{user.UserId}@homely.com", // TODO: Get from Supabase auth.users
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = primaryMembership?.Role ?? "member",
            Status = "active", // TODO: Get from Supabase auth.users
            HouseholdId = primaryMembership?.HouseholdId ?? Guid.Empty,
            HouseholdName = primaryMembership?.Household?.Name ?? "No Household",
            LastLogin = user.LastActiveAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber
        };
    }
}
