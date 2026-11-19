using System.Net.Http.Json;
using System.Text.Json;
using VehicleExplorer.Api.DTOs;
using VehicleExplorer.Api.Services;

namespace VehicleExplorer.Api.Clients;

public sealed class NhtsaClient(HttpClient http) : INhtsaClient
{
    private async Task<IReadOnlyList<TOut>> FetchAsync<TIn, TOut>(string relativeUrl, Func<TIn, TOut> map, CancellationToken ct)
    {
        using var response = await http.GetAsync(relativeUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<VpicEnvelope<TIn>>(cancellationToken: ct);
        if (envelope?.Results is null) throw new JsonException("NHTSA response is missing Results.");
        return envelope.Results.Select(map).ToArray();
    }

    public Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken ct) =>
        FetchAsync<VpicMake, MakeDto>("getallmakes?format=json", x => new(x.Id, x.Name), ct);

    public Task<IReadOnlyList<TypeDto>> GetTypesAsync(int makeId, CancellationToken ct) =>
        FetchAsync<VpicType, TypeDto>($"GetVehicleTypesForMakeId/{makeId}?format=json",
            x => new(x.Id, x.Name, VehicleTypes.TryNormalize(x.Name, out var key) ? key : null), ct);

    public Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string? normalizedType, CancellationToken ct)
    {
        var suffix = normalizedType is null ? "" : $"/vehicletype/{Uri.EscapeDataString(normalizedType)}";
        return FetchAsync<VpicModel, ModelDto>(
            $"GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}{suffix}?format=json",
            x => new(x.Id, x.Name, x.Make, x.VehicleType), ct);
    }
}
