using System;
using System.Collections.Generic;
using Annium.Data.Models;

namespace Xs.Cli.Core.Models
{
    public class ProjectType : Equatable<ProjectType>
    {
        private static readonly List<ProjectType> Types = new();

        // TODO: use instead of null
        public static readonly ProjectType None = new(string.Empty);

        public static void Register(string name)
        {
            if (Types.FindIndex(e => e._name == name) < 0)
                Types.Add(new ProjectType(name));
        }

        public static ProjectType Get(string name) => Types.Find(e => e._name == name) ??
            throw new Exception($"Project type {name} is not registered.");

        public static IEnumerable<ProjectType> List() => Types.ToArray();

        private readonly string _name;

        private ProjectType(string name)
        {
            _name = name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is string str)
                return _name == str;

            return base.Equals(obj);
        }

        public override string ToString() => _name;

        public override int GetHashCode() => _name.GetHashCode();
    }
}