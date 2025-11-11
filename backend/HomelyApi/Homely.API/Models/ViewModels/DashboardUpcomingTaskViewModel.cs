namespace Homely.API.Models.ViewModels;

public class DashboardUpcomingTaskViewModel
{
    public Guid Id { get; set; }
    public DateOnly DueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryTypeName { get; set; } = string.Empty;
    public Guid HouseholdId { get; set; }
    public string HouseholdName { get; set; } = string.Empty;
    public string UrgencyStatus { get; set; } = string.Empty;
    public int DaysUntilDue { get; set; }
    public int PriorityScore { get; set; }
}