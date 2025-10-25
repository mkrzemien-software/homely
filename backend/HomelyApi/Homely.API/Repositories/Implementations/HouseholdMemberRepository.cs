using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

public class HouseholdMemberRepository : BaseRepository<HouseholdMemberEntity, Guid>, IHouseholdMemberRepository
{
    public HouseholdMemberRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<HouseholdMemberEntity, bool>> GetIdPredicate(Guid id)
    {
        return x => x.Id == id;
    }

    public async Task<IEnumerable<HouseholdMemberEntity>> GetHouseholdMembersAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(hm => hm.HouseholdId == householdId, hm => hm.Household);
    }

    public async Task<IEnumerable<HouseholdMemberEntity>> GetUserMembershipsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(hm => hm.UserId == userId, hm => hm.Household, hm => hm.Household.PlanType);
    }

    public async Task<HouseholdMemberEntity?> GetMembershipAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetFirstAsync(hm => hm.HouseholdId == householdId && hm.UserId == userId, cancellationToken);
    }

    public async Task<HouseholdMemberEntity?> GetByInvitationTokenAsync(string invitationToken, CancellationToken cancellationToken = default)
    {
        return await GetFirstAsync(hm => hm.InvitationToken == invitationToken && 
                                        hm.InvitationExpiresAt > DateTimeOffset.UtcNow,
            hm => hm.Household);
    }

    public async Task<bool> HasRoleAsync(Guid householdId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(hm => hm.HouseholdId == householdId && 
                                      hm.UserId == userId && 
                                      hm.Role == role, cancellationToken);
    }

    public async Task<IEnumerable<HouseholdMemberEntity>> GetAdminsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(hm => hm.HouseholdId == householdId && 
                                        hm.Role == DatabaseConstants.HouseholdRoles.Admin);
    }
}
