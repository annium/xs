using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class DependencyManager : IDependencyManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;
        public Uri DefaultServer { get; } = new Uri(Constants.DefaultServer);

        public async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken)
        {
            var request = Http.Open(serverUri).Get(HttpUtility.UrlEncode(package.Name.ToLowerInvariant()));
            if (accessToken != null)
                request = request.BearerAuthorization(accessToken);

            var index = await request.AsAsync<Index>();
            if (index is null)
                return Array.Empty<Package>();

            var registrations = index.Versions.Keys
                .Select(v =>
                {
                    try
                    {
                        if (!Core.Models.Version.TryParse(v, out var version))
                            throw new ArgumentException($"Package {package.Name} registered version {v} is invalid");

                        return (Id: index.Name, Version: version);
                    }
                    catch
                    {
                        return (Id: index.Name, Version: Core.Models.Version.Empty);
                    }
                })
                .Where(e => e.Version != Core.Models.Version.Empty)
                .OrderByDescending(e => e.Version)
                .ToArray();

            return registrations.Select(r => new Package(Type, r.Id, r.Version)).ToArray();
        }

        private class Index
        {
            public string Name { get; set; } = string.Empty;

            public Dictionary<string, IndexVersion> Versions { get; set; } = new Dictionary<string, IndexVersion>();
        }

        private class IndexVersion { }
    }
}