using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Server;

public class ServerClient : ClientBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public ServerClient(
        IHttpRequestFactory httpRequestFactory
    )
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public Task DeletePackageAsync(string token, string name, string version)
    {
        return _httpRequestFactory.New(Uri)
            .Delete($"packages/{HttpUtility.UrlEncode(name)}/{version}")
            .BearerAuthorization(token)
            .EnsureSuccessStatusCode(response => $"Delete package failed with {response.StatusCode} ({response.StatusText}).")
            .RunAsync();
    }
}