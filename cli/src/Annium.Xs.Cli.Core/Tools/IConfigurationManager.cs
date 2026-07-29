using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Core.Tools;

public interface IConfigurationManager
{
    Task<SolutionConfiguration> LoadAsync(string folder, CancellationToken ct = default);

    void Save(SolutionConfiguration configuration, IReadOnlyCollection<IProject> projects);

    void Delete(string folder, IReadOnlyCollection<IProject> projects);
}
