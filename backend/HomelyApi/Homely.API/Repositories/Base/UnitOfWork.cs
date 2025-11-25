using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Repositories.Interfaces;
using Homely.API.Repositories.Implementations;

namespace Homely.API.Repositories.Base;

public class UnitOfWork : IUnitOfWork
{
    private readonly HomelyDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserProfileRepository? _userProfiles;
    private IPlanTypeRepository? _planTypes;
    private IHouseholdRepository? _households;
    private IHouseholdMemberRepository? _householdMembers;
    private ICategoryTypeRepository? _categoryTypes;
    private ICategoryRepository? _categories;
    private ITaskRepository? _tasks;
    private IEventRepository? _events;
    private IEventHistoryRepository? _eventsHistory;
    private IPlanUsageRepository? _planUsages;

    public UnitOfWork(HomelyDbContext context)
    {
        _context = context;
    }

    public IUserProfileRepository UserProfiles
    {
        get
        {
            _userProfiles ??= new UserProfileRepository(_context);
            return _userProfiles;
        }
    }

    public IPlanTypeRepository PlanTypes
    {
        get
        {
            _planTypes ??= new PlanTypeRepository(_context);
            return _planTypes;
        }
    }

    public IHouseholdRepository Households
    {
        get
        {
            _households ??= new HouseholdRepository(_context);
            return _households;
        }
    }

    public IHouseholdMemberRepository HouseholdMembers
    {
        get
        {
            _householdMembers ??= new HouseholdMemberRepository(_context);
            return _householdMembers;
        }
    }

    public ICategoryTypeRepository CategoryTypes
    {
        get
        {
            _categoryTypes ??= new CategoryTypeRepository(_context);
            return _categoryTypes;
        }
    }

    public ICategoryRepository Categories
    {
        get
        {
            _categories ??= new CategoryRepository(_context);
            return _categories;
        }
    }

    public ITaskRepository Tasks
    {
        get
        {
            _tasks ??= new TaskRepository(_context);
            return _tasks;
        }
    }

    public IEventRepository Events
    {
        get
        {
            _events ??= new EventRepository(_context);
            return _events;
        }
    }

    public IEventHistoryRepository EventsHistory
    {
        get
        {
            _eventsHistory ??= new EventHistoryRepository(_context);
            return _eventsHistory;
        }
    }

    public IPlanUsageRepository PlanUsages
    {
        get
        {
            _planUsages ??= new PlanUsageRepository(_context);
            return _planUsages;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transakcja już istnieje. Najpierw zatwierdź lub wycofaj bieżącą transakcję.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Brak aktywnej transakcji do zatwierdzenia.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Brak aktywnej transakcji do wycofania.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Executes an operation within a transaction using the configured execution strategy.
    /// This method is compatible with retry strategies like NpgsqlRetryingExecutionStrategy.
    /// </summary>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        // Get the execution strategy configured for the context
        var strategy = _context.Database.CreateExecutionStrategy();

        // Execute the operation within the strategy's transaction handling
        return await strategy.ExecuteAsync(async (ct) =>
        {
            // Begin transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // Execute the operation
                var result = await operation(ct);

                // Save changes and commit
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return result;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync(ct);
                throw;
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
