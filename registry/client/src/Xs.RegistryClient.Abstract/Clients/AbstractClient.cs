using System.Threading.Tasks;
using System.Web;
using Annium.Extensions.Net.Http;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Abstract
{
    public class AbstractClient : ClientBase
    {
        public AbstractClient()
        {

        }

        public Task DeletePackageAsync(string token, string name, string version)
        {
            return Http.Open(this.uri)
                .Delete($"{HttpUtility.UrlEncode(name)}/{version}")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Delete package failed with {response.StatusCode} ({response.ReasonPhrase}).")
                .RunAsync();
        }
    }
}