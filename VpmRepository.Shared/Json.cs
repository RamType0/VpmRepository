using System.Text.Json;
using System.Text.Json.Serialization;
namespace VpmRepository;

public static class Json
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        Converters =
        {
            new SemanticVersionJsonConverter(),
            new SemanticVersionRangeJsonConverter(),
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}