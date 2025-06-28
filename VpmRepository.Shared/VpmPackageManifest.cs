using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SemanticVersionRange = SemanticVersioning.Range;
namespace VpmRepository;

public record VpmPackageManifest : UpmPackageManifest
{

    [JsonPropertyName("vpmDependencies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, SemanticVersionRange>? VpmDependencies { get; set; }
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? Url { get; set; }

    [JsonPropertyName("zipSHA256")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ZipSha256 { get; set; }

    [JsonPropertyName("vrc-get")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VrcGetExtension? VrcGetExtension { get; set; }
}
