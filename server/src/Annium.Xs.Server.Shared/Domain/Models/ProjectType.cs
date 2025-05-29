using System;
using System.Collections.Generic;

namespace Annium.Xs.Server.Shared.Domain.Models;

public sealed record ProjectType
{
    private static readonly List<ProjectType> _types = new();

    public static ProjectType Register(string name)
    {
        if (_types.FindIndex(e => e._name == name) < 0)
            _types.Add(new ProjectType(name));

        return Get(name);
    }

    public static ProjectType Get(string name) =>
        _types.Find(e => e._name == name) ?? throw new Exception($"Project type {name} is not registered.");

    private readonly string _name;

    private ProjectType(string name)
    {
        _name = name;
    }

    public override string ToString() => _name;

    public override int GetHashCode() => _name.GetHashCode();
}
