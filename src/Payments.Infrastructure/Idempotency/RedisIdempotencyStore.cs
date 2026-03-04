using Payments.Application.Common.Interfaces;
using StackExchange.Redis;
namespace Payments.Infrastructure.Idempotency;
/// <summary>
/// Redis implementation of idempotency store.
/// Key format: idempo:{userId}:{idempotencyKey}
/// Value: paymentId (Guid as string)
/// DB remains the source of truth.
/// </summary>
public class RedisIdempotencyStore: IIdempotencyStore
{
    private readonly IDatabase _redis;

    public RedisIdempotencyStore(IConnectionMultiplexer multiplexer)
    {
        _redis = multiplexer.GetDatabase();
    }

    private static string BuildKey(string userId, string idempotencyKey) => $"idempo:{userId}:{idempotencyKey}";

    public async Task<Guid?> GetPaymentIdAsync(string userId, string idempotencyKey, CancellationToken ct)
    {
        var key = BuildKey(userId, idempotencyKey);
        // Fast lookup: if present, we return PaymentId and treat request as replay.
        var val = await _redis.StringGetAsync(key);

        if (!val.HasValue) return null;

        return Guid.TryParse(val.ToString(), out var id) ? id: null;

    }

    public async Task SetPaymentIdAsync(string userId, string idempotencyKey, Guid paymentId, TimeSpan ttl, CancellationToken ct)
    {
        var key = BuildKey(userId, idempotencyKey);
        // Cache for a short TTL (e.g., 15 minutes). This speeds up repeated requests.
        await _redis.StringSetAsync(key, paymentId.ToString(), expiry: ttl);
    }
}