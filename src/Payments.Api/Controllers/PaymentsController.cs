using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        //Temporary test return
        return Ok(new
        {
            request.userId,
            request.Amount,
            request.Currency,
            idempotencyKey
        }
        );
    }
}