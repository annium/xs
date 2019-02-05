using System;
using System.Collections.Generic;

namespace Xs.Registry.Db.Shared
{
    public class ProjectType
    {
        private static List<ProjectType> types = new List<ProjectType>();

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

        public override int GetHashCode() => name.GetHashCode();
    }
}