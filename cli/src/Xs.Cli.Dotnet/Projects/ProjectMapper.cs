using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class ProjectMapper : IProjectMapper<ISpecialProject, RawProject>
    {
        private static readonly IEnumerable<string> outputTypes = new [] { "Exe", "Library" };

        public RawProject Load(string path)
        {
            var project = new RawProject();
            var file = new FileInfo(path);

            var info = XElement.Load(file.OpenRead());

            var properties = info.Element(El.PropertyGroup);
            ValidateProperties(path, properties);

            project.Name = Path.GetFileNameWithoutExtension(file.Name);
            project.Version = new Core.Models.Version(properties.Element(El.PackageVersion).Value);
            project.Description = properties.Element(El.Description).Value;
            project.TargetFramework = TargetFrameworkParser.Parse(properties.Element(El.TargetFramework).Value);
            project.OutputType = properties.Element(El.OutputType).Value == "Exe" ? OutputType.Executable : OutputType.Library;

            project.ProjectDependencies = GetReferenceElements(El.ProjectReference)
                .Select(reference => ReadProjectDependency(project.Name, file, reference))
                .ToArray();

            project.PackageDependencies = GetReferenceElements(El.PackageReference)
                .Select(reference => ReadPackageDependency(project.Name, reference))
                .ToArray();

            return project;

            IEnumerable<XElement> GetReferenceElements(string referenceType) => info
                .Elements(El.ItemGroup)
                .SelectMany(group => group.Elements(referenceType));
        }

        public void Save(ISpecialProject project)
        {
            var path = project.File.FullName;
            var dir = Directory.GetParent(path).FullName;

            var info = XElement.Parse(File.ReadAllText(path));

            info.Element(El.PropertyGroup).SetElementValue(El.PackageVersion, project.Version);

            // remove project references group
            info.Elements(El.ItemGroup).Where(e => e.Elements(El.ProjectReference).Count() > 0).Remove();

            // remove package references group
            info.Elements(El.ItemGroup).Where(e => e.Elements(El.PackageReference).Count() > 0).Remove();

            // add project references group
            if (project.ProjectDependencies.Count > 0)
                info.Add(new XElement(
                    El.ItemGroup,
                    project.ProjectDependencies.OrderBy(e => e.Name).Select(e => new XElement(
                        El.ProjectReference,
                        new XAttribute(El.Include, Path.GetRelativePath(dir, e.File.FullName))
                    ))
                ));

            // add package references group
            if (project.PackageDependencies.Count > 0)
                info.Add(new XElement(
                    El.ItemGroup,
                    project.PackageDependencies.OrderBy(e => e.Name).Select(e => new XElement(
                        El.PackageReference,
                        new XAttribute(El.Include, e.Name),
                        new XAttribute(El.Version, e.Version)
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
            if (properties == null)
                throw new InvalidOperationException($"Project {path} has no properties defined.");

            if (properties.Element(El.PackageId) == null)
                throw new InvalidOperationException($"Project {path} has no {El.PackageId} defined.");

            var name = properties.Element(El.PackageId).Value;

            var fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName != name)
                throw new InvalidOperationException($"Project {path} project file name {fileName} doesn't match declared name {name}.");

            var dirName = Path.GetFileName(Path.GetDirectoryName(path));
            if (dirName != name)
                throw new InvalidOperationException($"Project {path} project directory name {dirName} doesn't match declared name {name}.");

            if (properties.Element(El.PackageVersion) == null)
                throw new InvalidOperationException($"Project {path} has no {El.PackageVersion} defined.");

            if (properties.Element(El.Description) == null)
                throw new InvalidOperationException($"Project {path} has no {El.Description} defined.");

            if (properties.Element(El.TargetFramework) == null)
                throw new InvalidOperationException($"Project {path} has no {El.TargetFramework} defined.");

            if (properties.Element(El.DebugType)?.Value != "portable")
                throw new InvalidOperationException($"Project {path} has no {El.DebugType} defined or it is not portable.");

            var outputType = properties.Element(El.OutputType)?.Value;
            if (!outputTypes.Contains(outputType))
                throw new InvalidOperationException($"Project {path} has no {El.OutputType} or it is not in {string.Join(", ", outputTypes)}.");

            if (properties.Element(El.WarningsAsErrors)?.Value != "true")
                throw new InvalidOperationException($"Project {path} has no {El.WarningsAsErrors} defined or it is not true.");
        }

        private string ReadProjectDependency(
            string project,
            FileInfo location,
            XElement reference
        )
        {
            var relativePath = reference.Attribute(El.Include)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty project dependency.");

            var path = Path.Combine(location.DirectoryName, relativePath);
            if (!File.Exists(path))
                throw new InvalidOperationException($"Project {project} has broken project dependency {relativePath}.");

            return path;
        }

        private static Dependency ReadPackageDependency(
            string project,
            XElement reference
        )
        {
            var name = reference.Attribute(El.Include)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty package dependency name.");

            var version = new Core.Models.Version(reference.Attribute(El.Version)?.Value ??
                throw new InvalidOperationException($"Project {project} has empty package dependency {name} version."));

            return new Dependency(Constants.ProjectType, name, version);
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

            public const string PropertyGroup = "PropertyGroup";

            public const string ItemGroup = "ItemGroup";

            public const string PackageReference = "PackageReference";

            public const string ProjectReference = "ProjectReference";

            public const string Include = "Include";

            public const string Version = "Version";
        }
    }
}