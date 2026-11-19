using VehicleExplorer.Api.DTOs;

namespace VehicleExplorer.Api.Services;

public interface IVehicleCatalog
{
    Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken ct);
    Task<IReadOnlyList<TypeDto>> GetTypesAsync(int makeId, CancellationToken ct);
    Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string? type, CancellationToken ct);
}
