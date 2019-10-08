using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Node.Projects
{
    internal class ProjectMapper : IProjectMapper<ISpecialProject, RawProject>
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IgnoreNullValues = true,
        };

        private const string FilePrefix = "file:";

        public RawProject Load(string path, DiscoverConfiguration configuration)
        {
            var project = new RawProject();
            var file = new FileInfo(path);

            var info = JsonSerializer.Deserialize<Raw>(File.ReadAllText(file.FullName), jsonSerializerOptions);

            project.Name = info.Name ??
                throw new InvalidOperationException($"Project {path} is missing name");

            var rawVersion = info.Version ??
                throw new InvalidOperationException($"Project {path} is missing version");
            if (Core.Models.Version.TryParse(rawVersion, out var version))
                project.Version = version;
            else
                throw new InvalidOperationException($"Project {path} version {rawVersion} is invalid");

            if (configuration.SkipChecks)
                project.Description = info.Description ?? string.Empty;
            else
                project.Description = info.Description ??
                throw new InvalidOperationException($"Project {path} is missing description");

            // is not packable, if is private - when private: true is specified
            project.IsPackable = !info.Private.HasValue || !info.Private.Value;

            var projects = new List<Dependency<string>>();
            var packages = new List<Dependency<Package>>();
            if (info.Dependencies != null)
            {
                projects.AddRange(GetProjectDependencies(info.Dependencies, DependencyType.Normal));
                packages.AddRange(GetPackageDependencies(info.Dependencies, DependencyType.Normal));
            }
            if (info.DevDependencies != null)
            {
                projects.AddRange(GetProjectDependencies(info.DevDependencies, DependencyType.Dev));
                packages.AddRange(GetPackageDependencies(info.DevDependencies, DependencyType.Dev));
            }
            if (info.PeerDependencies != null)
            {
                projects.AddRange(GetProjectDependencies(info.PeerDependencies, DependencyType.Peer));
                packages.AddRange(GetPackageDependencies(info.PeerDependencies, DependencyType.Peer));
            }
            project.Projects = projects;
            project.Packages = packages;

            project.Scripts = info.Scripts ?? new Dictionary<string, string>();

            return project;

            IEnumerable<Dependency<string>> GetProjectDependencies(IReadOnlyDictionary<string, string> value, DependencyType type) =>
                value
                .Where(e => e.Value.StartsWith(FilePrefix))
                .Select(e => new Dependency<string>(type, ReadProjectDependency(project.Name, file, e.Value.Substring(FilePrefix.Length))));

            IEnumerable<Dependency<Package>> GetPackageDependencies(IReadOnlyDictionary<string, string> value, DependencyType type) =>
                value
                .Where(e => !e.Value.StartsWith(FilePrefix))
                .Select(e => new Dependency<Package>(type, ReadPackageDependency(project.Name, e.Key, e.Value)));
        }

        public void Save(ISpecialProject project)
        {
            var path = project.File;
            var dir = Directory.GetParent(path).FullName;

            var info = JsonSerializer.Deserialize<Raw>(File.ReadAllText(path), jsonSerializerOptions);

            info.Name = project.Name;
            info.Version = project.Version.ToString();
            info.Description = project.Description;
            if (project is IPublishableProject)
                info.Private = null;
            else
                info.Private = true;

            info.Dependencies = getDeps(project, DependencyType.Normal);
            info.DevDependencies = getDeps(project, DependencyType.Dev);
            info.PeerDependencies = getDeps(project, DependencyType.Peer);

            File.WriteAllText(path, JsonSerializer.Serialize(info, jsonSerializerOptions));
            File.AppendAllText(path, Environment.NewLine);

            static Dictionary<string, string> ? getDeps(ISpecialProject project, DependencyType type)
            {
                var deps = project.Projects
                    .Where(e => e.Type == type)
                    .Select(e => (name: e.Value.Name, value: FilePrefix + Path.GetRelativePath(project.Directory, e.Value.Directory)))
                    .Concat(
                        project.Packages
                        .Where(e => e.Type == type)
                        .Select(e => (name: e.Value.Name, value: e.Value.Version.ToString()))
                    )
                    .OrderBy(e => e.name)
                    .ToDictionary(e => e.name, e => e.value);

                return deps.Count > 0 ? deps : null;
            }
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
            string rawVersion
        )
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Project {project} has empty package dependency name.");

            if (string.IsNullOrWhiteSpace(rawVersion))
                throw new InvalidOperationException($"Project {project} has empty package dependency {name} version.");

            if (!Core.Models.Version.TryParse(rawVersion, out var version))
                throw new InvalidOperationException($"Project {project} package dependency {name} version {rawVersion} is invalid.");

            return new Package(Constants.ProjectType, name, version);
        }

        private class Raw
        {
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool? Private { get; set; }
            public string? Main { get; set; }
            public Dictionary<string, string> ? Dependencies { get; set; }
            public Dictionary<string, string> ? DevDependencies { get; set; }
            public Dictionary<string, string> ? PeerDependencies { get; set; }
            public Dictionary<string, string> ? Scripts { get; set; }

            [JsonPropertyName("browserslist")]
            public string[] ? BrowsersList { get; set; }
        }
    }
}
