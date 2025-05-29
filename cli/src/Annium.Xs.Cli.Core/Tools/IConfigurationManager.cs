using System.Collections.Generic;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Core.Tools;

public interface IConfigurationManager
{
    SolutionConfiguration Load(string folder);

    void Save(SolutionConfiguration configuration, IReadOnlyCollection<IProject> projects);

    void Delete(string folder, IReadOnlyCollection<IProject> projects);
}
