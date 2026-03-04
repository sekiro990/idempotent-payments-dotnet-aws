using System;
namespace Payments.Application.Common.Interfaces;
/// <summary>
/// Abstraction for idempotency storage (Redis locally, ElastiCache later).
/// Stores a mapping from (UserId, IdempotencyKey) -> PaymentId with TTL.
/// </summary>
public interface IIdempotencyStore
{
    Task<Guid?> GetPaymentIdAsync(string userId, string idempotencyKey, CancellationToken ct);
    Task SetPaymentIdAsync(string userId, string idempotencyKey, Guid paymentId, TimeSpan ttl, CancellationToken ct);
}