using System.Text.Json.Serialization;
using SemanticVersion = SemanticVersioning.Version;
namespace VpmRepository;

public record VpmPackageVersions
{
    [JsonPropertyName("versions")]
    public required Dictionary<SemanticVersion, VpmPackageManifest> Versions { get; set; } = new();
}
