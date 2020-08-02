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
using Xs.Cli.Core.Tools;

namespace Xs.Tools
{
    internal class ConfigurationManager : IConfigurationManager
    {
        private const string ConfigurationFile = ".xs";
        private const string CredentialsFile = ".xs.credentials";
        private const string IgnoreHeader = "# xs ignore patterns";
        private const string IgnoreFile = ".gitignore";
        private readonly IReadOnlyDictionary<ProjectType, ISpecialConfigurationManager> _specialManagers;
        private readonly Func<IConfigurationBuilder> _configurationBuilderFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<ConfigurationManager> _logger;

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
            _logger = logger;
        }

        public Configuration Load(string folder)
        {
            _logger.Trace($"Load configuration from {folder}");

            var cfgFile = GetConfigurationFile(folder);
            var credFile = GetCredentialsFile(folder);

            if (!File.Exists(cfgFile))
            {
                _logger.Trace($"Configuration missing in {folder}. Returning default");
                return Configuration.Empty();
            }

            var config = _configurationBuilderFactory()
                .AddYamlFile(cfgFile)
                .Build<Config>();

            _logger.Trace($"Configuration loaded from {folder}");

            return new Configuration(
                config.Registry,
                File.Exists(credFile) ? File.ReadAllText(credFile) : string.Empty,
                config.Servers,
                config.Types
            );
        }

        public void Save(string folder, IProject[] projects, Configuration configuration)
        {
            _logger.Trace($"Save configuration in {folder}");
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
                    _logger.Trace($"{type} configuration manager not found");
                    continue;
                }

                var targets = projects.Where(p => p.Type == type).ToArray();
                if (!targets.Any())
                {
                    _logger.Trace($"No {type} projects discovered to save configuration for");
                    continue;
                }

                _logger.Trace($"Save {type} -> {uri} configuration");
                ignorePatterns.AddRange(_specialManagers[type].IgnorePatterns);
                var typeConfiguration = new ProjectTypeConfiguration(
                    uri,
                    configuration.Token,
                    configuration.Types.FirstOrDefault(c => c.Type == type)
                );
                foreach (var project in targets)
                    _specialManagers[type].Save(project, typeConfiguration);
            }

            _logger.Trace($"Update ignore file in {folder}");
            var ignoreFile = Path.Combine(folder, IgnoreFile);

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

            void Write(Func<string, string> resolve, string data) => File.WriteAllText(resolve(folder), data);
        }

        public void Delete(string folder, IProject[] projects)
        {
            Delete(GetConfigurationFile);
            Delete(GetCredentialsFile);

            foreach (var project in projects)
                if (_specialManagers.ContainsKey(project.Type))
                    _specialManagers[project.Type].Delete(project);

            void Delete(Func<string, string> resolve)
            {
                var path = resolve(folder);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private string GetConfigurationFile(string folder) => Path.Combine(folder, ConfigurationFile);

        private string GetCredentialsFile(string folder) => Path.Combine(folder, CredentialsFile);

        private class Config
        {
            [DataMember(Order = 0)] public Uri Registry { get; private set; } = new Uri("http://localhost");
            [DataMember(Order = 1)] public Dictionary<ProjectType, Uri> Servers { get; private set; } = new Dictionary<ProjectType, Uri>();
            [DataMember(Order = 2)] public SpecialConfiguration[] Types { get; private set; } = Array.Empty<SpecialConfiguration>();
        }
    }
}