namespace Xs.Cli.Core.Models
{
    public class Dependency
    {
        public ProjectType Type { get; }

        public string Name { get; }

        public Version Version { get; }

        public Dependency(
            ProjectType type,
            string name,
            Version version
        )
        {
            Type = type;
            Name = name;
            Version = version;
        }

        public void Deconstruct(
            out ProjectType type,
            out string name,
            out Version version
        )
        {
            type = Type;
            name = Name;
            version = Version;
        }

        public override string ToString() => $"{Name} {Version}";

        public override bool Equals(object obj) => GetType() == obj.GetType() && GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
        {
            var hash = 7;

            hash = hash * 31 + Type.GetHashCode();
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + Version.GetHashCode();

            return hash;
        }
    }
}