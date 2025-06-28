using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.CI.GitHubActions;
using Octokit;
using VpmRepository.Build;
using System.Threading.Tasks;
using System.Collections.Generic;
using VpmRepository;
using System.Net.Http;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Buffers;
using System.Text.Json;
using Nuke.Common.Utilities;
using System.Net.Http.Json;
[GitHubActions(
        "build-repository",
        GitHubActionsImage.UbuntuLatest,
        On = new[] { GitHubActionsTrigger.WorkflowDispatch, GitHubActionsTrigger.Push },
        EnableGitHubToken = true,
        AutoGenerate = false,
        InvokedTargets = new[] { nameof(Publish) })]
class Build : NukeBuild
{

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Output directory")]
    readonly string? Output;

    static GitHubActions GitHubActions => GitHubActions.Instance;

    GitHubClient? _client;
    GitHubClient GitHubClient
    {
        get
        {
            if (_client == null)
            {
                _client = new(new ProductHeaderValue("VRChat-Package-Manager-Automation"));
                if (IsServerBuild)
                {
                    _client.Credentials = new Credentials(GitHubActions.Token);
                }
            }

            return _client;
        }
    }

    static HttpClient HttpClient { get; } = new()
    {
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new("VCCBootstrap","1.0"),
            },
        }
    };

    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    JsonSerializerOptions JsonSerializerOptions { get; } = new(Json.JsonSerializerOptions)
    {
        WriteIndented = IsLocalBuild,
    };

    static VpmRepositoryBuildSettings Settings { get; } = new()
    {
        Id = "com.ramtype0.vpm-repository",
        Name = GlobalSettings.RepositoryName,
        Author = "Ram.Type-0",
        Url = new(IsLocalBuild ? $"https://ramtype0.github.io/VpmRepository/{GlobalSettings.PublishedRepositoryManifestFileName}" : $"https://{GitHubActions.RepositoryOwner}.github.io/{GitHubActions.Repository.Split('/')[1]}/{GlobalSettings.PublishedRepositoryManifestFileName}"),
        GitHubRepositories =
                {
                    ["RamType0"] = ["Meshia.MeshSimplification"],
                    ["bdunderscore"] =["ndmf", "modular-avatar"],
                    ["anatawa12"] = ["CustomLocalization4EditorExtension"],
                },
    };

    Target RefreshVpmRepositoryManifest => _ => _
        .Executes(async () =>
        {
            AbsolutePath vpmRepositoryManifestJsonPath = (WorkingDirectory / "VpmRepository.Web" / "wwwroot" / GlobalSettings.PublishedRepositoryManifestFileName);
            
            VpmRepositoryManifest? existingVpmRepositoryManifest = null;
            if (vpmRepositoryManifestJsonPath.FileExists())
            {
                var vpmRepositoryManifestJsonBytes = vpmRepositoryManifestJsonPath.ReadAllBytes();
                try
                {
                    existingVpmRepositoryManifest = JsonSerializer.Deserialize<VpmRepositoryManifest>(vpmRepositoryManifestJsonBytes, JsonSerializerOptions);
                }
                catch (Exception)
                {
                    Serilog.Log.Warning($"Failed to deserialize {vpmRepositoryManifestJsonPath}. The file may be corrupted or in an unsupported format.");
                }
            }

            if(existingVpmRepositoryManifest is null)
            {
                try
                {
                    existingVpmRepositoryManifest = await HttpClient.GetFromJsonAsync<VpmRepositoryManifest>(Settings.Url, JsonSerializerOptions);
                }
                catch (Exception)
                {
                    Serilog.Log.Warning($"Failed to download existing VPM repository manifest from {Settings.Url}. It may not exist or the URL is incorrect.");
                }
            }

            Dictionary<Uri, VpmPackageManifest> existingPackageManifests = existingVpmRepositoryManifest?.Packages
                .SelectMany(keyValue => keyValue.Value.Versions.Values)
                .Where(packageManifest => packageManifest.Url is not null)
                .ToDictionary(packageManifest => packageManifest.Url!) ?? [];

            HashSet<VpmPackageManifest> vpmPackageManifests = new(UpmPackageManifestNameVersionEqualityComparer.Instance);

            foreach (var (owner, repoNames) in Settings.GitHubRepositories)
            {
                foreach (var repoName in repoNames)
                {
                    await foreach (var packageManifest in GetReleasedVpmPackageManifests(owner, repoName, existingPackageManifests))
                    {
                        vpmPackageManifests.Add(packageManifest);
                    }
                }
            }

            foreach (var packageZipUrl in Settings.PackageZipUrls)
            {
                var packageManifest = await GetVpmPackageManifestAsync(packageZipUrl, existingPackageManifests);
                if (packageManifest is null)
                {
                    throw new ArgumentException($"Package manifest for {packageZipUrl} is null. The package may not contain a valid package.json file or the URL is incorrect.");
                }
                vpmPackageManifests.Add(packageManifest);
            }

            foreach (var includedManifestUrl in Settings.IncludedVpmRepositoryManifestUrls)
            {
                var includedManifest = await HttpClient.GetFromJsonAsync<VpmRepositoryManifest>(includedManifestUrl, JsonSerializerOptions);
                if (includedManifest is null)
                {
                    throw new ArgumentException($"Included VPM repository manifest at {includedManifestUrl} is null.");
                }
                
                foreach (var packageManifest in includedManifest.Packages.Values.SelectMany(packageVersions => packageVersions.Versions.Values))
                {
                    vpmPackageManifests.Add(packageManifest);
                }

            }

            VpmRepositoryManifest vpmRepositoryManifest = new()
            {
                Id = Settings.Id,
                Name = Settings.Name,
                Author = Settings.Author,
                Url = Settings.Url,
                Packages = vpmPackageManifests
                    .GroupBy(packageManifest => packageManifest.Name)
                    .ToDictionary(
                        group => group.Key,
                        group => new VpmPackageVersions()
                        {
                            Versions = group.ToDictionary(packageManifest => packageManifest.Version),
                        }),
            };

            vpmRepositoryManifestJsonPath.WriteAllBytes(JsonSerializer.SerializeToUtf8Bytes(vpmRepositoryManifest, JsonSerializerOptions));
        });
    Target Publish => _ => _
        .DependsOn(RefreshVpmRepositoryManifest)
        .Executes(() =>
        {
            DotNetPublish(config => config
                .SetProject(Solution.VpmRepository_Web)
                .SetConfiguration(Configuration)
                .AddProperty("GHPages", true)
                .WhenNotNull(Output, (_, output) => _.SetOutput(output))
            );
        });

    async IAsyncEnumerable<VpmPackageManifest> GetReleasedVpmPackageManifests(string owner, string name, Dictionary<Uri, VpmPackageManifest> existingPackageManifests)
    {
        var repository = await GitHubClient.Repository.Get(owner, name) ?? throw new ArgumentException($"Repository {owner}/{name} not found.");

        var releases = await GitHubClient.Repository.Release.GetAll(owner, name);

        foreach (var release in releases)
        {
            if(release.Assets.Any(asset => asset.Name == "package.json") && release.Assets.FirstOrDefault(asset => asset.Name.EndsWith(".zip")) is { } packageZipAsset)
            {
                // Octokit does not exposes asset digest which contains SHA256 hash of the package zip, so we need to download it and compute hash manually.
                // To compute SHA256 hash of the package zip, we need to download it.
                var packageZipDownloadUrl = packageZipAsset.BrowserDownloadUrl;
                
                var packageManifest = await GetVpmPackageManifestAsync(new(packageZipDownloadUrl, UriKind.Absolute), existingPackageManifests);

                if(packageManifest is not null)
                {
                    yield return packageManifest;
                }
            }
        }
    }

    public async ValueTask<VpmPackageManifest?> GetVpmPackageManifestAsync(Uri packageZipUrl, Dictionary<Uri, VpmPackageManifest> existingPackageManifests)
    {
        if(existingPackageManifests.TryGetValue(packageZipUrl, out var existingPackageManifest))
        {
            return existingPackageManifest;
        }
        var packageZipDownloadResponse = await HttpClient.GetAsync(packageZipUrl, HttpCompletionOption.ResponseContentRead);
        packageZipDownloadResponse.EnsureSuccessStatusCode();
        var packageZipStream = await packageZipDownloadResponse.Content.ReadAsStreamAsync();


        using ZipArchive packageZip = new(packageZipStream);
        var packageJsonEntry = packageZip.GetEntry("package.json");
        if (packageJsonEntry is null)
        {
            return null;
        }
        var packageManifest = await JsonSerializer.DeserializeAsync<VpmPackageManifest>(packageJsonEntry.Open(), JsonSerializerOptions);

        if (packageManifest is null)
        {
            return null;
        }

        packageManifest.Url = packageZipUrl;


        var sha256Buffer = ArrayPool<byte>.Shared.Rent(SHA256.HashSizeInBytes);
        try
        {
            var sha256 = sha256Buffer.AsMemory()[..SHA256.HashSizeInBytes];
            packageZipStream.Position = 0; // Reset the stream position to the beginning before computing the hash
            await SHA256.HashDataAsync(packageZipStream, sha256);
            var sha256String = Convert.ToHexStringLower(sha256.Span);
            packageManifest.ZipSha256 = sha256String;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sha256Buffer);
        }
        return packageManifest;
    }
}
