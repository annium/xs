using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;

namespace Xs.Registry.Dotnet.Tests.Controllers
{
    public class IndexControllerTest : IntegrationTestBase
    {
        [Fact]
        public async Task Get_ReturnsIndex()
        {
            // act
            var response = await server.Get("/v3/index.json").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
        }
    }
}