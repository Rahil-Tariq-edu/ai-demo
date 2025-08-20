using Microsoft.Extensions.Caching.Memory;

namespace KbApi.Services;

public class RateLimitService
{
    private readonly IMemoryCache _cache;
    private const int LimitPerHour = 60;

    public RateLimitService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> IsAllowedAsync(HttpContext context)
    {
        var userKey = context.User?.Identity?.Name ?? context.Request.Headers["X-Demo-User"].FirstOrDefault() ?? "anonymous";
        var key = $"rl:{userKey}:{DateTime.UtcNow:yyyyMMddHH}";
        var count = _cache.GetOrCreate<int>(key, entry => { entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); return 0; });
        if (count >= LimitPerHour) return Task.FromResult(false);
        _cache.Set(key, count + 1, TimeSpan.FromHours(1));
        return Task.FromResult(true);
    }
}

