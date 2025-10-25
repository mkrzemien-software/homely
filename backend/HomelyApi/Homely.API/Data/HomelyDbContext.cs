using Microsoft.EntityFrameworkCore;
using Homely.API.Entities;
using Homely.API.Models.ViewModels;

namespace Homely.API.Data;

public class HomelyDbContext : DbContext
{
    public HomelyDbContext(DbContextOptions<HomelyDbContext> options) : base(options)
    {
    }

    public DbSet<PlanTypeEntity> PlanTypes { get; set; }
    public DbSet<HouseholdEntity> Households { get; set; }
    public DbSet<HouseholdMemberEntity> HouseholdMembers { get; set; }
    public DbSet<CategoryTypeEntity> CategoryTypes { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ItemEntity> Items { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TaskHistoryEntity> TasksHistory { get; set; }
    public DbSet<PlanUsageEntity> PlanUsages { get; set; }

    public DbSet<DashboardUpcomingTaskViewModel> DashboardUpcomingTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


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

        // Items
        modelBuilder.Entity<ItemEntity>()
            .HasIndex(i => i.HouseholdId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_items_household");

        modelBuilder.Entity<ItemEntity>()
            .HasIndex(i => i.CategoryId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_items_category");

        modelBuilder.Entity<ItemEntity>()
            .HasIndex(i => i.CreatedBy)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_items_created_by");

        modelBuilder.Entity<ItemEntity>()
            .HasIndex(i => i.IsActive)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_items_active");

        // Tasks
        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.DueDate)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_due_date");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => new { t.HouseholdId, t.Status })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_household_status");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.AssignedTo)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_assigned_to");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => new { t.Status, t.DueDate })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_status_due");

        modelBuilder.Entity<TaskEntity>()
            .HasIndex(t => t.ItemId)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("idx_tasks_item");

        // Task History
        modelBuilder.Entity<TaskHistoryEntity>()
            .HasIndex(th => th.HouseholdId)
            .HasDatabaseName("idx_tasks_history_household");

        modelBuilder.Entity<TaskHistoryEntity>()
            .HasIndex(th => th.CompletionDate)
            .HasDatabaseName("idx_tasks_history_completion_date");

        modelBuilder.Entity<TaskHistoryEntity>()
            .HasIndex(th => th.ItemId)
            .HasDatabaseName("idx_tasks_history_item");

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

        // Category -> CategoryType
        modelBuilder.Entity<CategoryEntity>()
            .HasOne(c => c.CategoryType)
            .WithMany(ct => ct.Categories)
            .HasForeignKey(c => c.CategoryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Item -> Household
        modelBuilder.Entity<ItemEntity>()
            .HasOne(i => i.Household)
            .WithMany(h => h.Items)
            .HasForeignKey(i => i.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item -> Category
        modelBuilder.Entity<ItemEntity>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Task -> Item
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Item)
            .WithMany(i => i.Tasks)
            .HasForeignKey(t => t.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Household
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Household)
            .WithMany(h => h.Tasks)
            .HasForeignKey(t => t.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskHistory -> Task
        modelBuilder.Entity<TaskHistoryEntity>()
            .HasOne(th => th.Task)
            .WithMany(t => t.TasksHistory)
            .HasForeignKey(th => th.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskHistory -> Item
        modelBuilder.Entity<TaskHistoryEntity>()
            .HasOne(th => th.Item)
            .WithMany(i => i.TasksHistory)
            .HasForeignKey(th => th.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskHistory -> Household
        modelBuilder.Entity<TaskHistoryEntity>()
            .HasOne(th => th.Household)
            .WithMany(h => h.TasksHistory)
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

        // Item priority
        modelBuilder.Entity<ItemEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Item_Priority", 
                "priority IN ('low', 'medium', 'high')"));

        // Task status
        modelBuilder.Entity<TaskEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Task_Status", 
                "status IN ('pending', 'completed', 'postponed', 'cancelled')"));

        // Task priority
        modelBuilder.Entity<TaskEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Task_Priority", 
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
                MaxItems = 5,
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
                MaxItems = 100,
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
                MaxItems = null,
                PriceMonthly = 19.99m,
                PriceYearly = 199.99m,
                Features = "[\"podstawowe_zadania\", \"widok_kalendarza\", \"powiadomienia_email\", \"dokumenty\", \"analityka\", \"historia\", \"wsparcie_priorytetowe\", \"nieograniczeni_czlonkowie\", \"nieograniczone_przedmioty\"]",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        modelBuilder.Entity<CategoryTypeEntity>().HasData(
            new CategoryTypeEntity { Id = 1, Name = "Przeglądy techniczne", Description = "Przeglądy techniczne pojazdów i urządzeń", SortOrder = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryTypeEntity { Id = 2, Name = "Wywóz śmieci", Description = "Harmonogram wywozu śmieci", SortOrder = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryTypeEntity { Id = 3, Name = "Wizyty medyczne", Description = "Wizyty zdrowotne członków gospodarstwa", SortOrder = 3, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );

        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = 1, CategoryTypeId = 1, Name = "Przegląd samochodu", Description = "Obowiązkowy przegląd techniczny pojazdu", SortOrder = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 2, CategoryTypeId = 1, Name = "Przegląd kotła", Description = "Przegląd kotła grzewczego", SortOrder = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 3, CategoryTypeId = 2, Name = "Śmieci zmieszane", Description = "Wywóz śmieci zmieszanych", SortOrder = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 4, CategoryTypeId = 2, Name = "Odpady segregowane", Description = "Wywóz odpadów segregowanych", SortOrder = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 5, CategoryTypeId = 3, Name = "Lekarz rodzinny", Description = "Wizyty u lekarza pierwszego kontaktu", SortOrder = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 6, CategoryTypeId = 3, Name = "Dentysta", Description = "Wizyty dentystyczne", SortOrder = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new CategoryEntity { Id = 7, CategoryTypeId = 3, Name = "Badania kontrolne", Description = "Regularne badania profilaktyczne", SortOrder = 3, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
    }
}
