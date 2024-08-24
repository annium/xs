using System;
using System.Threading;
using System.Threading.Tasks;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Cli.Core.Projects;

public interface IPublishableProject : IProject
{
    Task<string> PackAsync(Version version, CancellationToken ct);

    Task PublishAsync(Uri registry, string accessToken, Version version, CancellationToken ct);
}
