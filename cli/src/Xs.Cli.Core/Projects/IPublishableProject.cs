using System;
using System.Threading;
using System.Threading.Tasks;
using Version = Xs.Cli.Core.Models.Version;

namespace Xs.Cli.Core.Projects;

public interface IPublishableProject : IProject
{
    Task<string> PackAsync(Version version, CancellationToken ct);

    Task PublishAsync(Uri registry, string accessToken, Version version, CancellationToken ct);
}
