using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Commands
{
    public static class FilterExtensions
    {
        public static IEnumerable<T> FilterMask<T>(this IEnumerable<T> references, string mask)
        where T : IReference
        {
            if (mask == "all")
                return references;

            var pattern = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase);

            return references.Where(p => regex.IsMatch(p.Name));
        }

        public static IEnumerable<T> FilterType<T>(this IEnumerable<T> projects, ProjectType type)
        where T : IReference
        {
            if (type == null)
                return projects;

            return projects.Where(e => e.Type == type);
        }
    }
}