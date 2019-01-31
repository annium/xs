using System.IO;
using Annium.Extensions.Arguments;

namespace Xs.Cli.Main.Commands
{
    internal class CwdCommandConfiguration
    {
        [Option]
        [Help("Allows to run command in specific folder.")]
        public string Cwd
        {
            get
            {
                return cwd;
            }
            set
            {
                cwd = Path.GetFullPath(value);
            }
        }

        private string cwd = Directory.GetCurrentDirectory();
    }
}