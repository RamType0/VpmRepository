using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
namespace VpmRepository;

public record VrcGetExtension
{
    [JsonPropertyName("yanked")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonValue? Yanked { get; set; }
    [JsonPropertyName("aliases")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Aliases { get; set; }
}