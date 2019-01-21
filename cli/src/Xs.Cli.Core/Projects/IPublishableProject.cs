using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Projects
{
    public interface IPublishableProject : IProject
    {
        Task<string> PackAsync(Models.Version version, CancellationToken token);

        Task PublishAsync(Uri registry, string accessToken, Models.Version version, CancellationToken token);

        Task UnpublishAsync(Uri registry, string accessToken, Models.Version version, CancellationToken token);
    }
}