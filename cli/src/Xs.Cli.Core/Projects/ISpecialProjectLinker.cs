using System;
using System.Collections.Generic;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public interface ISpecialProjectLinker
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
        DiscoverConfiguration configuration,
        Action<Exception> addError
    );
}