/// <summary>
/// Handles idempotent payment creation.
/// Flow:
/// 1. Check Redis for cached idempotency key.
/// 2. If found, return existing payment (replay).
/// 3. If not found, check database.
/// 4. Insert new payment if none exists.
/// 5. Handle concurrent insert via unique constraint (Postgres 23505).
/// 6. Cache the result in Redis for fast future replays.
/// </summary>

using Payments.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Npgsql;
namespace Payments.Application.Payments.CreatePayment;

public class CreatePaymentHandler
{
    private readonly IPaymentsDbContext _db;
    private readonly IIdempotencyStore _idempo;

    public CreatePaymentHandler(IPaymentsDbContext db, IIdempotencyStore idempotencyStore)
    {
        _db = db;
        _idempo = idempotencyStore;
    }

    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand command, CancellationToken ct)
    {
        if(command == null) throw new ArgumentNullException(nameof(command));
        if(string.IsNullOrEmpty(command.UserId)) throw new ArgumentException("UserId is required", nameof(command.UserId));
        if(string.IsNullOrEmpty(command.IdempotencyKey)) throw new ArgumentException("IdempotencyKey is required", nameof(command.IdempotencyKey));
        if(command.Amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(command.Amount));
        if(string.IsNullOrWhiteSpace(command.Currency)) throw new ArgumentException("Currency is required", nameof(command.Currency));
        var key = command.IdempotencyKey.Trim();

        var cachedPaymentId = await _idempo.GetPaymentIdAsync(command.UserId, key, ct);

        if(cachedPaymentId.HasValue)
        {
            var cachedPayment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == cachedPaymentId.Value, ct);
            if(cachedPayment != null)
            {
                return new CreatePaymentResult
                {
                    IsReplay = true,
                    Payment = cachedPayment
                };
            }
        }

        

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
                _idempo.SetPaymentIdAsync(command.UserId, key, payment.Id, TimeSpan.FromMinutes(15), ct).Wait(ct);
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync(ct);
                return new CreatePaymentResult
                {
                    IsReplay = false,
                    Payment = payment
                };
            }
            // Postgres unique constraint violation (23505)
            // This occurs if two concurrent requests attempt to insert
            // the same (UserId, IdempotencyKey).
            // In this case, we re-query and return the already-created payment.
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