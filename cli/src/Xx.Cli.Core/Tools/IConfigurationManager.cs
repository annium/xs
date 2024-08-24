using System.Collections.Generic;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Core.Tools;

public interface IConfigurationManager
{
    SolutionConfiguration Load(string folder);

    void Save(SolutionConfiguration configuration, IReadOnlyCollection<IProject> projects);

    void Delete(string folder, IReadOnlyCollection<IProject> projects);
}
