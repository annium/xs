using System.IO;
using Annium.Extensions.Arguments;

namespace Xs.Cli.Core.Commands
{
    public class DiscoverConfiguration
    {
        [Option("cwd")]
        [Help("Allows to run command in specific folder.")]
        public string Root
        {
            get
            {
                return root;
            }
            set
            {
                root = Path.GetFullPath(value);
            }
        }

        [Option]
        [Help("Allows to disable useless checks.")]
        public bool SkipChecks { get; set; }

        private string root = Directory.GetCurrentDirectory();
    }
}