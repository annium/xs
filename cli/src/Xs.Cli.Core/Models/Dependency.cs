using System;

namespace Xs.Cli.Core.Models;

public sealed record Dependency<T>
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

    public override int GetHashCode() => HashCode.Combine(Type, Value);
}