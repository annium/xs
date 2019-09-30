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

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 7;

                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + Value!.GetHashCode();

                return hash;
            }
        }
    }
}