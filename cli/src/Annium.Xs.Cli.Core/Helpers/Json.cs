using System.IO;
using System.Text.Json;

namespace Annium.Xs.Cli.Core.Helpers;

public static class Json
{
    private static readonly JsonSerializerOptions _options = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static T Read<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value, _options)!;
    }

    public static T ReadFile<T>(string file)
    {
        return Read<T>(File.ReadAllText(file));
    }

    public static string Write<T>(T data)
    {
        return JsonSerializer.Serialize(data, _options);
    }

    public static void WriteFile<T>(string file, T data)
    {
        File.WriteAllText(file, Write(data));
    }
}
