using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Repositories.Interfaces;
using Homely.API.Repositories.Implementations;
using Homely.API.Repositories.Base;
using Homely.API.Services;

namespace Homely.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HomelyDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                npgsqlOptions.MigrationsAssembly("Homely.API");
            });
            
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging(true);
                options.EnableDetailedErrors(true);
            }
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IPlanTypeRepository, PlanTypeRepository>();
        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IHouseholdMemberRepository, HouseholdMemberRepository>();
        services.AddScoped<ICategoryTypeRepository, CategoryTypeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
        services.AddScoped<IPlanUsageRepository, PlanUsageRepository>();

        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<ISystemUsersService, SystemUsersService>();
        services.AddScoped<ISystemHouseholdsService, SystemHouseholdsService>();
        services.AddScoped<ICategoryTypeService, CategoryTypeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITaskService, TaskService>();
        // services.AddScoped<IHouseholdService, HouseholdService>();
        // services.AddScoped<IItemService, ItemService>();

        return services;
    }

    public static IServiceCollection AddHomelyServices(this IServiceCollection services, string connectionString)
    {
        services
            .AddDatabase(connectionString)
            .AddRepositories()
            .AddBusinessServices();

        return services;
    }
}
