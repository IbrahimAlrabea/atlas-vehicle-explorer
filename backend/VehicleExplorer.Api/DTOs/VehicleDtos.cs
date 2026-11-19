namespace VehicleExplorer.Api.DTOs;

public sealed record MakeDto(int Id, string Name);
public sealed record TypeDto(int Id, string Name, string? FilterKey);
public sealed record ModelDto(int Id, string Name, string Make, string? VehicleType);
