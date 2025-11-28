using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Microsoft.Extensions.DependencyInjection;
using Version = Annium.Xs.Cli.Core.Models.Version;

namespace Annium.Xs.Cli.Node.Projects;

internal class DependencyManager : IDependencyManager
{
    public ProjectType Type => Constants.ProjectType;
    public Uri DefaultServer { get; } = new(Constants.DefaultServer);
    private readonly IHttpRequestFactory _httpRequestFactory;

    public DependencyManager([FromKeyedServices("node")] IHttpRequestFactory httpRequestFactory)
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public async Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken)
    {
        var request = _httpRequestFactory
            .New(serverUri)
            .Get(HttpUtility.UrlEncode(package.Name.ToLowerInvariant()))
            .BearerAuthorization(accessToken);

        var index = await request.AsAsync(new Index());
        var registrations = index
            .Versions.Keys.Select(v =>
            {
                try
                {
                    if (!Version.TryParse(v, out var version))
                        throw new ArgumentException($"Package {package.Name} registered version {v} is invalid");

                    return (Id: index.Name, Version: version);
                }
                catch
                {
                    return (Id: index.Name, Version: Version.Empty);
                }
            })
            .Where(e => e.Version != Version.Empty)
            .OrderByDescending(e => e.Version)
            .ToArray();

        return registrations.Select(r => new Package(Type, r.Id, r.Version)).ToArray();
    }

    private class Index
    {
        public string Name { get; set; } = string.Empty;

        public Dictionary<string, IndexVersion> Versions { get; set; } = new();
    }

    private class IndexVersion { }
}
