using VehicleExplorer.Api.DTOs;

namespace VehicleExplorer.Api.Clients;

public interface INhtsaClient
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken ct);
    Task<IReadOnlyList<TypeDto>> GetTypesAsync(int makeId, CancellationToken ct);
    Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string? normalizedType, CancellationToken ct);
}
