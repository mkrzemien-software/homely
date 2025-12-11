using Microsoft.EntityFrameworkCore;
using Homely.API.Entities;
using Homely.API.Models.ViewModels;

namespace Homely.API.Data;

public class HomelyDbContext : DbContext
{
    public HomelyDbContext(DbContextOptions<HomelyDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<PlanTypeEntity> PlanTypes { get; set; }
    public DbSet<HouseholdEntity> Households { get; set; }
    public DbSet<HouseholdMemberEntity> HouseholdMembers { get; set; }
    public DbSet<CategoryTypeEntity> CategoryTypes { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    /// <summary>
    /// Tasks DbSet - task templates that define "what" and "how often"
    /// Maps to the 'tasks' table in the database
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Events DbSet - concrete scheduled occurrences created from task templates
    /// Maps to the 'events' table in the database
    /// </summary>
    public DbSet<EventEntity> Events { get; set; }

    /// <summary>
    /// Events History DbSet - archival record of completed events (premium feature)
    /// Maps to the 'events_history' table in the database
    /// </summary>
    public DbSet<EventHistoryEntity> EventsHistory { get; set; }
    public DbSet<PlanUsageEntity> PlanUsages { get; set; }

    public DbSet<DashboardUpcomingTaskViewModel> DashboardUpcomingTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Profiles
        modelBuilder.Entity<UserProfileEntity>()
            .HasIndex(up => up.DeletedAt)
            .HasDatabaseName("idx_user_profiles_deleted_at");

        modelBuilder.Entity<UserProfileEntity>()
            .HasIndex(up => up.LastActiveAt)
            .HasDatabaseName("idx_user_profiles_last_active");

        // Plan Types
        modelBuilder.Entity<PlanTypeEntity>()
            .HasIndex(p => p.IsActive)
            .HasDatabaseName("idx_plan_types_active");

        modelBuilder.Entity<PlanTypeEntity>()
            .HasIndex(p => p.DeletedAt)
            .HasDatabaseName("idx_plan_types_deleted_at");

        // Households
        modelBuilder.Entity<HouseholdEntity>()
            .HasIndex(h => h.PlanTypeId)
            .HasDatabaseName("idx_households_plan_type");

        modelBuilder.Entity<HouseholdEntity>()
            .HasIndex(h => h.DeletedAt)
            .HasDatabaseName("idx_households_deleted_at");

        // Household Members
        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasIndex(hm => hm.HouseholdId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_household_members_household");

        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasIndex(hm => hm.UserId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_household_members_user");

        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasIndex(hm => new { hm.HouseholdId, hm.Role })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_household_members_role");

        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasIndex(hm => new { hm.HouseholdId, hm.UserId })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_household_members_unique_active");

        // Category Types
        modelBuilder.Entity<CategoryTypeEntity>()
            .HasIndex(ct => ct.IsActive)
            .HasDatabaseName("idx_category_types_active");

        modelBuilder.Entity<CategoryTypeEntity>()
            .HasIndex(ct => ct.SortOrder)
            .HasDatabaseName("idx_category_types_sort_order");

        // Categories
        modelBuilder.Entity<CategoryEntity>()
            .HasIndex(c => c.CategoryTypeId)
            .HasDatabaseName("idx_categories_type");

        modelBuilder.Entity<CategoryEntity>()
            .HasIndex(c => c.IsActive)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_categories_active");

        modelBuilder.Entity<CategoryEntity>()
            .HasIndex(c => new { c.CategoryTypeId, c.Name })
            .IsUnique()
            .HasDatabaseName("categories_unique_name");

        // Tasks (task templates)
        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.HouseholdId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_household");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.CategoryId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_category");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.CreatedBy)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_created_by");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.IsActive)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_active");

        // Events (concrete occurrences)
        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => e.DueDate)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_events_due_date");

        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => new { e.HouseholdId, e.Status })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_events_household_status");

        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => e.AssignedTo)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_events_assigned_to");

        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => new { e.Status, e.DueDate })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_events_status_due");

        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => e.TaskId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_events_task");

        // Event History
        modelBuilder.Entity<EventHistoryEntity>()
            .HasIndex(th => th.HouseholdId)
            .HasDatabaseName("idx_events_history_household");

        modelBuilder.Entity<EventHistoryEntity>()
            .HasIndex(th => th.CompletionDate)
            .HasDatabaseName("idx_events_history_completion_date");

        modelBuilder.Entity<EventHistoryEntity>()
            .HasIndex(th => th.TaskId)
            .HasDatabaseName("idx_events_history_task");

        modelBuilder.Entity<EventHistoryEntity>()
            .HasIndex(th => th.EventId)
            .HasDatabaseName("idx_events_history_event");

        // Plan Usage
        modelBuilder.Entity<PlanUsageEntity>()
            .HasIndex(pu => new { pu.HouseholdId, pu.UsageType })
            .HasDatabaseName("idx_plan_usage_household_type");

        modelBuilder.Entity<PlanUsageEntity>()
            .HasIndex(pu => pu.UsageDate)
            .HasDatabaseName("idx_plan_usage_date");

        // Household -> PlanType
        modelBuilder.Entity<HouseholdEntity>()
            .HasOne(h => h.PlanType)
            .WithMany(pt => pt.Households)
            .HasForeignKey(h => h.PlanTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // HouseholdMember -> Household
        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasOne(hm => hm.Household)
            .WithMany(h => h.HouseholdMembers)
            .HasForeignKey(hm => hm.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // HouseholdMember -> UserProfile
        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasOne<UserProfileEntity>()
            .WithMany(up => up.HouseholdMemberships)
            .HasForeignKey(hm => hm.UserId)
            .HasPrincipalKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> CategoryType
        modelBuilder.Entity<CategoryEntity>()
            .HasOne(c => c.CategoryType)
            .WithMany(ct => ct.Categories)
            .HasForeignKey(c => c.CategoryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Task -> Household
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Household)
            .WithMany(h => h.Tasks)
            .HasForeignKey(t => t.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Category
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Task -> CreatedBy User
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedBy)
            .HasPrincipalKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> Task (template)
        modelBuilder.Entity<EventEntity>()
            .HasOne(e => e.Task)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        // Event -> Household
        modelBuilder.Entity<EventEntity>()
            .HasOne(e => e.Household)
            .WithMany(h => h.Events)
            .HasForeignKey(e => e.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event -> AssignedTo User
        modelBuilder.Entity<EventEntity>()
            .HasOne(e => e.AssignedToUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedTo)
            .HasPrincipalKey(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Event -> CreatedBy User
        modelBuilder.Entity<EventEntity>()
            .HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .HasPrincipalKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // EventHistory -> Event (completed event)
        modelBuilder.Entity<EventHistoryEntity>()
            .HasOne(th => th.Event)
            .WithMany()
            .HasForeignKey(th => th.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventHistory -> Task (task template)
        modelBuilder.Entity<EventHistoryEntity>()
            .HasOne(th => th.Task)
            .WithMany()
            .HasForeignKey(th => th.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventHistory -> Household
        modelBuilder.Entity<EventHistoryEntity>()
            .HasOne(th => th.Household)
            .WithMany(h => h.EventsHistory)
            .HasForeignKey(th => th.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlanUsage -> Household
        modelBuilder.Entity<PlanUsageEntity>()
            .HasOne(pu => pu.Household)
            .WithMany(h => h.PlanUsages)
            .HasForeignKey(pu => pu.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlanUsage: unique constraint on household_id, usage_type, usage_date
        modelBuilder.Entity<PlanUsageEntity>()
            .HasIndex(pu => new { pu.HouseholdId, pu.UsageType, pu.UsageDate })
            .IsUnique()
            .HasDatabaseName("plan_usage_unique_constraint");

        // HouseholdMember: unique invitation token
        modelBuilder.Entity<HouseholdMemberEntity>()
            .HasIndex(hm => hm.InvitationToken)
            .IsUnique()
            .HasFilter("invitation_token IS NOT NULL")
            .HasDatabaseName("household_members_unique_invitation_token");

        // Household subscription status
        modelBuilder.Entity<HouseholdEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Household_SubscriptionStatus", 
                "subscription_status IN ('free', 'active', 'cancelled', 'expired')"));

        // HouseholdMember role
        modelBuilder.Entity<HouseholdMemberEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_HouseholdMember_Role",
                "role IN ('admin', 'member', 'dashboard')"));

        // Task priority (task templates)
        modelBuilder.Entity<TaskEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Task_Priority",
                "priority IN ('low', 'medium', 'high')"));

        // Event status (concrete occurrences)
        modelBuilder.Entity<EventEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Event_Status",
                "status IN ('pending', 'completed', 'postponed', 'cancelled')"));

        // Event priority
        modelBuilder.Entity<EventEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Event_Priority",
                "priority IN ('low', 'medium', 'high')"));

        // Dashboard view configuration (read-only)
        modelBuilder.Entity<DashboardUpcomingTaskViewModel>()
            .ToView("dashboard_upcoming_tasks")
            .HasKey(d => d.Id);

        modelBuilder.Entity<PlanTypeEntity>().HasData(
            new PlanTypeEntity
            {
                Id = 1,
                Name = "Darmowy",
                Description = "Podstawowe zarządzanie gospodarstwem domowym",
                MaxHouseholdMembers = 3,
                MaxTasks = 5,
                PriceMonthly = 0.00m,
                PriceYearly = 0.00m,
                Features = "[\"podstawowe_zadania\", \"widok_kalendarza\"]",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new PlanTypeEntity
            {
                Id = 2,
                Name = "Premium",
                Description = "Pełne zarządzanie gospodarstwem z analityką",
                MaxHouseholdMembers = 10,
                MaxTasks = 100,
                PriceMonthly = 9.99m,
                PriceYearly = 99.99m,
                Features = "[\"podstawowe_zadania\", \"widok_kalendarza\", \"powiadomienia_email\", \"dokumenty\", \"analityka\", \"historia\", \"wsparcie_priorytetowe\"]",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new PlanTypeEntity
            {
                Id = 3,
                Name = "Rodzinny",
                Description = "Kompletne rozwiązanie dla rodziny",
                MaxHouseholdMembers = null,
                MaxTasks = null,
                PriceMonthly = 19.99m,
                PriceYearly = 199.99m,
                Features = "[\"podstawowe_zadania\", \"widok_kalendarza\", \"powiadomienia_email\", \"dokumenty\", \"analityka\", \"historia\", \"wsparcie_priorytetowe\", \"nieograniczeni_czlonkowie\", \"nieograniczone_przedmioty\"]",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        // Note: CategoryType and Category seed data removed
        // Categories are now multi-tenant (household-specific) and should be created per-household
        // via the API or through household initialization logic, not as global seed data.
    }
}
