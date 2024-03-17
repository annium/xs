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
using Version = Xs.Cli.Core.Models.Version;

namespace Xs.Cli.Dotnet.Projects;

internal class ProjectMapper : IProjectMapper<IPlatformProject, RawProject>
{
    private static readonly string[] ImplicitPackages = { "Microsoft.AspNetCore.App" };
    private static readonly string[] BooleanStrings = { "true", "false" };
    private static readonly string[] DisabledProperties = { El.PublishReadyToRun, El.PublishReadyToRunShowWarnings };

    public RawProject Load(string path, DiscoverConfiguration configuration)
    {
        var project = new RawProject();
        var file = new FileInfo(path);

        var info = XElement.Load(file.OpenRead());

        var properties = info.GetElement(El.PropertyGroup);

        if (properties is null)
            throw new InvalidOperationException($"Project {path} has no properties defined.");
        if (!configuration.SkipChecks)
            ValidateProperties(path, properties);

        project.Name = Path.GetFileNameWithoutExtension(file.Name);
        if (configuration.SkipChecks)
        {
            var rawVersion = properties.GetElement(El.PackageVersion)?.Value ?? "0.1.0";
            if (!Version.TryParse(rawVersion, out var version))
                throw new ArgumentException($"Project {project.Name} version {rawVersion} is invalid");

            project.Version = version;
            project.Description = properties.GetElement(El.Description)?.Value ?? string.Empty;
        }
        else
        {
            var rawVersion = properties.GetElement(El.PackageVersion)?.Value ?? string.Empty;
            if (!Version.TryParse(rawVersion, out var version))
                throw new ArgumentException($"Project {project.Name} version {rawVersion} is invalid");

            project.Version = version;
            project.Description = properties.GetElement(El.Description)?.Value ?? string.Empty;
        }

        project.TargetFramework = properties.GetElement(El.TargetFramework)?.Value ?? TargetFramework.Net7;
        if (configuration.SkipChecks)
            project.OutputType =
                properties.GetElement(El.OutputType)?.Value == "Exe" ? OutputType.Exe : OutputType.Library;
        else
            project.OutputType =
                properties.GetElement(El.OutputType)!.Value == "Exe" ? OutputType.Exe : OutputType.Library;

        project.Projects = GetReferenceElements(El.ProjectReference)
            .Select(reference => ReadProjectDependency(project.Name, file, reference))
            .Select(reference => new Dependency<string>(DependencyType.Normal, reference))
            .ToArray();

        project.Packages = GetReferenceElements(El.PackageReference)
            .Select(reference =>
            {
                var package = ReadPackageDependency(project.Name, reference, configuration);
                var dependencyType =
                    reference.GetElements(El.PrivateAssets).Any() || reference.Attribute(El.PrivateAssets) is not null
                        ? DependencyType.Dev
                        : DependencyType.Normal;

                return new Dependency<Package>(dependencyType, package);
            })
            .ToArray();

        project.IsPackable =
            properties.GetElement(El.IsPackable) is { } && bool.Parse(properties.GetElement(El.IsPackable)!.Value);

        project.IsTestProject =
            properties.GetElement(El.IsTestProject) is { }
            && bool.Parse(properties.GetElement(El.IsTestProject)!.Value);

        return project;

        IEnumerable<XElement> GetReferenceElements(string referenceType) =>
            info.GetElements(El.ItemGroup).SelectMany(group => group.GetElements(referenceType));
    }

