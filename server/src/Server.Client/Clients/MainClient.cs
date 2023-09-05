using System;
using System.Threading.Tasks;
using Annium;
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

    public async Task<string> LoginAsync(string login, string password)
    {
        var response = await _httpRequestFactory.New(Uri)
            .Post("login")
            .JsonContent(new { login, password })
            .AsResponseAsync<string>();

        if (response.IsFailure)
            throw new Exception($"User login failed with {response.StatusCode} ({response.StatusText}).");

        return response.Data.NotNull();
    }

    public async Task<Registry> GetRegistryInfoAsync()
    {
        var response = await _httpRequestFactory.New(Uri)
            .Get("registry")
            .AsResponseAsync<Registry>();

        if (response.IsFailure)
            throw new Exception($"Registry info fetch failed with {response.StatusCode} ({response.StatusText}).");

        return response.Data.NotNull();
    }

    public async Task<MetaPackage[]> SearchAsync(string token, string type, string query)
    {
        var response = await _httpRequestFactory.New(Uri)
            .Get("packages/search")
            .BearerAuthorization(token)
            .Param("type", type)
            .Param("query", query)
            .AsResponseAsync<MetaPackage[]>();

        if (response.IsFailure)
            throw new Exception($"Search failed with {response.StatusCode} ({response.StatusText}).");

        return response.Data.NotNull();
    }
}