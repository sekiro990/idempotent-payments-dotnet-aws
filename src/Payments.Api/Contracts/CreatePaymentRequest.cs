public class CreatePaymentRequest
{
    public  string userId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
}