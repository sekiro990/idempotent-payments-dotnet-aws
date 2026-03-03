namespace Payments.Application.Payments.CreatePayment;

public class CreatePaymentCommand
{
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string IdempotencyKey { get; set; } = default!;
}