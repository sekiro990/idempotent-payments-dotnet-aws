using Microsoft.AspNetCore.Mvc;
using Payments.Application.Payments.CreatePayment;
using Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly CreatePaymentHandler _createPaymentHandler;
    private readonly PaymentsDbContext _db;
    

    public PaymentsController(CreatePaymentHandler createPaymentHandler, PaymentsDbContext db)
    {
        _createPaymentHandler = createPaymentHandler;
        _db = db;
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
       
    {
        

        if(request == null)
        {
            return BadRequest(new{error = "Request body is required"});
        }
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new{error = "Idempotency-Key header is required"});
        }
        if(string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new{error = "userId is required"});
        }
        idempotencyKey = idempotencyKey.Trim();
        
        var command = new CreatePaymentCommand
        {
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim(),
            IdempotencyKey = idempotencyKey
        };

        var result = await _createPaymentHandler.Handle(command, ct);

        if(result.IsReplay)
        {
            return Ok(new{isReplay = true, payment = result.Payment});
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Payment.Id }, new{isReplay = false, payment = result.Payment});
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p=> p.Id == id,ct);
        
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }
}
