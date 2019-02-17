using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Node.Projects
{
    internal class ProjectMapper
    {
        public RawProject Load(string path)
        {
            var project = new RawProject();
            var file = new FileInfo(path);

            var info = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file.FullName));

            project.Name = info.Property(El.Name)?.Value.ToString() ??
                throw new InvalidOperationException($"Project is misssing name");
            project.Version = info.Property(El.Version) == null ? null :
                new Core.Models.Version(info.Property(El.Version).Value.ToString());

            var deps = GetPropertyDictionary(info, El.Dependencies)
                .Concat(GetPropertyDictionary(info, El.DevDependencies))
                .ToDictionary(e => e.Key, e => e.Value);

            project.ProjectDependencies = deps
                .Where(e => e.Value.StartsWith(El.FilePrefix))
                .Select(e => ReadProjectDependency(project.Name, file, e.Value.Substring(El.FilePrefix.Length)))
                .ToArray();

            project.PackageDependencies = deps
                .Where(e => !e.Value.StartsWith(El.FilePrefix))
                .Select(e => ReadPackageDependency(project.Name, e.Key, e.Value))
                .ToArray();

            project.Scripts = GetPropertyDictionary(info, El.Scripts);

            return project;
        }

        public void Save(ISpecialProject project)
        {
            var path = project.File.FullName;
            var dir = Directory.GetParent(path).FullName;

            var info = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));

            var currentDeps = GetPropertyDictionary(info, El.Dependencies);
            var currentDevDeps = GetPropertyDictionary(info, El.DevDependencies);

            var projectDeps = project.ProjectDependencies
                .Where(e => !currentDevDeps.ContainsKey(e.Name))
                .OrderBy(e => e.Name)
                .ToDictionary(e => e.Name, e => El.FilePrefix + Path.GetRelativePath(dir, e.File.DirectoryName));

            var projectDevDeps = project.ProjectDependencies
                .Where(e => currentDevDeps.ContainsKey(e.Name))
                .OrderBy(e => e.Name)
                .ToDictionary(e => e.Name, e => El.FilePrefix + Path.GetRelativePath(dir, e.File.DirectoryName));

            var packageDeps = project.PackageDependencies
                .Where(e => !currentDevDeps.ContainsKey(e.Name))
                .OrderBy(e => e.Name)
                .ToDictionary(e => e.Name, e => e.Version.ToString());

            var packageDevDeps = project.PackageDependencies
                .Where(e => currentDevDeps.ContainsKey(e.Name))
                .OrderBy(e => e.Name)
                .ToDictionary(e => e.Name, e => e.Version.ToString());

            var deps = projectDeps.Concat(packageDeps).ToDictionary(e => e.Key, e => e.Value);
            var devDeps = projectDevDeps.Concat(packageDevDeps).ToDictionary(e => e.Key, e => e.Value);

            if (project.Version == null)
                info.Property(El.Version)?.Remove();
            else
                info[El.Version] = project.Version.ToString();

            if (deps.Count > 0)
                info[El.Dependencies] = JObject.FromObject(deps);
            else
                info.Remove(El.Dependencies);

            if (devDeps.Count > 0)
                info[El.DevDependencies] = JObject.FromObject(devDeps);
            else
                info.Remove(El.DevDependencies);

            File.WriteAllText(path, JsonConvert.SerializeObject(info, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
            }));
        }

        private string ReadProjectDependency(
            string project,
            FileInfo file,
            string location
        )
        {
            var path = Path.Combine(file.DirectoryName, location, ProjectFactory.ProjectFileName);
            if (!File.Exists(path))
                throw new InvalidOperationException($"Project {project} has broken project dependency {location}.");

            return path;
        }

        private static Dependency ReadPackageDependency(
            string project,
            string name,
            string version
        )
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Project {project} has empty package dependency name.");

            if (string.IsNullOrWhiteSpace(version))
                throw new InvalidOperationException($"Project {project} has empty package dependency {name} version.");

            return new Dependency(Constants.ProjectType, name, new Core.Models.Version(version));
        }

        private IReadOnlyDictionary<string, string> GetPropertyDictionary(JObject raw, string propertyName) => raw
            .Property(propertyName) ?
            .Value
            .ToObject<Dictionary<string, string>>() ??
            new Dictionary<string, string>();

        private static class El
        {
            public const string Name = "name";

            public const string Version = "version";

            public const string Dependencies = "dependencies";

            public const string DevDependencies = "devDependencies";

            public const string FilePrefix = "file:";

            public const string Scripts = "scripts";
        }
    }
}