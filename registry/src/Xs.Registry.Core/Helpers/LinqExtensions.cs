using System.Collections.Generic;
using System.Linq;

namespace Xs.Registry.Core.Helpers
{
    public static class LinqExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source
        ) => source.ToDictionary(e => e.Key, e => e.Value);

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source
        ) => source.ToDictionary();
    }
}