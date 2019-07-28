using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Net.Http;
using Xs.RegistryClient.Main.Models;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Main
{
    public class MainClient : ClientBase
    {
        public MainClient()
        {

        }

        public Task<string> LoginAsync(string name, string password)
        {
            return Http.Open(this.uri)
                .Post("login/app")
                .JsonContent(new { name, password })
                .EnsureSuccessStatusCode(response => $"User login failed with {response.StatusCode} ({response.ReasonPhrase}).")
                .AsAsync<string>();
        }

        public Task<Dictionary<string, Uri>> GetRegistryInfoAsync()
        {
            return Http.Open(this.uri)
                .Get("registry")
                .EnsureSuccessStatusCode(response => $"Registry info fetch failed with {response.StatusCode} ({response.ReasonPhrase}).")
                .AsAsync<Dictionary<string, Uri>>();
        }

        public Task<MetaPackage[]> SearchAsync(string token, string type, string query)
        {
            return Http.Open(this.uri)
                .Get("packages/search")
                .BearerAuthorization(token)
                .Param("type", type)
                .Param("query", query)
                .EnsureSuccessStatusCode(response => $"Search failed with {response.StatusCode} ({response.ReasonPhrase}).")
                .AsAsync<MetaPackage[]>();
        }
    }
}