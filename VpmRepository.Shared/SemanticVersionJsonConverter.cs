using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using SemanticVersion = SemanticVersioning.Version;
using SemanticVersionRange = SemanticVersioning.Range;
namespace VpmRepository;

public sealed class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var semVerString = reader.GetString();
        return SemanticVersion.Parse(semVerString);
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override SemanticVersion ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var semVerString = reader.GetString();
        return SemanticVersion.Parse(semVerString);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] SemanticVersion value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
