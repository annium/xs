using System;
using System.Collections.Generic;
using System.Linq;
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

            var list = references.ToList();
            var comparison = StringComparison.CurrentCultureIgnoreCase;

            var exactMatch = list.First(i => i.Name.Equals(mask, comparison));
            if (!exactMatch.Equals(default(T)))
                return new [] { exactMatch };

            return list.Where(p => p.Name.Contains(mask, comparison));
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