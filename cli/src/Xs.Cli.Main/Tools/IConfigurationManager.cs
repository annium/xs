using System.Threading.Tasks;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tools
{
    public interface IConfigurationManager
    {
        Configuration LoadBarebone(string folder);

        Task<Configuration> LoadAsync(string folder);

        void Save(string folder, IProject[] projects, Configuration configuration);

        void Delete(string folder, IProject[] projects);
    }
}