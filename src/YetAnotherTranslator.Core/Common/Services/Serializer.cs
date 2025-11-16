using System.Text.Json;

namespace YetAnotherTranslator.Core.Common.Services;

public interface ISerializer
{
    string Serialize<T>(T? value);
    T? Deserialize<T>(string data);
}

internal class Serializer : ISerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize<T>(T? value)
    {
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    public T? Deserialize<T>(string data)
    {
        return JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
    }
}
