using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;
using Annium.Testing;

namespace Xs.Registry.Dotnet.Tests
{
    public class IndexControllerTest : IntegrationTest
    {
        private IRequest http => GetRequest<Startup<ServicePack>>();

        // [Fact]
        // public async Task True_IsTrue()
        // {
        //     // act
        //     var response = await http.Get("/").RunAsync();

        //     // assert
        //     response.StatusCode.IsEqual(HttpStatusCode.OK);
        // }
    }
}