using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Commands;

public static class FilterExtensions
{
    public static IEnumerable<Dependency<T>> FilterMask<T>(this IEnumerable<Dependency<T>> references, string mask) where T : IReference =>
        references.FilterMask(p => p.Value.Name, mask);

    public static IEnumerable<T> FilterMask<T>(this IEnumerable<T> references, string mask) where T : IReference =>
        references.FilterMask(e => e.Name, mask);

    private static IEnumerable<T> FilterMask<T>(this IEnumerable<T> references, Func<T, string> getName, string mask)
    {
        if (mask == "all")
            return references;

        var list = references.ToList();
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        var exactMatch = list.FirstOrDefault(i => getName(i).Equals(mask, comparison));
        if (exactMatch != null && !exactMatch.Equals(default(T) !))
            return new[] { exactMatch };

        return list.Where(p => getName(p).Contains(mask, comparison));
    }

    public static IEnumerable<T> FilterType<T>(this IEnumerable<T> projects, ProjectType type)
        where T : IReference
    {
        if (type == ProjectType.None)
            return projects;

        return projects.Where(e => e.Type == type);
    }
}