using System.Runtime.InteropServices;
using Octokit;

namespace Tkmm.Core.Helpers;

public static class OctokitHelper
{
    private const string USER_AGENT = "tkmm-client-application";
    
    private static readonly string _assetName = $"tkmm-win-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}";
    
    private static readonly HttpClient _client = new() {
        DefaultRequestHeaders = {
            { "user-agent", USER_AGENT },
            { "accept", "application/octet-stream" },
            { "X-GitHub-Api-Version", "2022-11-28" },
        }
    };

    private static readonly GitHubClient _githubClient = new(
        productInformation: new ProductHeaderValue(USER_AGENT)
    );
    
    public static Task<Release> GetLatestRelease(string owner, string name)
    {
        return _githubClient
            .Repository
            .Release
            .GetLatest(owner, name);
    }
    
    public static async Task<Stream?> DownloadReleaseAsset(Release release)
    {
        ReleaseAsset? asset = release
            .Assets
            .FirstOrDefault(asset => asset.Name == _assetName);

        return asset is not null
            ? await _client.GetStreamAsync(asset.BrowserDownloadUrl)
            : null;
    }
}