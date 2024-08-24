using System;
using System.Collections.Generic;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public interface IProjectLinker
{
    void PreLink(
        IReadOnlyCollection<IProject> projects,
        IReadOnlyDictionary<ProjectType, HashSet<Package>> packages,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    );

    void Link(
        IProject project,
        IReadOnlyCollection<IProject> projects,
        IReadOnlyCollection<Package> packages,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    );
}
