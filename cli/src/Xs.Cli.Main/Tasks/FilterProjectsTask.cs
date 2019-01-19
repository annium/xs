using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks
{
    public class FilterProjectsTask
    {
        private readonly ILogger logger;

        public FilterProjectsTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public IEnumerable<IProject> Run(IEnumerable<IProject> projects, string mask)
        {
            if (mask == "*")
                return projects;

            var pattern = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase);

            var result = projects.Where(p => regex.IsMatch(p.Name)).OrderBy(p => p.Name).ToArray();

            logger.LogDebug($"Mask {regex} filtered {projects.Count()} projects to {result.Length} projects");

            return result;
        }
    }
}