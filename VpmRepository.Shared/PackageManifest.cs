using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SemanticVersion = SemanticVersioning.Version;
using SemanticVersionRange = SemanticVersioning.Range;
namespace VpmRepository;

public record UpmPackageManifest
{
    /// <summary>
    /// A unique identifier that conforms to the Unity Package Manager naming convention, which uses reverse domain name notation. 
    /// For more information about the naming convention, refer to <see href="https://docs.unity3d.com/6000.2/Documentation/Manual/cus-naming.html">Naming your package</see>.
    /// </summary>
    /// <remarks>
    /// The <c>name</c> identifier is different than the <see href="https://docs.unity3d.com/6000.2/Documentation/Manual/upm-manifestPkg.html#displayName">user-friendly display name</see> that appears in the <see href="https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui-list.html">list panel</see> in the <b>Package Manager</b> window.
    /// 
    /// </remarks>
    [JsonPropertyName("name")]
    public required string Name { get; set; } = string.Empty;
    /// <summary>
    /// The package version number, which uses the format <c>"major.minor.patch"</c>.
    /// </summary>
    [JsonPropertyName("version")]
    [JsonConverter(typeof(SemanticVersionJsonConverter))]
    public required SemanticVersion Version { get; set; }
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
    [JsonPropertyName("displayName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; set; }
    [JsonPropertyName("unity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Unity { get; set; }
    [JsonPropertyName("author")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonNode? Author { get; set; }
    [JsonPropertyName("changelogUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? ChangelogUrl { get; set; }
    /// <remarks>
    /// Some packages incorrectly use version ranges in the <c>dependencies</c> field, which is not supported by Unity Package Manager. But we support it here for compatibility.
    /// </remarks>
    [JsonPropertyName("dependencies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, SemanticVersionRange> Dependencies { get; set; } = new();
    [JsonPropertyName("documentationUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? DocumentationUrl { get; set; }
    [JsonPropertyName("hideInEditor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HideInEditor { get; set; }
    [JsonPropertyName("keywords")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Keywords { get; set; }
    [JsonPropertyName("license")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? License { get; set; }
    [JsonPropertyName("licensesUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Uri? LicensesUrl { get; set; }
    [JsonPropertyName("samples")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PackageSample[]? Samples { get; set; }
    /// <summary>
    /// <b>Reserved for internal use.</b>
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
    [JsonPropertyName("unityRelease")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UnityRelease { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
