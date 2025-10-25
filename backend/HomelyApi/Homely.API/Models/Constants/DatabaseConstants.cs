namespace Homely.API.Models.Constants;

public static class DatabaseConstants
{
    public static class SubscriptionStatuses
    {
        public const string Free = "free";
        public const string Active = "active";
        public const string Cancelled = "cancelled";
        public const string Expired = "expired";

        public static readonly string[] AllStatuses = { Free, Active, Cancelled, Expired };
    }

    public static class HouseholdRoles
    {
        public const string Admin = "admin";
        public const string Member = "member";
        public const string Dashboard = "dashboard";

        public static readonly string[] AllRoles = { Admin, Member, Dashboard };
    }

    public static class TaskStatuses
    {
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string Postponed = "postponed";
        public const string Cancelled = "cancelled";

        public static readonly string[] AllStatuses = { Pending, Completed, Postponed, Cancelled };
    }

    public static class PriorityLevels
    {
        public const string Low = "low";
        public const string Medium = "medium";
        public const string High = "high";

        public static readonly string[] AllPriorities = { Low, Medium, High };
    }

    public static class UrgencyStatuses
    {
        public const string Overdue = "overdue";
        public const string Today = "today";
        public const string ThisWeek = "this_week";
        public const string ThisMonth = "this_month";
        public const string Upcoming = "upcoming";

        public static readonly string[] AllUrgencyStatuses = { Overdue, Today, ThisWeek, ThisMonth, Upcoming };
    }

    public static class UsageTypes
    {
        public const string Items = "items";
        public const string HouseholdMembers = "household_members";
        public const string StorageMb = "storage_mb";

        public static readonly string[] AllUsageTypes = { Items, HouseholdMembers, StorageMb };
    }
}
