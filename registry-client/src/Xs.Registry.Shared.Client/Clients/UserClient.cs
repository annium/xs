using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
using Xs.Registry.Core.Client;

namespace Xs.Registry.Shared.Client
{
    public class UserClient : ClientBase
    {
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