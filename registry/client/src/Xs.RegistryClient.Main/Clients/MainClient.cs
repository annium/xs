using System.Threading.Tasks;
using Annium.Net.Http;
using Xs.RegistryClient.Main.Models;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Main
{
    public class MainClient : ClientBase
    {
        private readonly IHttpRequestFactory _httpRequestFactory;

        public MainClient(
            IHttpRequestFactory httpRequestFactory
        )
        {
            _httpRequestFactory = httpRequestFactory;
        }

        public Task<string> LoginAsync(string name, string password)
        {
            return _httpRequestFactory.Get(uri)
                .Post("login/app")
                .JsonContent(new { name, password })
                .EnsureSuccessStatusCode(response => $"User login failed with {response.StatusCode} ({response.StatusText}).")
                .AsAsync<string>();
        }

        public Task<Registry> GetRegistryInfoAsync()
        {
            return _httpRequestFactory.Get(uri)
                .Get("registry")
                .EnsureSuccessStatusCode(response => $"Registry info fetch failed with {response.StatusCode} ({response.StatusText}).")
                .AsAsync<Registry>();
        }

        public Task<MetaPackage[]> SearchAsync(string token, string type, string query)
        {
            return _httpRequestFactory.Get(uri)
                .Get("packages/search")
                .BearerAuthorization(token)
                .Param("type", type)
                .Param("query", query)
                .EnsureSuccessStatusCode(response => $"Search failed with {response.StatusCode} ({response.StatusText}).")
                .AsAsync<MetaPackage[]>();
        }
    }
}