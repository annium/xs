using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Newtonsoft.Json;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class DependencyManager : IDependencyManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public Uri DefaultServer { get; } = new Uri(Constants.DefaultServer);

        private const string RegistrationsBaseUrlService = "RegistrationsBaseUrl/Versioned";

        public async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken)
        {
            var registrationBaseUrl = (await Http.Open(serverUri).Get(Constants.ServerPathSuffix).AsAsync<ServiceIndex>())
                .Resources.First(r => r.Type == RegistrationsBaseUrlService).Id;

            var registrationUrl = registrationBaseUrl +
                (registrationBaseUrl.EndsWith('/') ? string.Empty : "/") +
                $"{HttpUtility.UrlEncode(package.Name.ToLowerInvariant())}/index.json";

            var index = await LoadIndexAsync(registrationUrl);
            if (index == null)
                return Array.Empty<Package>();

            var registrations = index.Items
                .SelectMany(i => i.Items)
                .Select(i => i.CatalogEntry)
                .Select(e =>
                {
                    try
                    {
                        var version = new Core.Models.Version(e.Version);

                        return (Id: e.Id, Version: version);
                    }
                    catch
                    {
                        return (Id: e.Id, Version: null);
                    }
                })
                .Where(e => e.Version != null)
                .OrderByDescending(e => e.Version)
                .ToArray();

            return registrations.Select(r => new Package(Type, r.Id, r.Version)).ToArray();
        }

        private async Task<RegistrationIndex> LoadIndexAsync(string registrationUrl)
        {
            var index = await Http.Open().Get(registrationUrl).AsAsync<RegistrationIndex>();
            if (index == null)
                return null;

            index.Items = (await Task.WhenAll(index.Items.Select(page =>
            {
                if (page.Items.Length > 0)
                    return Task.FromResult(page);

                return Http.Open().Get(page.Id).AsAsync<RegistrationPage>();
            }))).OfType<RegistrationPage>().ToArray();

            return index;
        }

        private class ServiceIndex
        {
            public ServiceIndexResource[] Resources { get; set; }
        }

        private class ServiceIndexResource
        {
            [JsonProperty("@id")]
            public string Id { get; set; }

            [JsonProperty("@type")]
            public string Type { get; set; }
        }

        private class RegistrationIndex
        {
            public RegistrationPage[] Items { get; set; } = Array.Empty<RegistrationPage>();
        }

        private class RegistrationPage
        {
            [JsonProperty("@id")]
            public string Id { get; set; }

            public RegistrationLeaf[] Items { get; set; } = Array.Empty<RegistrationLeaf>();
        }

        private class RegistrationLeaf
        {
            public RegistrationCatalogEntry CatalogEntry { get; set; } = new RegistrationCatalogEntry();
        }

        private class RegistrationCatalogEntry
        {
            public string Id { get; set; }

            public string Version { get; set; }
        }
    }
}