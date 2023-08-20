using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Annium.Configuration.Abstractions;
using Annium.Core.Mapper;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tools;

internal class ConfigurationManager : IConfigurationManager, ILogSubject<ConfigurationManager>
{
    private const string ConfigurationFile = ".xs";
    private const string CredentialsFile = ".xs.credentials";
    private const string IgnoreHeader = "# xs ignore patterns";
    private const string IgnoreFile = ".gitignore";
    public ILogger<ConfigurationManager> Logger { get; }
    private readonly IReadOnlyDictionary<ProjectType, ISpecialConfigurationManager> _specialManagers;
    private readonly Func<IConfigurationBuilder> _configurationBuilderFactory;
    private readonly IMapper _mapper;

    public ConfigurationManager(
        IEnumerable<ISpecialConfigurationManager> specialManagers,
        Func<IConfigurationBuilder> configurationBuilderFactory,
        IMapper mapper,
        ILogger<ConfigurationManager> logger
    )
    {
        _specialManagers = specialManagers.ToDictionary(e => e.Type, e => e);
        _configurationBuilderFactory = configurationBuilderFactory;
        _mapper = mapper;
        Logger = logger;
    }

    public Configuration Load(string folder)
    {
        this.Log().Trace($"Load configuration from {folder}");
        var directory = GetConfigurationFolder(new DirectoryInfo(folder));

        if (directory is null)
        {
            this.Log().Trace($"Configuration missing in {folder}. Returning default");
            return Configuration.Empty;
        }

        this.Log().Trace($"Loaded configuration from {directory}");
        var cfgFile = GetConfigurationFile(directory.FullName);
        var credFile = GetCredentialsFile(directory.FullName);

        var config = _configurationBuilderFactory()
            .AddYamlFile(cfgFile)
            .Build<Config>();

        this.Log().Trace($"Configuration loaded from {folder}");

        return new Configuration(
            directory.FullName,
            config.Registry,
            File.Exists(credFile) ? File.ReadAllText(credFile) : string.Empty,
            config.Servers,
            config.Types
        );

        DirectoryInfo? GetConfigurationFolder(DirectoryInfo dir)
        {
            if (File.Exists(GetConfigurationFile(dir.FullName)))
                return dir;

            if (dir.FullName == dir.Root.FullName)
                return null;

            return GetConfigurationFolder(dir.Parent ?? throw new DirectoryNotFoundException($"Directory {dir} has no parent directory"));
        }
    }

    public void Save(Configuration configuration, IReadOnlyCollection<IProject> projects)
    {
        this.Log().Trace($"Save configuration in {configuration.Directory}");
        var cfg = _mapper.Map<Config>(configuration);
        Write(GetConfigurationFile, Yaml.Serializer.Serialize(cfg));
        Write(GetCredentialsFile, configuration.Token);

        // save configuration for each project
        var ignorePatterns = new List<string>
        {
            FileManager.IgnoreFile,
            CredentialsFile
        };
        foreach ((ProjectType type, Uri uri) in configuration.Servers.OrderBy(s => s.Key.ToString()))
        {
            if (!_specialManagers.ContainsKey(type))
            {
                this.Log().Trace($"{type} configuration manager not found");
                continue;
            }

            var targets = projects.Where(p => p.Type.Equals(type)).ToArray();
            if (!targets.Any())
            {
                this.Log().Trace($"No {type} projects discovered to save configuration for");
                continue;
            }

            this.Log().Trace($"Save {type} -> {uri} configuration");
            ignorePatterns.AddRange(_specialManagers[type].IgnorePatterns);
            var typeConfiguration = new ProjectTypeConfiguration(
                uri,
                configuration.Token,
                configuration.Types.FirstOrDefault(c => c.Type.Equals(type))
            );
            foreach (var project in targets)
                _specialManagers[type].Save(project, typeConfiguration);
        }

        this.Log().Trace($"Update ignore file in {configuration.Directory}");
        var ignoreFile = Path.Combine(configuration.Directory, IgnoreFile);

        if (File.Exists(ignoreFile))
        {
            var lines = File.ReadAllLines(ignoreFile).ToList();
            if (lines.IndexOf(IgnoreHeader) >= 0)
            {
                lines = lines.Where(line => !ignorePatterns.Contains(line)).ToList();
                var index = lines.IndexOf(IgnoreHeader);
                lines.InsertRange(index + 1, ignorePatterns);
            }
            else
            {
                lines.Add(string.Empty);
                lines.Add(IgnoreHeader);
                lines.AddRange(ignorePatterns);
            }

            File.WriteAllLines(ignoreFile, lines);
        }
        else
        {
            File.WriteAllLines(ignoreFile, new[] { IgnoreHeader }.Concat(ignorePatterns));
        }

        void Write(Func<string, string> resolve, string data) => File.WriteAllText(resolve(configuration.Directory), data);
    }

    public void Delete(string folder, IReadOnlyCollection<IProject> projects)
    {
        DeleteFile(GetConfigurationFile);
        DeleteFile(GetCredentialsFile);

        foreach (var project in projects)
            if (_specialManagers.ContainsKey(project.Type))
                _specialManagers[project.Type].Delete(project);

        void DeleteFile(Func<string, string> resolveFile)
        {
            var path = resolveFile(folder);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private string GetConfigurationFile(string folder) => Path.Combine(folder, ConfigurationFile);

    private string GetCredentialsFile(string folder) => Path.Combine(folder, CredentialsFile);

    private class Config
    {
        [DataMember(Order = 0)]
        public Uri Registry { get; private set; } = new("http://localhost");

        [DataMember(Order = 1)]
        public Dictionary<ProjectType, Uri> Servers { get; private set; } = new();

        [DataMember(Order = 2)]
        public SpecialConfiguration[] Types { get; private set; } = Array.Empty<SpecialConfiguration>();
    }
}