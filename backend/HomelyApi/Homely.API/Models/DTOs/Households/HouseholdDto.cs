namespace Homely.API.Models.DTOs.Households
{
    /// <summary>
    /// DTO for household information
    /// </summary>
    public class HouseholdDto
    {
        /// <summary>
        /// Household ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Household name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Household address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Plan type ID
        /// </summary>
        public int PlanTypeId { get; set; }

        /// <summary>
        /// Plan type name (e.g., "Free", "Premium")
        /// </summary>
        public string PlanTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Subscription status (free, active, cancelled, expired)
        /// </summary>
        public string SubscriptionStatus { get; set; } = "free";

        /// <summary>
        /// Subscription start date
        /// </summary>
        public DateOnly? SubscriptionStartDate { get; set; }

        /// <summary>
        /// Subscription end date
        /// </summary>
        public DateOnly? SubscriptionEndDate { get; set; }

        /// <summary>
        /// Number of active members in the household
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Date when household was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
