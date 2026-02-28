namespace Payments.Domain.Entities;

public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }
public class Payment
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;

    public string IdempotencyKey { get; set; } = default!;
    
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "AUD";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


}