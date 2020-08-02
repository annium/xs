using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Tools
{
    public interface IConfigurationManager
    {
        Configuration Load(string folder);

        void Save(Configuration configuration, IProject[] projects);

        void Delete(string folder, IProject[] projects);
    }
}