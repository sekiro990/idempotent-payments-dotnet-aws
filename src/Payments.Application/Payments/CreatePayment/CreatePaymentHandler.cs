using Payments.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Npgsql;
namespace Payments.Application.Payments.CreatePayment;

public class CreatePaymentHandler
{
    private readonly IPaymentsDbContext _db;

    public CreatePaymentHandler(IPaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand command, CancellationToken ct)
    {
        if(command == null) throw new ArgumentNullException(nameof(command));
        if(string.IsNullOrEmpty(command.UserId)) throw new ArgumentException("UserId is required", nameof(command.UserId));
        if(string.IsNullOrEmpty(command.IdempotencyKey)) throw new ArgumentException("IdempotencyKey is required", nameof(command.IdempotencyKey));
        if(command.Amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(command.Amount));
        if(string.IsNullOrWhiteSpace(command.Currency)) throw new ArgumentException("Currency is required", nameof(command.Currency));
        var key = command.IdempotencyKey.Trim();

        

        var existingPayment = await FindByIdempotentAsync(command.UserId, key, ct);
        
        if(existingPayment != null)
        {
            return new CreatePaymentResult
            {
                IsReplay = true,
                Payment = existingPayment
            };
        }
        else
        {
            var payment = BuildPayment(command, key);
            try
            {
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync(ct);
                return new CreatePaymentResult
                {
                    IsReplay = false,
                    Payment = payment
                };
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                var concurrentPayment = await FindByIdempotentAsync(command.UserId, key, ct);
                if (concurrentPayment != null){
                    return new CreatePaymentResult
                    {
                        IsReplay = true,
                        Payment = concurrentPayment
                    };
                }else{
                    throw;
                }
            }
        }
    }

    private async Task<Payment?> FindByIdempotentAsync(string userId, string key, CancellationToken ct)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId && p.IdempotencyKey == key, ct);
        if(payment != null)
        {
            return payment;
        }
        return null;
    }

    private static Payment BuildPayment(CreatePaymentCommand command, string key)
    {
        return new Payment
        {
        Id = Guid.NewGuid(),
        UserId = command.UserId,
        IdempotencyKey = key,
        Amount = command.Amount,
        Currency = command.Currency,
        Status = PaymentStatus.Pending,
        CreatedAtUtc = DateTime.UtcNow
        };

    }
}