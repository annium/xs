using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class ProjectMapper : IProjectMapper<ISpecialProject, RawProject>
    {
        public RawProject Load(string path, DiscoverConfiguration configuration)
        {
            var project = new RawProject();
            var file = new FileInfo(path);

            var info = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file.FullName));

            project.Name = info.Property(El.Name)?.Value.ToString() ??
                throw new InvalidOperationException($"Project {path} is missing name");
            project.Version = new Core.Models.Version(
                info.Property(El.Version)?.Value.ToString() ??
                throw new InvalidOperationException($"Project {path} is missing version")
            );

            if (configuration.SkipChecks)
                project.Description = info.Property(El.Description)?.Value.ToString() ?? string.Empty;
            else
                project.Description = info.Property(El.Description)?.Value.ToString() ??
                throw new InvalidOperationException($"Project {path} is missing description");

            project.IsPackable = info.Property(El.Private)?.Value.ToString().ToLowerInvariant() == "true";

            var projects = new List<Dependency<string>>();
            projects.AddRange(GetProjectDependencies(El.Dependencies, DependencyType.Normal));
            projects.AddRange(GetProjectDependencies(El.DevDependencies, DependencyType.Dev));
            projects.AddRange(GetProjectDependencies(El.PeerDependencies, DependencyType.Peer));
            project.Projects = projects;

            var packages = new List<Dependency<Package>>();
            packages.AddRange(GetPackageDependencies(El.Dependencies, DependencyType.Normal));
            packages.AddRange(GetPackageDependencies(El.DevDependencies, DependencyType.Dev));
            packages.AddRange(GetPackageDependencies(El.PeerDependencies, DependencyType.Peer));
            project.Packages = packages;

            project.Scripts = getPropertyDictionary(info, El.Scripts);

            return project;

            IReadOnlyDictionary<string, string> getPropertyDictionary(JObject raw, string propertyName) =>
                raw.Property(propertyName)?.Value.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            IEnumerable<Dependency<string>> GetProjectDependencies(string el, DependencyType type) =>
                getPropertyDictionary(info, el)
                .Where(e => e.Value.StartsWith(El.FilePrefix))
                .Select(e => new Dependency<string>(type, ReadProjectDependency(project.Name, file, e.Value.Substring(El.FilePrefix.Length))));

            IEnumerable<Dependency<Package>> GetPackageDependencies(string el, DependencyType type) =>
                getPropertyDictionary(info, el)
                .Where(e => !e.Value.StartsWith(El.FilePrefix))
                .Select(e => new Dependency<Package>(type, ReadPackageDependency(project.Name, e.Key, e.Value)));
        }

        public void Save(ISpecialProject project)
        {
            var path = project.File;
            var dir = Directory.GetParent(path).FullName;

            var info = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
            var scripts = info[El.Scripts];
            var browsersList = info[El.BrowsersList];

            info.Remove(El.Dependencies);
            info.Remove(El.DevDependencies);
            info.Remove(El.PeerDependencies);
            info.Remove(El.Scripts);
            info.Remove(El.BrowsersList);

            var normalDeps = getDeps(DependencyType.Normal);
            var devDeps = getDeps(DependencyType.Dev);
            var peerDeps = getDeps(DependencyType.Peer);

            info[El.Version] = project.Version.ToString();

            if (normalDeps.Count > 0)
                info.Add(El.Dependencies, JObject.FromObject(normalDeps));

            if (devDeps.Count > 0)
                info.Add(El.DevDependencies, JObject.FromObject(devDeps));

            if (peerDeps.Count > 0)
                info.Add(El.PeerDependencies, JObject.FromObject(peerDeps));

            if (scripts != null)
                info.Add(El.Scripts, scripts);
            if (browsersList != null)
                info.Add(El.BrowsersList, browsersList);

            File.WriteAllText(path, JsonConvert.SerializeObject(info, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
            }));
            File.AppendAllText(path, Environment.NewLine);

            Dictionary<string, string> getDeps(DependencyType type) =>
                project.Projects
                .Where(e => e.Type == type)
                .Select(e => (name: e.Value.Name, value: El.FilePrefix + Path.GetRelativePath(dir, e.Value.Directory)))
                .Concat(
                    project.Packages
                    .Where(e => e.Type == type)
                    .Select(e => (name: e.Value.Name, value: e.Value.Version.ToString()))
                )
                .OrderBy(e => e.name)
                .ToDictionary(e => e.name, e => e.value);
        }

        private string ReadProjectDependency(
            string project,
            FileInfo file,
            string location
        )
        {
            var path = Path.GetFullPath(Path.Combine(file.DirectoryName, location, ProjectFactory.ProjectFileName));
            if (!File.Exists(path))
                throw new InvalidOperationException($"Project {project} has broken project dependency {location}.");

            return path;
        }

        private static Package ReadPackageDependency(
            string project,
            string name,
            string version
        )
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Project {project} has empty package dependency name.");

            if (string.IsNullOrWhiteSpace(version))
                throw new InvalidOperationException($"Project {project} has empty package dependency {name} version.");

            return new Package(Constants.ProjectType, name, new Core.Models.Version(version));
        }

        private static class El
        {
            public const string Name = "name";
            public const string Version = "version";
            public const string Description = "description";
            public const string Private = "private";
            public const string Dependencies = "dependencies";
            public const string DevDependencies = "devDependencies";
            public const string PeerDependencies = "peerDependencies";
            public const string FilePrefix = "file:";
            public const string Scripts = "scripts";
            public const string BrowsersList = "browserslist";
        }
    }
}