using System;

namespace Xs.Registry.Node.Models
{
    public class PackageName
    {
        public static PackageName Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) fail();

            if (!value.StartsWith('@'))
                return new PackageName(null, value);

            var parts = value.Substring(1).Split('/');
            if (parts.Length != 2) fail();

            var(scope, name) = (parts[0].Trim(), parts[1].Trim());
            if (scope.Length == 0 || name.Length == 0) fail();

            return new PackageName(scope, name);

            void fail() =>
                throw new ArgumentException($"'{value}' is not a valid package name");
        }

        public string Scope { get; }

        public string Name { get; }

        private PackageName(
            string scope,
            string name
        )
        {
            Scope = scope;
            Name = name;
        }

        public override string ToString() => Scope == null ? Name : $"@{Scope}/{Name}";

        public string ToFileName() => Scope == null ? Name : $"{Scope}-{Name}";

        public static implicit operator string(PackageName name) => name.ToString();
    }
}