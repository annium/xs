using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Node.Projects
{
    internal class RawProject
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public IEnumerable<Dependency<string>> Projects { get; set; }
        public IEnumerable<Dependency<Package>> Packages { get; set; }
        public IReadOnlyDictionary<string, string> Scripts { get; set; }
        public bool IsPackable { get; set; }

        public void Deconstruct(
            out string name,
            out Version version,
            out string description,
            out IEnumerable<Dependency<string>> projects,
            out IEnumerable<Dependency<Package>> packages,
            out IReadOnlyDictionary<string, string> scripts,
            out bool isPackable
        )
        {
            name = Name;
            version = Version;
            description = Description;
            projects = Projects;
            packages = Packages;
            scripts = Scripts;
            isPackable = IsPackable;
        }
    }
}