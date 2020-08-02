using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Tools
{
    public interface IConfigurationManager
    {
        Configuration Load(string folder);

        void Save(string folder, IProject[] projects, Configuration configuration);

        void Delete(string folder, IProject[] projects);
    }
}