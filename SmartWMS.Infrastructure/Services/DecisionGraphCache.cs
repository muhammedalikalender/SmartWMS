namespace SmartWMS.Infrastructure.Services;

using System;
using Microsoft.Extensions.Caching.Memory;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Features.Anomaly.Models;

public class DecisionGraphCache : IDecisionGraphCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public DecisionGraphCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public DecisionGraphDto? Get(Guid anomalyId)
    {
        return _cache.Get<DecisionGraphDto>($"graph_{anomalyId}");
    }

    public void Set(Guid anomalyId, DecisionGraphDto graph)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheDuration)
            .SetSize(1); // Limit Memory usage if needed

        _cache.Set($"graph_{anomalyId}", graph, options);
    }

    public void Remove(Guid anomalyId)
    {
        _cache.Remove($"graph_{anomalyId}");
    }
}
