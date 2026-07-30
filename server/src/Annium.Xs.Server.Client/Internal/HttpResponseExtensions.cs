using System;
using Annium.Net.Http;

namespace Annium.Xs.Server.Client.Internal;

/// <summary>
/// Failure handling shared by the typed clients.
/// </summary>
internal static class HttpResponseExtensions
{
    /// <summary>
    /// Throws when the response is a failure, naming the action that failed.
    /// </summary>
    /// <param name="response">The response to check</param>
    /// <param name="action">Human-readable name of the attempted action, used to build the message</param>
    public static void EnsureSuccess(this IHttpResponse response, string action)
    {
        if (response.IsFailure)
            throw new Exception($"{action} failed with {response.StatusCode} ({response.StatusText}).");
    }
}
