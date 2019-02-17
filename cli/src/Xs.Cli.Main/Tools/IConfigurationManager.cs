using System.Threading.Tasks;
using Xs.Cli.Main.Models;

namespace Xs.Cli.Main.Tools
{
    public interface IConfigurationManager
    {
        Task<Configuration> Load(string folder);

        void Save(string folder, Configuration configuration);
    }
}