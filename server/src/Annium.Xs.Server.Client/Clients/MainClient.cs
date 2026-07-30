using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Xs.Server.Client.Internal;
using Annium.Xs.Server.Client.Models;

namespace Annium.Xs.Server.Client.Clients;

public class MainClient : ClientBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public MainClient(IHttpRequestFactory httpRequestFactory)
    {
        _httpRequestFactory = httpRequestFactory;
    }

    public async Task<string> LoginAsync(string login, string password)
    {
        var response = await _httpRequestFactory
            .New(Uri)
            .Post("login")
            .JsonContent(new { login, password })
            .AsResponseAsync<string>();

        response.EnsureSuccess("User login");

        return response.Data.NotNull();
    }

    public async Task<Registry> GetRegistryInfoAsync()
    {
        var response = await _httpRequestFactory.New(Uri).Get("registry").AsResponseAsync<Registry>();

        response.EnsureSuccess("Registry info fetch");

        return response.Data.NotNull();
    }

    public async Task<MetaPackage[]> SearchAsync(string token, string type, string query)
    {
        var response = await _httpRequestFactory
            .New(Uri)
            .Get("packages/search")
            .BearerAuthorization(token)
            .Param("type", type)
            .Param("query", query)
            .AsResponseAsync<MetaPackage[]>();

        response.EnsureSuccess("Search");

        return response.Data.NotNull();
    }
}
