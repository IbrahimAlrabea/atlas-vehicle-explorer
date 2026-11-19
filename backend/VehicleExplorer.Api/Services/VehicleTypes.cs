namespace VehicleExplorer.Api.Services;

public static class VehicleTypes
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["car"] = "car", ["passenger car"] = "car",
        ["truck"] = "truck", ["multipurpose passenger vehicle (mpv)"] = "mpv", ["mpv"] = "mpv",
        ["motorcycle"] = "motorcycle", ["bus"] = "bus", ["trailer"] = "trailer",
        ["low speed vehicle (lsv)"] = "low speed vehicle", ["low speed vehicle"] = "low speed vehicle",
        ["incomplete vehicle"] = "incomplete vehicle"
    };

    public static bool TryNormalize(string? name, out string value)
    {
        if (string.IsNullOrWhiteSpace(name)) { value = string.Empty; return false; }
        return Aliases.TryGetValue(name.Trim(), out value!);
    }
}
