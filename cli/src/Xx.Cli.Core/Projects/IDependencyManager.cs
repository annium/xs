using System;
using System.Threading.Tasks;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public interface IDependencyManager
{
    ProjectType Type { get; }
    Uri DefaultServer { get; }

    Task<Package[]> ResolveVersionsAsync(Package package, Uri serverUri, string accessToken);
}
