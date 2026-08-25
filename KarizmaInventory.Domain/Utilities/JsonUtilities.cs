using System.Text.Json;

namespace KarizmaPlatform.Inventory.Domain.Utilities;

/// <summary>
/// Helpers for reading the jsonb columns of the inventory tables (price, metadata) into
/// the generic types the consumer supplies when registering the inventory module.
/// </summary>
public static class JsonUtilities
{
    public static bool IsNullOrEmptyJson(string? json)
    {
        return string.IsNullOrWhiteSpace(json) || json == "null";
    }

    public static T? Deserialize<T>(string? json, JsonSerializerOptions? options = null)
    {
        if (IsNullOrEmptyJson(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json!, options);
        }
        catch
        {
            return default;
        }
    }

    public static string? Serialize<T>(T? value, JsonSerializerOptions? options = null)
    {
        return value is null ? null : JsonSerializer.Serialize(value, options);
    }
}
