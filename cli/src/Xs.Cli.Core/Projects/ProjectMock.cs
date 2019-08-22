using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public class ProjectMock<TProject> : IProject where TProject : class, IProject
    {
        public ProjectType Type { get; } = Constants.MockProjectType;
        public string Name { get; private set; }
        public Models.Version Version { get; private set; }
        public string Description { get; private set; }
        public string Directory { get; private set; }
        public string File { get; private set; }
        public HashSet<Dependency<IProject>> Projects => new HashSet<Dependency<IProject>>();
        public HashSet<Dependency<Package>> Packages => new HashSet<Dependency<Package>>();

        public ProjectMock(
            string name,
            Models.Version version,
            string description,
            string directory,
            string file
        )
        {
            Name = name;
            Version = version;
            Description = description;
            Directory = directory;
            File = file;
        }

        public bool IsRelated(string path) =>
            throw new NotImplementedException();

        public void Save() =>
            throw new NotImplementedException();

        public void SetDirectory(string directory) =>
            throw new NotImplementedException();

        public void SetName(string name) =>
            throw new NotImplementedException();

        public void SetVersion(Models.Version version) =>
            throw new NotImplementedException();
    }
}