using System;
using System.Linq;

namespace Xs.Cli.Core.Models
{
    public class Version
    {
        public uint Major { get; }

        public uint Minor { get; }

        public uint Patch { get; }

        public string Suffix { get; }

        public Version(uint major, uint minor, uint patch, string suffix)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Suffix = suffix;
        }

        public Version(string version)
        {
            if (version == null)
                throw new ArgumentNullException(version);

            var parts = version.Split('.');
            if (parts.Length != 3)
                throwException();

            try
            {
                Major = uint.Parse(parts[0]);
                Minor = uint.Parse(parts[1]);
                var patchParts = parts[2].Split('-');
                Patch = uint.Parse(patchParts[0]);
                if (patchParts.Length == 1)
                    return;

                Suffix = '-' + string.Join('-', patchParts.Skip(1));
            }
            catch
            {
                throwException();
            }

            void throwException() =>
                throw new InvalidOperationException($"Version {version} doesn't follow SemVer notation.");
        }

        public override string ToString() => $"{Major}.{Minor}.{Patch}{Suffix}";

        public override bool Equals(object obj) => GetType() == obj?.GetType() && GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 7;

                hash = hash * 31 + Major.GetHashCode();
                hash = hash * 31 + Minor.GetHashCode();
                hash = hash * 31 + Patch.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(Version v1, Version v2) => v1?.GetHashCode() == v2?.GetHashCode();

        public static bool operator !=(Version v1, Version v2) => !(v1 == v2);
    }
}