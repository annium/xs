using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Dotnet.Tools;

internal class SpecialConfigurationManager : ISpecialConfigurationManager, ILogSubject<SpecialConfigurationManager>
{
    private const string File = "nuget.config";
    public ProjectType Type => Constants.ProjectType;
    public string[] IgnorePatterns { get; } = new[] { File, "lcov.info" };
    public ILogger<SpecialConfigurationManager> Logger { get; }
    private readonly string _registryName = "registry";
    private readonly string _defaultName = "nuget";
    private readonly Uri _defaultUri = new(Constants.DefaultServer);

    public SpecialConfigurationManager(
        ILogger<SpecialConfigurationManager> logger
    )
    {
        Logger = logger;
    }

    public void Save(IProject project, ProjectTypeConfiguration configuration)
    {
        this.Log().Trace($"Save configuration for {Constants.ProjectType} project {project}");

        var sources = new XElement(El.PackageSources);
        sources.Add(new XElement(El.Clear));

        sources.Add(GetAddRule(_registryName, configuration.Server));

        sources.Add(GetAddRule(_defaultName, _defaultUri));

        Save(project.Directory, new XElement(El.Configuration, sources));
    }

    public void Delete(IProject project)
    {
        var path = FilePath(project.Directory);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private void Save(string folder, XElement info)
    {
        var path = FilePath(folder);
        var xws = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = new string(' ', 2),
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
        };

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var xw = XmlWriter.Create(fs, xws);
        info.Save(xw);
    }

    private XElement GetAddRule(string name, Uri uri) => new(
        El.Add,
        new XAttribute(El.Key, name),
        new XAttribute(El.Value, uri.IsFile ? uri.AbsolutePath : new Uri(uri, Constants.ServerPathSuffix).ToString())
    );

    private string FilePath(string folder) => Path.Combine(folder, File);

    private static class El
    {
        public const string Configuration = "configuration";
        public const string PackageSources = "packageSources";
        public const string Clear = "clear";
        public const string Add = "add";
        public const string Key = "key";
        public const string Value = "value";
    }
}