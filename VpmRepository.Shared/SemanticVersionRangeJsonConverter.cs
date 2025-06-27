using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using SemanticVersionRange = SemanticVersioning.Range;
namespace VpmRepository;

public sealed class SemanticVersionRangeJsonConverter : JsonConverter<SemanticVersionRange>
{
    public override SemanticVersionRange? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var semVerRangeString = reader.GetString();
        return SemanticVersionRange.Parse(semVerRangeString);
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersionRange value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override SemanticVersionRange ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var semVerRangeString = reader.GetString();
        return SemanticVersionRange.Parse(semVerRangeString);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] SemanticVersionRange value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}