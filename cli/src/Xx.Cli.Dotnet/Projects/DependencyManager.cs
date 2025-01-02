using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Annium;
using Annium.Net.Http;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Cli.Dotnet.Projects;

internal class DependencyManager : IDependencyManager
{
    public ProjectType Type => Constants.ProjectType;
    public Uri DefaultServer { get; } = new(Constants.DefaultServer);
    private const string RegistrationsBaseUrlService = "RegistrationsBaseUrl/Versioned";
    private readonly IHttpRequestFactory _httpRequestFactory;

    private readonly HttpClient _client =
        new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip, MaxConnectionsPerServer = 16 });

    public DependencyManager(IHttpRequestFactory httpRequestFactory)
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken)
    {
        var serverIndex = await _httpRequestFactory
            .New(serverUri)
            .UseClient(_client)
            .Get(Constants.ServerPathSuffix)
            .AsAsync<ServiceIndex>();
        var registrationBaseUrl = serverIndex.NotNull().Resources.First(r => r.Type == RegistrationsBaseUrlService).Id;

        var registrationUrl =
            registrationBaseUrl
            + (registrationBaseUrl.EndsWith('/') ? string.Empty : "/")
            + $"{HttpUtility.UrlEncode(package.Name.ToLowerInvariant())}/index.json";

        var index = await LoadIndexAsync(registrationUrl);
        if (index is null)
            return Array.Empty<Package>();

        var registrations = index
            .Items.SelectMany(i => i.Items)
            .Select(i => i.CatalogEntry)
            .Select(e =>
            {
                try
                {
                    if (!Version.TryParse(e.Version, out var version))
                        throw new ArgumentException($"Package {e.Id} version {e.Version} is invalid");

                    return (e.Id, Version: version);
                }
                catch
                {
                    return (e.Id, Version: Version.Empty);
                }
            })
            .Where(e => e.Version != Version.Empty)
            .OrderByDescending(e => e.Version)
            .ToArray();

        return registrations.Select(r => new Package(Type, r.Id, r.Version)).ToArray();
    }

    private async Task<RegistrationIndex?> LoadIndexAsync(string registrationUrl)
    {
        var index = await _httpRequestFactory
            .New()
            .UseClient(_client)
            .Get(registrationUrl)
            .AsAsync(new RegistrationIndex());
        index.Items = await Task.WhenAll(
            index.Items.Select(async page =>
            {
                if (page.Items.Length > 0)
                    return page;

                var result = await _httpRequestFactory
                    .New()
                    .UseClient(_client)
                    .Get(page.Id)
                    .AsAsync<RegistrationPage>();
                return result.NotNull();
            })
        );

        return index;
    }

    private class ServiceIndex
    {
        public ServiceIndexResource[] Resources { get; set; } = Array.Empty<ServiceIndexResource>();
    }

    private class ServiceIndexResource
    {
        [JsonPropertyName("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("@type")]
        public string Type { get; set; } = string.Empty;
    }

    private class RegistrationIndex
    {
        public RegistrationPage[] Items { get; set; } = Array.Empty<RegistrationPage>();
    }

    private class RegistrationPage
    {
        [JsonPropertyName("@id")]
        public string Id { get; set; } = string.Empty;

        public RegistrationLeaf[] Items { get; set; } = Array.Empty<RegistrationLeaf>();
    }

    private class RegistrationLeaf
    {
        public RegistrationCatalogEntry CatalogEntry { get; set; } = new();
    }

    private class RegistrationCatalogEntry
    {
        public string Id { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;
    }
}
