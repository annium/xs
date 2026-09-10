using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Annium.Xs.Cli.Dotnet.Extensions;
using Annium.Xs.Cli.Dotnet.Models;
using Version = Annium.Xs.Cli.Core.Models.Version;

namespace Annium.Xs.Cli.Dotnet.Projects;

internal interface IPropsFilesManager
{
    TargetFramework? ResolveTarggetFramework(string directory);

    /// <summary>
    /// Whether the nearest Directory.Packages.props above the directory turns on central package management.
    /// When it does, a PackageReference must not carry a Version attribute - NuGet rejects the project
    /// outright (NU1008), and the version belongs in a PackageVersion item instead.
    /// </summary>
    bool IsCentrallyManaged(string directory);

    Version? ResolveVersion(string directory, string name);
    void SaveVersion(string directory, string name, string version);
}

internal class PropsFilesManager : IPropsFilesManager
{
    private const string DirectoryBuildFileName = "Directory.Build.props";
    private const string DirectoryPackagesFileName = "Directory.Packages.props";

    public TargetFramework? ResolveTarggetFramework(string directory)
    {
        TargetFramework? result = null;
        QueryHierarchically(
            directory,
            DirectoryBuildFileName,
            el =>
            {
                var rawTargetFramework = el.GetElement(El.PropertyGroup)?.GetElement(El.TargetFramework);
                if (rawTargetFramework is null)
                    return false;

                result = rawTargetFramework.Value;
                return true;
            }
        );

        return result;
    }

    public bool IsCentrallyManaged(string directory)
    {
        var result = false;
        QueryHierarchically(
            directory,
            DirectoryPackagesFileName,
            el =>
            {
                var raw = el.GetElement(El.PropertyGroup)?.GetElement(El.ManagePackageVersionsCentrally)?.Value;
                if (raw is null)
                    return false;

                result = bool.TryParse(raw, out var value) && value;
                return true;
            }
        );

        return result;
    }

    public Version? ResolveVersion(string directory, string name)
    {
        Version? result = null;
        QueryHierarchically(
            directory,
            DirectoryPackagesFileName,
            el =>
            {
                var rawVersions = el.GetElement(El.ItemGroup)?.GetElements(El.PackageVersion).ToArray();
                if (rawVersions is null)
                    return false;

                var rawVersion = rawVersions
                    .FirstOrDefault(x => x.Attribute(El.Include)?.Value == name)
                    ?.Attribute(El.Version)
                    ?.Value;
                if (rawVersion is null)
                    return false;

                if (!Version.TryParse(rawVersion, out var version))
                    return false;

                result = version;
                return true;
            }
        );

        return result;
    }

    public void SaveVersion(string directory, string name, string version)
    {
        // update the entry wherever in the hierarchy it is declared
        var updated = ExecuteHierarchically(
            directory,
            DirectoryPackagesFileName,
            el =>
            {
                var rawVersion = el.GetElement(El.ItemGroup)
                    ?.GetElements(El.PackageVersion)
                    .FirstOrDefault(x => x.Attribute(El.Include)?.Value == name);
                if (rawVersion is null)
                    return false;

                rawVersion.SetAttributeValue(El.Version, version);
                return true;
            }
        );

        if (updated)
            return;

        // a package nothing declared yet belongs in the nearest file, kept in the order the file is kept in
        var added = ExecuteHierarchically(
            directory,
            DirectoryPackagesFileName,
            el =>
            {
                var group = el.GetElement(El.ItemGroup);
                if (group is null)
                {
                    group = new XElement(El.ItemGroup);
                    el.Add(group);
                }

                var entry = new XElement(
                    El.PackageVersion,
                    new XAttribute(El.Include, name),
                    new XAttribute(El.Version, version)
                );

                var next = group
                    .GetElements(El.PackageVersion)
                    .FirstOrDefault(x =>
                        string.Compare(
                            x.Attribute(El.Include)?.Value,
                            name,
                            StringComparison.InvariantCultureIgnoreCase
                        ) > 0
                    );

                if (next is null)
                    group.Add(entry);
                else
                    next.AddBeforeSelf(entry);

                return true;
            }
        );

        if (!added)
            throw new InvalidOperationException(
                $"Failed to save {name}@{version} from {directory}: no {DirectoryPackagesFileName} found above it"
            );
    }

    /// <summary>
    /// Walks up looking for the file, and stops at the first one the handler accepts. Unlike
    /// <see cref="ExecuteHierarchically" /> it never writes, so a lookup does not rewrite what it read.
    /// </summary>
    private bool QueryHierarchically(string directory, string fileName, Func<XElement, bool> handle)
    {
        var dir = directory;
        while (dir is not null)
        {
            var file = Path.Combine(dir, fileName);
            if (File.Exists(file) && handle(Read(file)))
                return true;

            dir = Directory.GetParent(dir)?.FullName;
        }

        return false;
    }

    private bool ExecuteHierarchically(string directory, string fileName, Func<XElement, bool> handle)
    {
        var dir = directory;
        while (dir is not null)
        {
            var file = Path.Combine(dir, fileName);
            if (!File.Exists(file))
            {
                dir = Directory.GetParent(dir)?.FullName;
                continue;
            }

            var element = Read(file);
            var succeed = handle(element);
            if (!succeed)
            {
                dir = Directory.GetParent(dir)?.FullName;
                continue;
            }

            Write(file, element);
            return true;
        }

        return false;
    }

    private static XElement Read(string file)
    {
        using var fs = File.OpenRead(file);
        return XElement.Load(fs);
    }

    private static void Write(string file, XElement element)
    {
        var xws = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = new string(' ', 2),
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
        };

        using (var xw = XmlWriter.Create(file, xws))
            element.Save(xw);
        File.AppendAllText(file, "\n");
    }
}
