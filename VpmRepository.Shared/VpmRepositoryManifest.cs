using System.Text.Json.Serialization;
namespace VpmRepository;

public record VpmRepositoryManifest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("author")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Author { get; set; }
    [JsonPropertyName("url")]
    public required Uri Url { get; set; }

    [JsonPropertyName("packages")]
    public required Dictionary<string, VpmPackageVersions> Packages { get; set; }
}
