using Microsoft.AspNetCore.Mvc;
using Payments.Infrastructure.Persistence;
using Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public PaymentsController(PaymentsDbContext db)
    {
        _db = db;
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new{error = "Idempotency-Key header is required"});
        }
        if(string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new{error = "userId is required"});
        }
         // If already created with same (userId, idempotencyKey), return it

         var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.IdempotencyKey == idempotencyKey);
        
        if (existingPayment != null)        {
            return Ok(existingPayment);
        }
        
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            IdempotencyKey = idempotencyKey,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Payments.Add(payment);
      await _db.SaveChangesAsync();
      
        
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }
}