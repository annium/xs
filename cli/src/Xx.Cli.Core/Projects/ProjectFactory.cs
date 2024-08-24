using System.Collections.Generic;
using System.Linq;

namespace Xx.Cli.Core.Projects;

internal class ProjectFactory : IProjectFactory
{
    private readonly IEnumerable<IPlatformProjectFactory> _factories;

    public ProjectFactory(IEnumerable<IPlatformProjectFactory> factories)
    {
        _factories = factories;
    }

    public IPlatformProjectFactory? ResolveFactory(string directory)
    {
        return _factories.FirstOrDefault(e => e.IsProjectDirectory(directory));
    }

    public bool IsProjectFile(string file)
    {
        return _factories.Any(e => e.IsProjectFile(file));
    }
}
