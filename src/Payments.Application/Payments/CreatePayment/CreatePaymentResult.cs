using Payments.Domain.Entities;
namespace Payments.Application.Payments.CreatePayment;

public class CreatePaymentResult
{
    public bool IsReplay { get; set; }
    
    public required Payment Payment { get; set; }
}