using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Dotnet.Internal.Extensions;

internal static class HttpRequestExtensions
{
    public static async Task<Stream?> GetUploadStreamOrNullAsync(
        this HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        // Try to get the nupkg from the multipart/form-data
        Stream? rawUploadStream = null;
        try
        {
            if (request is { HasFormContentType: true, Form.Files.Count: > 0 })
            {
                rawUploadStream = request.Form.Files[0].OpenReadStream();

                // Convert the upload stream into a temporary file stream to
                // minimize memory usage.
                return await rawUploadStream.AsTemporaryFileStreamAsync(cancellationToken);
            }

            return null;
        }
        finally
        {
            if (rawUploadStream is not null)
                await rawUploadStream.DisposeAsync();
        }
    }
}
