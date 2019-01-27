using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
using Xs.Registry.Core.Client;

namespace Xs.Registry.Shared.Client
{
    public class SharedClient : ClientBase
    {
        public PermissionsClient Permissions { get; }

        public UserClient User { get; }

        public SharedClient(
            PermissionsClient permissionsClient,
            UserClient userClient
        ) : base(
            permissionsClient,
            userClient
        )
        {
            Permissions = permissionsClient;
            User = userClient;
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