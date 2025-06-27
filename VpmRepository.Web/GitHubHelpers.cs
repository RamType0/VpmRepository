using System.Text.RegularExpressions;

namespace VpmRepository.Web;

public static partial class GitHubHelpers
{
    [GeneratedRegex(@"https://github\.com/(?<owner>[^/]+)/(?<repo>[^/]+).+")]
    public static partial Regex GitHubOwnerAndRepoRegex();
}
