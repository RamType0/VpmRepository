using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Version = SemanticVersioning.Version;
namespace VpmRepository.Build;
public record VpmRepositorySettings
{
    public required string Name { get; init; }
    public required string Id { get; init; }
    public string? Author { get; init; }
    public required Uri Url { get; init; }
    public Uri[] PackageZipUrls { get; init; } = [];

    public Dictionary<string, string[]> GitHubRepositories { get; init; } = [];

    public Uri[] IncludedVpmRepositoryManifestUrls { get; init; } = [];
}

