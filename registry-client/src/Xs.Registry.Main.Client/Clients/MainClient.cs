using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
using Xs.Registry.Shared.Client;

namespace Xs.Registry.Main.Client
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
                .AsStringAsync();
        }

        public Task<Dictionary<string, Uri>> GetRegistryInfoAsync(string token)
        {
            return Http.Open(this.uri)
                .Get("registry")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Registry info fetch failed with {response.StatusCode} ({response.ReasonPhrase}).")
                .AsAsync<Dictionary<string, Uri>>();
        }
    }
}