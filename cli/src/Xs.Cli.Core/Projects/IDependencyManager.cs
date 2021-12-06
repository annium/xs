using System;
using System.Threading.Tasks;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public interface IDependencyManager
{
    ProjectType Type { get; }
    Uri DefaultServer { get; }

    Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken);
}