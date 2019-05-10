using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Extensions.Net.Http;
using Newtonsoft.Json;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class DependencyManager : IDependencyManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        private const string RegistrationsBaseUrlService = "RegistrationsBaseUrl/Versioned";

        public async Task<Package[]> GetVersionsAsync(Package package, Configuration configuration)
        {
            var registryUri = configuration.Servers.FirstOrDefault(s => s.Key == Type).Value;

            // try get result from registry
            var result = (registryUri?.IsFile ?? true) ? null : await ResolveVersionsAsync(package, registryUri);

            // fallback to default server result
            return result.Length == 0 ? await ResolveVersionsAsync(package, new Uri(Constants.DefaultServer)) : result;
        }

        private async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri)
        {
            var registrationBaseUrl = (await Http.Open(serverUri).Get(Constants.ServerPathSuffix).AsAsync<ServiceIndex>())
                .Resources.First(r => r.Type == RegistrationsBaseUrlService).Id;

            var registrationUrl = registrationBaseUrl +
                (registrationBaseUrl.EndsWith('/') ? string.Empty : "/") +
                $"{HttpUtility.UrlEncode(package.Name.ToLowerInvariant())}/index.json";

            var registrations = ((await Http.Open().Get(registrationUrl).AsAsync<RegistrationIndex>()) ??
                    new RegistrationIndex { Items = Array.Empty<RegistrationPage>() })
                .Items.SelectMany(i => i.Items)
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
            public RegistrationPage[] Items { get; set; }
        }

        private class RegistrationPage
        {
            public RegistrationLeaf[] Items { get; set; }
        }

        private class RegistrationLeaf
        {
            public RegistrationCatalogEntry CatalogEntry { get; set; }
        }

        private class RegistrationCatalogEntry
        {
            public string Id { get; set; }

            public string Version { get; set; }
        }
    }
}