using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Linq;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Commands;

public static class FilterExtensions
{
    public static IEnumerable<Dependency<T>> FilterMask<T>(this IEnumerable<Dependency<T>> references, string mask)
        where T : IReference => references.FilterMask(p => p.Value.Name, mask);

    public static IEnumerable<T> FilterMask<T>(this IEnumerable<T> references, string mask)
        where T : IReference => references.FilterMask(e => e.Name, mask);

    private static IEnumerable<T> FilterMask<T>(this IEnumerable<T> references, Func<T, string> getName, string mask)
    {
        if (mask == "all")
            return references;

        var list = references.ToList();
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        var exactMatch = list.FirstOrDefault(i => getName(i).Equals(mask, comparison));
        if (exactMatch is not null && !exactMatch.Equals(default(T)!))
            return new[] { exactMatch };

        var masks = mask.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var positive = masks.Where(x => !x.StartsWith('-')).ToArray();
        var negative = masks.Where(x => x.StartsWith('-')).Select(x => x[1..]).ToArray();

        return list.Where(x =>
        {
            var name = getName(x);

            return positive.Any(m => name.Contains(m, comparison)) && negative.None(m => name.Contains(m, comparison));
        });
    }

    public static IEnumerable<T> FilterType<T>(this IEnumerable<T> projects, ProjectType type)
        where T : IReference
    {
        if (type == ProjectType.None)
            return projects;

        return projects.Where(e => e.Type == type);
    }
}
