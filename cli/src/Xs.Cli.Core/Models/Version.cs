using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Core.Models
{
    public class Version : Comparable<Version>
    {
        public uint Major { get; private set; }
        public uint Minor { get; private set; }
        public uint Patch { get; private set; }
        public string Suffix { get; private set; }

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

            var parts = version.Split('.', 3);
            if (parts.Length < 3)
                throwException();

            try
            {
                Major = uint.Parse(parts[0]);
                Minor = uint.Parse(parts[1]);

                var hasHash = parts[2].Contains('+');
                var patchParts = parts[2].Split('.', '-', '+');
                Patch = uint.Parse(patchParts[0]);

                // drop scm hash
                if (patchParts.Length == 1 || hasHash)
                    return;

                Suffix = parts[2].Substring(patchParts[0].Length);
            }
            catch
            {
                throwException();
            }

            void throwException() =>
                throw new InvalidOperationException($"Version {version} doesn't follow SemVer notation.");
        }

        public void Update(Version version)
        {
            Major = version.Major;
            Minor = version.Minor;
            Patch = version.Patch;
            Suffix = version.Suffix;
        }

        public override string ToString() => $"{Major}.{Minor}.{Patch}{Suffix}";

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 7;

                hash = hash * 31 + Major.GetHashCode();
                hash = hash * 31 + Minor.GetHashCode();
                hash = hash * 31 + Patch.GetHashCode();
                hash = hash * 31 + (Suffix?.GetHashCode() ?? 0);

                return hash;
            }
        }

        protected override IEnumerable<Func<Version, IComparable>> GetComparables()
        {
            yield return x => x.Major;
            yield return x => x.Minor;
            yield return x => x.Patch;
            yield return x => x.Suffix;
        }
    }
}