    public void Save(IPlatformProject project)
    {
        var path = project.File;
        var parent =
            Directory.GetParent(path) ?? throw new DirectoryNotFoundException($"Path {path} has no parent directory");
        var dir = parent.FullName;

        var info = XElement.Parse(File.ReadAllText(path));

        var oldProps = info.GetElement(El.PropertyGroup)!;
        var newProps = new XElement(El.PropertyGroup);
        oldProps.AddBeforeSelf(newProps);
        oldProps.Remove();

        if (project.Config.AddPreferredAttributes)
        {
            newProps.Add(new XElement(El.PackageId, project.Name));
            newProps.Add(new XElement(El.PackageVersion, project.Version));
            newProps.Add(new XElement(El.Description, project.Description));
            newProps.Add(new XElement(El.TargetFramework, project.TargetFramework));
            newProps.Add(new XElement(El.OutputType, project.OutputType));
            newProps.Add(new XElement(El.DebugType, "portable"));
            newProps.Add(new XElement(El.WarningsAsErrors, "true"));
            newProps.Add(new XElement(El.IsPackable, project is IPublishableProject ? "true" : "false"));
            if (project is TestProject)
                newProps.Add(new XElement(El.IsTestProject, "true"));

            newProps.Add(new XElement(El.Nullable, "enable"));
            // newProps.Add(new XElement(El.PublishReadyToRun, "true"));
            // newProps.Add(new XElement(El.PublishReadyToRunShowWarnings, "true"));
        }

        var remainingProps = oldProps
            .Elements()
            .Where(el => !DisabledProperties.Contains(el.Name.ToString()))
            .Where(el => newProps.Elements().All(newEl => newEl.Name != el.Name))
            .ToList();
        newProps.Add(remainingProps);

        // add package references group
        newProps.AddAfterSelf(SavePackages(info, project.Packages));

        // add project references group
        newProps.AddAfterSelf(SaveProjects(info, dir, project.Config.DirectorySeparator[0], project.Projects));

        // remove empty item groups
        info.GetElements(El.ItemGroup).Where(x => !x.HasElements).Remove();

        var xws = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = new string(' ', 4),
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
        };

