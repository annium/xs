using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private readonly IHttpRequestFactory _httpRequestFactory;

        private readonly HttpClient _client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip,
            MaxConnectionsPerServer = 16,
        });

        public DependencyManager(
            IHttpRequestFactory httpRequestFactory
        )
        {
            _httpRequestFactory = httpRequestFactory;
        }

        public async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken)
        {
            var request = _httpRequestFactory.New(serverUri)
                .UseClient(_client)
                .Get(HttpUtility.UrlEncode(package.Name.ToLowerInvariant()));
            if (accessToken != null)
                request = request.BearerAuthorization(accessToken);

            var index = await request.AsAsync(new Index());
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

        private class IndexVersion
        {
        }
    }
}