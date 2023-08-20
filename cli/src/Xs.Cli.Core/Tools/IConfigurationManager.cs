using System.Collections.Generic;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tools;

public interface IConfigurationManager
{
    Configuration Load(string folder);

    void Save(Configuration configuration, IReadOnlyCollection<IProject> projects);

    void Delete(string folder, IReadOnlyCollection<IProject> projects);
}