using System;
using System.Collections.Generic;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;

namespace Annium.Xs.Cli.Core.Projects;

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
