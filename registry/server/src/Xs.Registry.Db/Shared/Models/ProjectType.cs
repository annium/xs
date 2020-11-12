using System;
using System.Collections.Generic;

namespace Xs.Registry.Db.Shared
{
    public class ProjectType
    {
        private static List<ProjectType> _Types = new List<ProjectType>();

        public static void Register(string name)
        {
            if (_Types.FindIndex(e => e._name == name) < 0)
                _Types.Add(new ProjectType(name));
        }

        public static ProjectType Get(string name) => _Types.Find(e => e._name == name) ??
            throw new Exception($"Project type {name} is not registered.");

        public static IEnumerable<ProjectType> List() => _Types.ToArray();

        private readonly string _name;

        private ProjectType(string name)
        {
            _name = name;
        }

        public override string ToString() => _name;

        public override int GetHashCode() => _name.GetHashCode();
    }
}