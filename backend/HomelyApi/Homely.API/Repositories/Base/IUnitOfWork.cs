using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Base;

public interface IUnitOfWork : IDisposable
{
    IUserProfileRepository UserProfiles { get; }
    IPlanTypeRepository PlanTypes { get; }
    IHouseholdRepository Households { get; }
    IHouseholdMemberRepository HouseholdMembers { get; }
    ICategoryTypeRepository CategoryTypes { get; }
    ICategoryRepository Categories { get; }
    ITaskRepository Tasks { get; }
    IEventRepository Events { get; }
    IEventHistoryRepository EventsHistory { get; }
    IPlanUsageRepository PlanUsages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation within a transaction using the configured execution strategy.
    /// This method is compatible with retry strategies like NpgsqlRetryingExecutionStrategy.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
