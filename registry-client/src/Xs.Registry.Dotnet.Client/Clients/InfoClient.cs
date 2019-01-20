using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
using Newtonsoft.Json;
using Xs.Registry.Core.Client;

namespace Xs.Registry.Dotnet.Client
{
    public class InfoClient : ClientBase, IInfoClient
    {
        public async Task<IReadOnlyDictionary<string, string>> SearchAsync(string query, string token)
        {
            var result = await Http.Open(this.uri)
                .Get("info/search")
                .Param("query", query)
                .NuGetAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Search failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsAsync<Dictionary<string, string>>();

            return result;
        }

        public async Task<string> GetInfoAsync(string name, string token)
        {
            var result = await Http.Open(this.uri)
                .Get($"info/{name}")
                .NuGetAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Versions fetch failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsStringAsync();

            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result), Formatting.Indented);
        }
    }
}