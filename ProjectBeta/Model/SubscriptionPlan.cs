namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class SubscriptionPlan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "validation.subscription_plan.name.required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "validation.subscription_plan.name.length")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.subscription_plan.description.required")]
    [StringLength(250, ErrorMessage = "validation.subscription_plan.description.length")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999.99, ErrorMessage = "validation.subscription_plan.monthly_price.range")]
    public decimal MonthlyPrice { get; set; }

    public bool IncludesFreeMondayCinema { get; set; } = true;

    public bool IncludesDiscounts { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Subscription> Subscriptions { get; set; } = new();
}
