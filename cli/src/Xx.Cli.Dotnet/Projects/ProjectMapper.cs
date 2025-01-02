using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Annium;
using Annium.Linq;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Dotnet.Extensions;
using Xx.Cli.Dotnet.Models;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Cli.Dotnet.Projects;

internal class ProjectMapper(IPackageVersionsManager packageVersionsManager)
    : IProjectMapper<IPlatformProject, RawProject>
{
    private static readonly string[] _implicitPackages = ["Microsoft.AspNetCore.App"];
    private static readonly string[] _booleanStrings = ["true", "false"];
    private static readonly string[] _disabledProperties =
    [
        El.PublishReadyToRun,
        El.PublishReadyToRunShowWarnings,
        El.PackageId,
        El.PackageVersion,
        El.Description,
        El.DebugType,
        El.WarningsAsErrors,
        El.Nullable,
    ];

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
        project.Description = properties.GetElement(El.Description)?.Value ?? string.Empty;

        project.Solutions =
            properties.GetElement(El.Solutions)?.Value.Split(';').WhereNot(string.IsNullOrWhiteSpace).ToArray() ?? [];
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
                var package = ReadPackageDependency(
                    Path.GetDirectoryName(path).NotNull(),
                    project.Name,
                    reference,
                    configuration
                );
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
            // newProps.Add(new XElement(El.PackageId, project.Name));
            // newProps.Add(new XElement(El.PackageVersion, project.Version));
            // newProps.Add(new XElement(El.Description, project.Description));
            newProps.Add(new XElement(El.Solutions, project.Solutions.Join(";")));
            // newProps.Add(new XElement(El.TargetFramework, project.TargetFramework));
            newProps.Add(new XElement(El.OutputType, project.OutputType));
            // newProps.Add(new XElement(El.DebugType, "portable"));
            // newProps.Add(new XElement(El.WarningsAsErrors, "true"));
            newProps.Add(new XElement(El.IsPackable, project is IPublishableProject ? "true" : "false"));
            if (project is TestProject)
                newProps.Add(new XElement(El.IsTestProject, "true"));

            // newProps.Add(new XElement(El.Nullable, "enable"));
            // newProps.Add(new XElement(El.PublishReadyToRun, "true"));
            // newProps.Add(new XElement(El.PublishReadyToRunShowWarnings, "true"));
        }

        var remainingProps = oldProps
            .Elements()
            .Where(el => !_disabledProperties.Contains(el.Name.ToString()))
            .Where(el => newProps.Elements().All(newEl => newEl.Name != el.Name))
            .ToList();
        newProps.Add(remainingProps);

        // add package references group
        newProps.AddAfterSelf(SavePackages(info, project.Directory, project.Packages));

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

    private XElement SavePackages(XElement info, string directory, IReadOnlyCollection<Dependency<Package>> packages)
    {
        // collect target refs
        var refs = packages
            .OrderBy(x => x.Value.Name)
            .ToDictionary(
                x => x.Value.Name,
                x => new XElement(
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
                if (newVersion is null)
                    continue;

                // update inline version if defined
                if (existingRef.Attribute(El.Version) is not null)
                    existingRef.SetAttributeValue(El.Version, newVersion.Value);
                else
                    packageVersionsManager.SaveVersion(directory, include, newVersion.Value);
            }

        var sortedRefs = refs.OrderBy(x => x.Key, StringComparer.InvariantCultureIgnoreCase).ToArray();
        foreach (var pair in sortedRefs)
            group.Add(pair.Value);

        return group;
    }

    private static void ValidateProperties(string path, XElement properties)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var dirName = Path.GetFileName(Path.GetDirectoryName(path));
        if (dirName != fileName)
            throw new InvalidOperationException(
                $"Project {path} project directory name {dirName} doesn't match declared name {fileName}."
            );

        var outputType = properties.GetElement(El.OutputType)?.Value;
        var outputTypes = Enum.GetNames(typeof(OutputType));
        if (!outputTypes.Contains(outputType))
            throw new InvalidOperationException(
                $"Project {path} has no {El.OutputType} or it is not in {string.Join(", ", outputTypes)}."
            );

        EnsureValidBoolean(El.IsTestProject);

        void EnsureValidBoolean(string el)
        {
            var element = properties.GetElement(el);
            if (element is not null && !_booleanStrings.Contains(element.Value))
                throw new InvalidOperationException(
                    $"Project {path} {el} must be one of {string.Join(", ", _booleanStrings)}."
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

    private Package ReadPackageDependency(
        string directory,
        string project,
        XElement reference,
        DiscoverConfiguration configuration
    )
    {
        var name =
            reference.Attribute(El.Include)?.Value
            ?? throw new InvalidOperationException($"Project {project} has empty package dependency name.");

        if (configuration.SkipChecks && _implicitPackages.Any(p => p == name))
            return new Package(Constants.ProjectType, name, new Version(1, 0, 0, string.Empty));

        var rawVersion = reference.Attribute(El.Version)?.Value ?? string.Empty;

        if (rawVersion == string.Empty)
        {
            var version =
                packageVersionsManager.ResolveVersion(directory, name)
                ?? throw new InvalidOperationException(
                    $"Project {project} package dependency {name} version is defined not locally neither in Directory.Packages.props file(s)."
                );
            return new Package(Constants.ProjectType, name, version);
        }

        {
            if (!Version.TryParse(rawVersion, out var version))
                throw new InvalidOperationException(
                    $"Project {project} package dependency {name} version {rawVersion} is invalid."
                );

            return new Package(Constants.ProjectType, name, version);
        }
    }
}
