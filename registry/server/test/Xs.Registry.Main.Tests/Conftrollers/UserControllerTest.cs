using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;
using Xs.Registry.Main.Payloads;

namespace Xs.Registry.Main.Tests.Conftrollers
{
    public class UserControllerTest : IntegrationTestBase
    {
        [Fact]
        public async Task CreateUser_BadRequest_ReturnsBadRequest()
        {
            // arrange
            var p = new UserRegistrationPayload { Name = "user" };

            // act
            var response = await Main.Put("/user").JsonContent(p).RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.BadRequest);
        }
    }
}