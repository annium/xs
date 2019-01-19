using System;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;

namespace Xs.Registry.Shared.Client
{
    public class UserClient
    {
        private Uri uri;

        internal void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned");

            this.uri = uri;
        }

        public Task<string> CreateAsync(string name, string password)
        {
            return Http.Open(this.uri)
                .Put("user")
                .JsonContent(new { name, password })
                .EnsureSuccessStatusCode(response => $"User create failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsStringAsync();
        }

        public Task<string> UpdateAsync(string token, string newPassword)
        {
            return Http.Open(this.uri)
                .Post("user")
                .BearerAuthorization(token)
                .JsonContent(new { newPassword })
                .EnsureSuccessStatusCode(response => $"User update failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsStringAsync();
        }

        public Task<string> LoginAsync(string name, string password)
        {
            return Http.Open(this.uri)
                .Get("user")
                .Param("name", name)
                .Param("password", password)
                .EnsureSuccessStatusCode(response => $"User login failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsStringAsync();
        }

        public Task DeleteAsync(string token)
        {
            return Http.Open(this.uri)
                .Get("user")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"User delete failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsStringAsync();
        }
    }
}