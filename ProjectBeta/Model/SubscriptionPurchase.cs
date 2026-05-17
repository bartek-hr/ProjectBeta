namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class SubscriptionPurchase
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "validation.subscription_purchase.subscription_id.required")]
    public int SubscriptionId { get; set; }

    public Subscription? Subscription { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "validation.subscription_purchase.user_id.required")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Range(0.01, 999.99, ErrorMessage = "validation.subscription_purchase.amount.range")]
    public decimal Amount { get; set; }

    public decimal DiscountPercentage { get; set; }

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public string PaymentStatus { get; set; } = "Pending";

    public bool IsSuccessful { get; set; }

    public void MarkAsPaid()
    {
        PaymentStatus = "Paid";
        IsSuccessful = true;
    }

    public void MarkAsFailed()
    {
        PaymentStatus = "Failed";
        IsSuccessful = false;
    }
}
