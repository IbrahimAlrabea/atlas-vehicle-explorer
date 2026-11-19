using Microsoft.Extensions.Caching.Memory;
using VehicleExplorer.Api.Clients;
using VehicleExplorer.Api.DTOs;

namespace VehicleExplorer.Api.Services;

public sealed class VehicleCatalogService(INhtsaClient client, IMemoryCache cache) : IVehicleCatalog
{
    private async Task<IReadOnlyList<T>> CachedAsync<T>(string key, TimeSpan ttl,
        Func<CancellationToken, Task<IReadOnlyList<T>>> load, CancellationToken ct)
    {
        if (cache.TryGetValue(key, out IReadOnlyList<T>? cached) && cached is not null) return cached;
        var values = await load(ct);
        cache.Set(key, values, ttl);
        return values;
    }

    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken ct) =>
        CachedAsync("makes", TimeSpan.FromHours(12), client.GetMakesAsync, ct);

    public Task<IReadOnlyList<TypeDto>> GetTypesAsync(int makeId, CancellationToken ct) =>
        CachedAsync($"types:{makeId}", TimeSpan.FromHours(6), token => client.GetTypesAsync(makeId, token), ct);

    public Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string? type, CancellationToken ct)
    {
        string? normalized = null;
        if (type is not null)
        {
            if (!VehicleTypes.TryNormalize(type, out var value)) throw new ArgumentException("Unsupported vehicle type.");
            normalized = value;
        }
        return CachedAsync($"models:{makeId}:{year}:{normalized}", TimeSpan.FromHours(3),
            token => client.GetModelsAsync(makeId, year, normalized, token), ct);
    }
}
