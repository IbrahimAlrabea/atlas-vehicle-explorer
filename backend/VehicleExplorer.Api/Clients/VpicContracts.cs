using System.Text.Json.Serialization;

namespace VehicleExplorer.Api.Clients;

internal sealed record VpicEnvelope<T>([property: JsonPropertyName("Results")] List<T>? Results);
internal sealed record VpicMake([property: JsonPropertyName("Make_ID")] int Id, [property: JsonPropertyName("Make_Name")] string Name);
internal sealed record VpicType([property: JsonPropertyName("VehicleTypeId")] int Id, [property: JsonPropertyName("VehicleTypeName")] string Name);
internal sealed record VpicModel([property: JsonPropertyName("Model_ID")] int Id, [property: JsonPropertyName("Model_Name")] string Name, [property: JsonPropertyName("Make_Name")] string Make, [property: JsonPropertyName("VehicleTypeName")] string? VehicleType);
