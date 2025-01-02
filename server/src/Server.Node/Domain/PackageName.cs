using System;

namespace Server.Node.Domain;

public class PackageName
{
    public static PackageName Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            Fail();

        if (!value.StartsWith('@'))
            return new PackageName(null, value);

        var parts = value[1..].Split('/');
        if (parts.Length != 2)
            Fail();

        var (scope, name) = (parts[0].Trim(), parts[1].Trim());
        if (scope.Length == 0 || name.Length == 0)
            Fail();

        return new PackageName(scope, name);

        void Fail() => throw new ArgumentException($"'{value}' is not a valid package name.");
    }

    private readonly string? _scope;
    private readonly string _name;

    private PackageName(string? scope, string name)
    {
        _scope = scope;
        _name = name;
    }

    public override string ToString() => _scope is null ? _name : $"@{_scope}/{_name}";

    public string ToFileName() => _scope is null ? _name : $"{_scope}-{_name}";

    public static implicit operator string(PackageName name) => name.ToString();
}
