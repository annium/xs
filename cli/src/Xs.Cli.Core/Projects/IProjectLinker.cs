using System;
using System.Collections.Generic;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public interface IProjectLinker
    {
        void PreLink(
            IEnumerable<IProject> projects,
            IReadOnlyDictionary<ProjectType, HashSet<Package>> packages,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        );

        void Link(
            IProject project,
            IEnumerable<IProject> projects,
            IEnumerable<Package> packages,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        );
    }
}