using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectMapper : IProjectMapper<ISpecialProject, RawProject>
    {
        private static readonly string[] implicitPackages = new [] { "Microsoft.AspNetCore.App" };
        private static readonly string[] booleanStrings = new [] { "true", "false" };

        public RawProject Load(string path, DiscoverConfiguration configuration)
        {
            var project = new RawProject();
            var file = new FileInfo(path);

            var info = XElement.Load(file.OpenRead());

            var properties = info.Element(El.PropertyGroup);
            if (properties is null)
                throw new InvalidOperationException($"Project {path} has no properties defined.");
            if (!configuration.SkipChecks)
                ValidateProperties(path, properties);

            project.Name = Path.GetFileNameWithoutExtension(file.Name);
            if (configuration.SkipChecks)
            {
                var rawVersion = properties.Element(El.PackageVersion)?.Value ?? "0.1.0";
                if (!Core.Models.Version.TryParse(rawVersion, out var version))
                    throw new ArgumentException($"Project {project.Name} version {rawVersion} is invalid");

                project.Version = version;
                project.Description = properties.Element(El.Description)?.Value ?? string.Empty;
            }
            else
            {
                var rawVersion = properties.Element(El.PackageVersion)?.Value;
                if (!Core.Models.Version.TryParse(rawVersion, out var version))
                    throw new ArgumentException($"Project {project.Name} version {rawVersion} is invalid");

                project.Version = version;
                project.Description = properties.Element(El.Description).Value;
            }
            project.TargetFramework = properties.Element(El.TargetFramework).Value;
            if (configuration.SkipChecks)
                project.OutputType = properties.Element(El.OutputType)?.Value == "Exe" ? OutputType.Exe : OutputType.Library;
            else
                project.OutputType = properties.Element(El.OutputType).Value == "Exe" ? OutputType.Exe : OutputType.Library;

            project.Projects = GetReferenceElements(El.ProjectReference)
                .Select(reference => ReadProjectDependency(project.Name, file, reference, configuration))
                .Select(reference => new Dependency<string>(DependencyType.Normal, reference))
                .ToArray();

            project.Packages = GetReferenceElements(El.PackageReference)
                .Select(reference => ReadPackageDependency(project.Name, reference, configuration))
                .Select(package => new Dependency<Package>(DependencyType.Normal, package))
                .ToArray();

            project.IsPackable = properties.Element(El.IsPackable) is null ?
                false :
                bool.Parse(properties.Element(El.IsPackable).Value);

            project.IsTestProject = properties.Element(El.IsTestProject) is null ?
                false :
                bool.Parse(properties.Element(El.IsTestProject).Value);

            return project;

            IEnumerable<XElement> GetReferenceElements(string referenceType) => info
                .Elements(El.ItemGroup)
                .SelectMany(group => group.Elements(referenceType));
        }

        public void Save(ISpecialProject project)
        {
            var path = project.File;
            var dir = Directory.GetParent(path).FullName;

            var info = XElement.Parse(File.ReadAllText(path));

            var oldProps = info.Element(El.PropertyGroup);
            var newProps = new XElement(El.PropertyGroup);
            oldProps.AddBeforeSelf(newProps);
            oldProps.Remove();

            newProps.Add(new XElement(El.PackageId, project.Name));
            newProps.Add(new XElement(El.PackageVersion, project.Version));
            newProps.Add(new XElement(El.Description, project.Description));
            newProps.Add(new XElement(El.TargetFramework, project.TargetFramework));
            newProps.Add(new XElement(El.OutputType, project.OutputType));
            newProps.Add(new XElement(El.DebugType, "portable"));
            newProps.Add(new XElement(El.LangVersion, "latest"));
            newProps.Add(new XElement(El.WarningsAsErrors, "true"));
            newProps.Add(new XElement(El.IsPackable, project is IPublishableProject? "true": "false"));
            if (project is TestProject)
                newProps.Add(new XElement(El.IsTestProject, "true"));

            foreach (var el in oldProps.Elements().Where(oldEl => !newProps.Elements().Any(newEl => newEl.Name == oldEl.Name)))
                newProps.Add(el);

            // remove project references group
            info.Elements(El.ItemGroup).Where(e => e.Elements(El.ProjectReference).Count() > 0).Remove();

            // remove package references group
            info.Elements(El.ItemGroup).Where(e => e.Elements(El.PackageReference).Count() > 0).Remove();

            // add package references group
            if (project.Packages.Count > 0)
                newProps.AddAfterSelf(new XElement(
                    El.ItemGroup,
                    project.Packages.OrderBy(e => e.Value.Name).Select(e => new XElement(
                        El.PackageReference,
                        new XAttribute(El.Include, e.Value.Name),
                        new XAttribute(El.Version, e.Value.Version)
                    ))
                ));

            // add project references group
            if (project.Projects.Count > 0)
                newProps.AddAfterSelf(new XElement(
                    El.ItemGroup,
                    project.Projects.OrderBy(e => e.Value.Name).Select(e => new XElement(
                        El.ProjectReference,
                        new XAttribute(El.Include, Path.GetRelativePath(dir, e.Value.File).Replace('\\', '/'))
                    ))
                ));

            var xws = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = new string(' ', 4),
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(false),
            };
            using(var fs = new FileStream(path, FileMode.Truncate))
            using(var xw = XmlWriter.Create(fs, xws))
            {
                info.Save(xw);
            }
        }

        private void ValidateProperties(string path, XElement properties)
        {
            if (properties.Element(El.PackageId) is null)
                throw new InvalidOperationException($"Project {path} has no {El.PackageId} defined.");

            var name = properties.Element(El.PackageId).Value;

            var fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName != name)
                throw new InvalidOperationException($"Project {path} project file name {fileName} doesn't match declared name {name}.");

            var dirName = Path.GetFileName(Path.GetDirectoryName(path));
            if (dirName != name)
                throw new InvalidOperationException($"Project {path} project directory name {dirName} doesn't match declared name {name}.");

            if (properties.Element(El.PackageVersion) is null)
                throw new InvalidOperationException($"Project {path} has no {El.PackageVersion} defined.");

            if (properties.Element(El.Description) is null)
                throw new InvalidOperationException($"Project {path} has no {El.Description} defined.");

            if (properties.Element(El.TargetFramework) is null)
                throw new InvalidOperationException($"Project {path} has no {El.TargetFramework} defined.");

            if (properties.Element(El.DebugType)?.Value != "portable")
                throw new InvalidOperationException($"Project {path} has no {El.DebugType} defined or it is not portable.");

            var outputType = properties.Element(El.OutputType)?.Value;
            var outputTypes = Enum.GetNames(typeof(OutputType));
            if (!outputTypes.Contains(outputType))
                throw new InvalidOperationException($"Project {path} has no {El.OutputType} or it is not in {string.Join(", ", outputTypes)}.");

            if (properties.Element(El.LangVersion)?.Value != "latest")
                throw new InvalidOperationException($"Project {path} has no {El.LangVersion} defined or it is not latest.");

            ensureValidBoolean(El.WarningsAsErrors);
            if (properties.Element(El.WarningsAsErrors)?.Value != "true")
                throw new InvalidOperationException($"Project {path} has no {El.WarningsAsErrors} defined or it is not true.");

            ensureValidBoolean(El.IsPackable);
            if (properties.Element(El.IsPackable) is null)
                throw new InvalidOperationException($"Project {path} has no {El.IsPackable} defined.");

            ensureValidBoolean(El.IsTestProject);

            void ensureValidBoolean(string el)
            {
                var element = properties.Element(el);
                if (element != null && !booleanStrings.Contains(element.Value))
                    throw new InvalidOperationException($"Project {path} {el} must be one of {string.Join(", ", booleanStrings)}.");
            }
        }

        private string ReadProjectDependency(
            string project,
            FileInfo file,
            XElement reference,
            DiscoverConfiguration configuration
        )
        {
            var relativePath = reference.Attribute(El.Include)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty project dependency.");

            relativePath = relativePath.Replace('\\', '/');

            var path = Path.GetFullPath(Path.Combine(file.DirectoryName, relativePath));
            if (!File.Exists(path))
                throw new InvalidOperationException($"Project {project} has broken project dependency {relativePath}.");

            return path;
        }

        private Package ReadPackageDependency(
            string project,
            XElement reference,
            DiscoverConfiguration configuration
        )
        {
            var name = reference.Attribute(El.Include)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty package dependency name.");

            if (configuration.SkipChecks && implicitPackages.Any(p => p == name))
                return new Package(Constants.ProjectType, name, new Core.Models.Version(1, 0, 0, string.Empty));

            var rawVersion = reference.Attribute(El.Version)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty package dependency {name} version.");

            if (!Core.Models.Version.TryParse(rawVersion, out var version))
                throw new InvalidOperationException($"Project {project} package dependency {name} version {rawVersion} is invalid.");

            return new Package(Constants.ProjectType, name, version);
        }

        private static class El
        {
            public const string PackageId = "PackageId";
            public const string PackageVersion = "PackageVersion";
            public const string Description = "Description";
            public const string TargetFramework = "TargetFramework";
            public const string DebugType = "DebugType";
            public const string OutputType = "OutputType";
            public const string WarningsAsErrors = "WarningsAsErrors";
            public const string LangVersion = "LangVersion";
            public const string IsPackable = "IsPackable";
            public const string IsTestProject = "IsTestProject";
            public const string PropertyGroup = "PropertyGroup";
            public const string ItemGroup = "ItemGroup";
            public const string PackageReference = "PackageReference";
            public const string ProjectReference = "ProjectReference";
            public const string Include = "Include";
            public const string Version = "Version";
        }
    }
}