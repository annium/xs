using System;
using System.Collections.Generic;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public interface IPlatformProjectLinker
{
    ProjectType Type { get; }

    void PreLink(
        IReadOnlyCollection<IProject> projects,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    );

    void Link(
        IProject project,
        IReadOnlyCollection<IProject> projects,
        IReadOnlyCollection<Package> packages,
        Action<Exception> addError
    );
}
