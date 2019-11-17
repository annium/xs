using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Core.Models
{
    public class Dependency<T> : Equatable<Dependency<T>>
    {
        public DependencyType Type { get; }
        public T Value { get; }

        public Dependency(
            DependencyType type,
            T value
        )
        {
            Type = type;
            Value = value;
        }

        public void Deconstruct(
            out DependencyType type,
            out T value
        )
        {
            type = Type;
            value = Value;
        }

        public override string ToString() => Value!.ToString()!;

        public override IEnumerable<int> GetComponentHashCodes()
        {
            yield return Type.GetHashCode();
            yield return Value!.GetHashCode();
        }
    }
}