        using var xw = XmlWriter.Create(path, xws);
        info.Save(xw);
    }

    private XElement SaveProjects(
        XElement info,
        string dir,
        char separator,
        IReadOnlyCollection<Dependency<IProject>> projects
    )
    {
        // collect target refs
        var refs = projects
            .Select(x => NormalizePath(Path.GetRelativePath(dir, x.Value.File)))
            .ToDictionary(x => x, x => new XElement(El.ProjectReference, new XAttribute(El.Include, x)));

        // collect existing package references
        var existingRefs = info.GetElements(El.ItemGroup)
            .SelectMany(x => x.GetElements(El.ProjectReference))
            .Select(x =>
            {
                var include = x.Attribute(El.Include)?.Value!;
                var path = Path.GetRelativePath(dir, Path.GetFullPath(include, dir));

                return (Path: path, Value: x);
            })
            .ToDictionary(x => x.Path, x => x.Value);

        var group = new XElement(El.ItemGroup);

        // remove existing refs from their parents
        foreach (var existingRef in existingRefs)
            existingRef.Value.Remove();

        // replace refs with existing refs (thus preserving attributes and inner structure)
        foreach (var (include, existingRef) in existingRefs)
            if (refs.ContainsKey(include))
                refs[include] = existingRef;

        var sortedRefs = refs.OrderBy(x => x.Key, StringComparer.InvariantCultureIgnoreCase).ToArray();
        foreach (var pair in sortedRefs)
            group.Add(pair.Value);

        return group;

        string NormalizePath(string path) => path.Replace('\\', '/').Replace('/', separator);
    }

    private XElement SavePackages(XElement info, IReadOnlyCollection<Dependency<Package>> packages)
    {
        // collect target refs
        var refs = packages
            .OrderBy(x => x.Value.Name)
            .ToDictionary(
                x => x.Value.Name,
                x =>
                    new XElement(
                        El.PackageReference,
                        new XAttribute(El.Include, x.Value.Name),
                        new XAttribute(El.Version, x.Value.Version)
                    )
            );

        // collect existing package references
        var existingRefs = info.GetElements(El.ItemGroup)
            .SelectMany(x => x.GetElements(El.PackageReference))
            .Select(x => (Name: x.Attribute(El.Include)?.Value, Value: x))
            .ToDictionary(x => x.Name!, x => x.Value);

        var group = new XElement(El.ItemGroup);

        // remove existing refs from their parents
        foreach (var existingRef in existingRefs)
            existingRef.Value.Remove();

        // replace refs with existing refs (thus preserving attributes and inner structure)
        foreach (var (include, existingRef) in existingRefs)
            if (refs.TryGetValue(include, out var newRef))
            {
                refs[include] = existingRef;
                var newVersion = newRef.Attribute(El.Version);
                if (newVersion is not null)
                    existingRef.SetAttributeValue(El.Version, newVersion.Value);
            }

        var sortedRefs = refs.OrderBy(x => x.Key, StringComparer.InvariantCultureIgnoreCase).ToArray();
        foreach (var pair in sortedRefs)
            group.Add(pair.Value);

        return group;
    }

    private static void ValidateProperties(string path, XElement properties)
    {
        if (properties.GetElement(El.PackageId) is null)
            throw new InvalidOperationException($"Project {path} has no {El.PackageId} defined.");

        var name = properties.GetElement(El.PackageId)!.Value;

        var fileName = Path.GetFileNameWithoutExtension(path);
        if (fileName != name)
            throw new InvalidOperationException(
                $"Project {path} project file name {fileName} doesn't match declared name {name}."
            );

        var dirName = Path.GetFileName(Path.GetDirectoryName(path));
        if (dirName != name)
            throw new InvalidOperationException(
                $"Project {path} project directory name {dirName} doesn't match declared name {name}."
            );

        if (properties.GetElement(El.PackageVersion) is null)
            throw new InvalidOperationException($"Project {path} has no {El.PackageVersion} defined.");

        if (properties.GetElement(El.Description) is null)
            throw new InvalidOperationException($"Project {path} has no {El.Description} defined.");

        if (properties.GetElement(El.TargetFramework) is null)
            throw new InvalidOperationException($"Project {path} has no {El.TargetFramework} defined.");

        if (properties.GetElement(El.DebugType)?.Value != "portable")
            throw new InvalidOperationException($"Project {path} has no {El.DebugType} defined or it is not portable.");

        var outputType = properties.GetElement(El.OutputType)?.Value;
        var outputTypes = Enum.GetNames(typeof(OutputType));
        if (!outputTypes.Contains(outputType))
            throw new InvalidOperationException(
                $"Project {path} has no {El.OutputType} or it is not in {string.Join(", ", outputTypes)}."
            );

        EnsureValidBoolean(El.WarningsAsErrors);
        if (properties.GetElement(El.WarningsAsErrors)?.Value != "true")
            throw new InvalidOperationException(
                $"Project {path} has no {El.WarningsAsErrors} defined or it is not true."
            );

        EnsureValidBoolean(El.IsPackable);
        if (properties.GetElement(El.IsPackable) is null)
            throw new InvalidOperationException($"Project {path} has no {El.IsPackable} defined.");

        EnsureValidBoolean(El.IsTestProject);

        void EnsureValidBoolean(string el)
        {
            var element = properties.GetElement(el);
            if (element is not null && !BooleanStrings.Contains(element.Value))
                throw new InvalidOperationException(
                    $"Project {path} {el} must be one of {string.Join(", ", BooleanStrings)}."
                );
        }
    }

    private static string ReadProjectDependency(string project, FileInfo file, XElement reference)
    {
        var relativePath =
            reference.Attribute(El.Include)?.Value
            ?? throw new InvalidOperationException($"Project {project} has empty project dependency.");

        relativePath = relativePath.Replace('\\', '/');

        var path = Path.GetFullPath(Path.Combine(file.DirectoryName!, relativePath));
        if (!File.Exists(path))
            throw new InvalidOperationException($"Project {project} has broken project dependency {relativePath}.");

        return path;
    }

    private static Package ReadPackageDependency(
        string project,
        XElement reference,
        DiscoverConfiguration configuration
    )
    {
        var name =
            reference.Attribute(El.Include)?.Value
            ?? throw new InvalidOperationException($"Project {project} has empty package dependency name.");

        if (configuration.SkipChecks && ImplicitPackages.Any(p => p == name))
            return new Package(Constants.ProjectType, name, new Version(1, 0, 0, string.Empty));

        var rawVersion =
            reference.Attribute(El.Version)?.Value
            ?? throw new InvalidOperationException($"Project {project} has empty package dependency {name} version.");

        if (!Version.TryParse(rawVersion, out var version))
            throw new InvalidOperationException(
                $"Project {project} package dependency {name} version {rawVersion} is invalid."
            );

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
        public const string IsPackable = "IsPackable";
        public const string IsTestProject = "IsTestProject";
        public const string Nullable = "Nullable";
        public const string PublishReadyToRun = "PublishReadyToRun";
        public const string PublishReadyToRunShowWarnings = "PublishReadyToRunShowWarnings";
        public const string PropertyGroup = "PropertyGroup";
        public const string ItemGroup = "ItemGroup";
        public const string PackageReference = "PackageReference";
        public const string ProjectReference = "ProjectReference";
        public const string PrivateAssets = "PrivateAssets";
        public const string Include = "Include";
        public const string Version = "Version";
    }
}

internal static class XElementExtensions
{
    public static XElement? GetElement(this XElement container, string name)
    {
        return container.Element(XName.Get(name, container.Name.NamespaceName));
    }

    public static IEnumerable<XElement> GetElements(this XElement container, string name)
    {
        return container.Elements(XName.Get(name, container.Name.NamespaceName));
    }
}
