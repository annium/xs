using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;

namespace Xs.Registry.Node.Tests.Controllers
{
    public class PackagesControllerTest : IntegrationTestBase
    {
        [Fact]
        public async Task Get_MissingPackage_ReturnsNotFound()
        {
            // act
            var response = await main.Get("/packages/fake").RunAsync();

            // assert
            // TODO: apply authorization
            response.StatusCode.IsEqual(HttpStatusCode.Unauthorized);
        }
    }
}