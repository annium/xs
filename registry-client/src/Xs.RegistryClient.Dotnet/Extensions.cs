using Annium.Extensions.Net.Http;

namespace Xs.RegistryClient.Dotnet
{
    internal static class Extensions
    {
        public static IRequest NuGetAuthorization(this IRequest request, string token) =>
            request.Header("X-NuGet-ApiKey", token);
    }
}