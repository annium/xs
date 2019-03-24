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

        [Option("-s")]
        [Help("Allows to disable normally forced checks.")]
        public bool SkipChecks { get; set; }

        [Option("-i")]
        [Help("Allows to ignore inconsistency to fix fursther.")]
        public bool IgnoreConsistency { get; set; }

        private string root = Directory.GetCurrentDirectory();
    }
}