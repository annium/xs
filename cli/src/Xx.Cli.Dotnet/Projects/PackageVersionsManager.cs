using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xx.Cli.Dotnet.Extensions;
using Version = Xx.Cli.Core.Models.Version;

namespace Xx.Cli.Dotnet.Projects;

internal interface IPackageVersionsManager
{
    Version? ResolveVersion(string directory, string name);
    void SaveVersion(string directory, string name, string version);
}

internal class PackageVersionsManager : IPackageVersionsManager
{
    private const string PackageVersionsFileName = "Directory.Packages.props";

    public Version? ResolveVersion(string directory, string name)
    {
        Version? result = null;
        ExecuteHierarchically(
            directory,
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
        var succeed = ExecuteHierarchically(
            directory,
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

        if (!succeed)
            throw new InvalidOperationException($"Failed to save {name}@{version} from {directory}");
    }

    private bool ExecuteHierarchically(string directory, Func<XElement, bool> handle)
    {
        var dir = directory;
        while (dir is not null)
        {
            var file = Path.Combine(dir, PackageVersionsFileName);
            if (!File.Exists(file))
            {
                dir = Directory.GetParent(dir)?.FullName;
                continue;
            }

            var element = Read(file);
            var succeed = handle(element);
            if (!succeed)
                continue;

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
            IndentChars = new string(' ', 4),
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
        };

        using var xw = XmlWriter.Create(file, xws);
        element.Save(xw);
    }
}
