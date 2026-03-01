public class CreatePaymentRequest
{
    public  string UserId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
}