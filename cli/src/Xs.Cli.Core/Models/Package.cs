using Annium.Data.Models;

namespace Xs.Cli.Core.Models
{
    public class Package : Equatable<Package>, IReference
    {
        public ProjectType Type { get; }
        public string Name { get; }
        public Version Version { get; }

        public Package(
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

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 7;

                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + Name.GetHashCode();
                hash = hash * 31 + Version.GetHashCode();

                return hash;
            }
        }
    }
}