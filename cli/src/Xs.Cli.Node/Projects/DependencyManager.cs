using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Extensions.Net.Http;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class DependencyManager : IDependencyManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public async Task<Package[]> GetVersionsAsync(Package package, Configuration configuration)
        {
            var registryUri = configuration.Servers.FirstOrDefault(s => s.Key == Type).Value;

            // try get result from registry
            var result = (registryUri?.IsFile ?? true) ? null : await ResolveVersionsAsync(package, registryUri, configuration.Token);

            // fallback to default server result
            return result.Length == 0 ? await ResolveVersionsAsync(package, new Uri(Constants.DefaultServer), null) : result;
        }

        private async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string token)
        {
            var request = Http.Open(serverUri).Get(HttpUtility.UrlEncode(package.Name.ToLowerInvariant()));
            if (token != null)
                request = request.BearerAuthorization(token);

            var index = await request.AsAsync<Index>();
            if (index == null)
                return Array.Empty<Package>();

            var registrations = index.Versions.Keys
                .Select(v =>
                {
                    try
                    {
                        var version = new Core.Models.Version(v);

                        return (Id: index.Name, Version: version);
                    }
                    catch
                    {
                        return (Id: index.Name, Version: null);
                    }
                })
                .Where(e => e.Version != null)
                .OrderByDescending(e => e.Version)
                .ToArray();

            return registrations.Select(r => new Package(Type, r.Id, r.Version)).ToArray();
        }

        private class Index
        {
            public string Name { get; set; }

            public Dictionary<string, IndexVersion> Versions { get; set; } = new Dictionary<string, IndexVersion>();
        }

        private class IndexVersion { }
    }
}