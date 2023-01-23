using System.Threading.Tasks;
using Annium.Net.Http;
using Server.Client.Internal;
using Server.Client.Models;

namespace Server.Client.Clients;

public class MainClient : ClientBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public MainClient(
        IHttpRequestFactory httpRequestFactory
    )
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public Task<string> LoginAsync(string login, string password)
    {
        return _httpRequestFactory.New(Uri)
            .Post("login")
            .JsonContent(new { login, password })
            .EnsureSuccessStatusCode(response => $"User login failed with {response.StatusCode} ({response.StatusText}).")
            .AsAsync<string>();
    }

    public Task<Registry> GetRegistryInfoAsync()
    {
        return _httpRequestFactory.New(Uri)
            .Get("registry")
            .EnsureSuccessStatusCode(response => $"Registry info fetch failed with {response.StatusCode} ({response.StatusText}).")
            .AsAsync<Registry>();
    }

    public Task<MetaPackage[]> SearchAsync(string token, string type, string query)
    {
        return _httpRequestFactory.New(Uri)
            .Get("packages/search")
            .BearerAuthorization(token)
            .Param("type", type)
            .Param("query", query)
            .EnsureSuccessStatusCode(response => $"Search failed with {response.StatusCode} ({response.StatusText}).")
            .AsAsync<MetaPackage[]>();
    }
}