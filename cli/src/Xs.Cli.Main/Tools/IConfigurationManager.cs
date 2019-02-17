using System.Threading.Tasks;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Models;

namespace Xs.Cli.Main.Tools
{
    public interface IConfigurationManager
    {
        Configuration LoadBarebone(string folder);

        Task<Configuration> Load(string folder);

        void Save(string folder, IProject[] projects, Configuration configuration);

        void Delete(string folder, IProject[] projects);
    }
}