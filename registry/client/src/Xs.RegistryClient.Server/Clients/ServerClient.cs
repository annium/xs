using System.Threading.Tasks;
using System.Web;
using Annium.Net.Http;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Server
{
    public class ServerClient : ClientBase
    {
        public ServerClient()
        {

        }

        public Task DeletePackageAsync(string token, string name, string version)
        {
            return Http.Open(uri)
                .Delete($"packages/{HttpUtility.UrlEncode(name)}/{version}")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Delete package failed with {response.StatusCode} ({response.StatusText}).")
                .RunAsync();
        }
    }
}