using System.Linq;
using System.IO;
using Annium.Extensions.Arguments;

namespace Xs.Cli.Core.Commands
{
    public class DiscoverConfiguration
    {
        [Option("cwd")]
        [Help("Allows to run command in specific folder.")]
        public string[] Roots
        {
            get
            {
                return roots;
            }
            set
            {
                roots = value.Select(Path.GetFullPath).ToArray();
            }
        }

        public string Root => Roots.First();

        [Option("-sc")]
        [Help("Allows to disable normally forced checks.")]
        public bool SkipChecks { get; set; }

        [Option("-fc")]
        [Help("Force string checks.")]
        public bool ForceChecks { get; set; }

        [Option("-ic")]
        [Help("Allows to ignore inconsistency to fix fursther.")]
        public bool IgnoreConsistency { get; set; }

        private string[] roots = new string[] { Directory.GetCurrentDirectory() };
    }
}