using System;
using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Annium.Xs.Server.Client.Internal;

namespace Annium.Xs.Server.Client.Clients;

public class ServerClient : ClientBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public ServerClient(IHttpRequestFactory httpRequestFactory)
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public async Task DeletePackageAsync(string token, string name, string version)
    {
        var response = await _httpRequestFactory
            .New(Uri)
            .Delete($"packages/{HttpUtility.UrlEncode(name)}/{version}")
            .BearerAuthorization(token)
            .RunAsync();

        if (response.IsFailure)
            throw new Exception($"Delete package failed with {response.StatusCode} ({response.StatusText}).");
    }
}
