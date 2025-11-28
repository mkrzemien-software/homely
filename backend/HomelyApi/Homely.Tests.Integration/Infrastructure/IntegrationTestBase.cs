using Homely.API.Data;
using Microsoft.EntityFrameworkCore;
using Respawn;

namespace Homely.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests with database cleanup between tests
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = null!;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Initialize Respawner for database cleanup
    /// </summary>
    public async Task InitializeAsync()
    {
        using var dbContext = Factory.GetDbContext();
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            // Don't reset seed data (plan_types, category_types, categories)
            TablesToIgnore = ["plan_types", "category_types", "categories", "__EFMigrationsHistory"]
        });
    }

    /// <summary>
    /// Reset database after each test
    /// </summary>
    public async Task DisposeAsync()
    {
        using var dbContext = Factory.GetDbContext();
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Helper to get fresh DbContext for test assertions
    /// </summary>
    protected HomelyDbContext GetDbContext()
    {
        return Factory.GetDbContext();
    }
}
