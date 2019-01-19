using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;

namespace Xs.Registry.Shared.Client
{
    public class SharedClient
    {
        public UserClient User { get; }

        private Uri uri;

        public SharedClient(
            UserClient userClient
        )
        {
            this.User = userClient;
        }

        internal void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned");

            User.SetUri(uri);
            this.uri = uri;
        }

        public Task<Dictionary<string, Uri>> GetRegistryInfoAsync(string token)
        {
            return Http.Open(this.uri)
                .Get("registry")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Registry info fetch failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsAsync<Dictionary<string, Uri>>();
        }
    }
}