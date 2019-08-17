using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;

namespace Xs.Registry.Main.Tests.Controllers
{
    public class RegistryControllerTest : IntegrationTestBase
    {
        [Fact]
        public async Task Get_ReturnsConfiguration()
        {
            // act
            var response = await main.Get("/registry").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
        }
    }
}