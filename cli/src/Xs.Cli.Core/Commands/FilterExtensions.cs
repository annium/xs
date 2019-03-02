using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Commands
{
    public static class FilterExtensions
    {
        public static IEnumerable<IProject> FilterMask(this IEnumerable<IProject> projects, string mask)
        {
            if (mask == "all")
                return projects;

            var pattern = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase);

            return projects.Where(p => regex.IsMatch(p.Name));
        }

        public static IEnumerable<IProject> FilterType(this IEnumerable<IProject> projects, ProjectType type)
        {
            if (type == null)
                return projects;

            return projects.Where(e => e.Type == type);
        }
    }
}