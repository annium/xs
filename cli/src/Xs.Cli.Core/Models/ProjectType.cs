using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Core.Models
{
    public class ProjectType : Equatable<ProjectType>
    {
        private static readonly List<ProjectType> types = new List<ProjectType>();

        // TODO: use instead of null
        public static readonly ProjectType None = new ProjectType(string.Empty);

        public static void Register(string name)
        {
            if (types.FindIndex(e => e.name == name) < 0)
                types.Add(new ProjectType(name));
        }

        public static ProjectType Get(string name) => types.Find(e => e.name == name) ??
        throw new Exception($"Project type {name} is not registered.");

        public static IEnumerable<ProjectType> List() => types.ToArray();

        private readonly string name;

        private ProjectType(string name)
        {
            this.name = name;
        }

        public override string ToString() => name;

        public override IEnumerable<int> GetComponentHashCodes()
        {
            yield return name.GetHashCode();
        }
    }
}