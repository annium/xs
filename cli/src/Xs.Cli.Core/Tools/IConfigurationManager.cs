using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Tools
{
    public interface IConfigurationManager
    {
        Configuration Load(string folder);

        void Save(string folder, Configuration configuration);
    }
}