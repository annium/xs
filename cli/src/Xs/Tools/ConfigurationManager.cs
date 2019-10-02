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
        private const string configurationFile = ".xs";
        private const string credentialsFile = ".xs.credentials";
        private const string ignoreHeader = "# xs ignore patterns";
        private const string ignoreFile = ".gitignore";
        private readonly IReadOnlyDictionary<ProjectType, ISpecialConfigurationManager> specialManagers;
        private readonly ILogger<ConfigurationManager> logger;

        public ConfigurationManager(
            IEnumerable<ISpecialConfigurationManager> specialManagers,
            ILogger<ConfigurationManager> logger
        )
        {
            this.specialManagers = specialManagers.ToDictionary(e => e.Type, e => e);
            this.logger = logger;
        }

        public Configuration Load(string folder)
        {
            logger.Trace($"Load configuration from {folder}");

            var cfgFile = ConfigurationFile(folder);
            var credFile = CredentialsFile(folder);

            if (!File.Exists(cfgFile))
            {
                logger.Trace($"Configuration missing in {folder}. Returning default");
                return Configuration.Empty();
            }
            var config = new ConfigurationBuilder()
                .AddYamlFile(cfgFile)
                .Build<Config>();

            logger.Trace($"Configuration loaded from {folder}");

            return new Configuration(
                config.Registry,
                File.Exists(credFile) ? File.ReadAllText(credFile) : string.Empty,
                config.Servers,
                config.Types
            );
        }

        public void Save(string folder, IProject[] projects, Configuration configuration)
        {
            logger.Trace($"Save configuration in {folder}");
            Write(ConfigurationFile, Yaml.Serializer.Serialize(Mapper.Map<Config>(configuration)));
            Write(CredentialsFile, configuration.Token);

            // save configuration for each project
            var ignorePatterns = new List<string>
            {
                FileManager.IgnoreFile,
                credentialsFile
            };
            foreach ((ProjectType type, Uri uri) in configuration.Servers.OrderBy(s => s.Key.ToString()))
            {
                if (!specialManagers.ContainsKey(type))
                {
                    logger.Trace($"{type} configuration manager not found");
                    continue;
                }

                var targets = projects.Where(p => p.Type == type);
                if (targets.Count() == 0)
                {
                    logger.Trace($"No {type} projects discovered to save configuration for");
                    continue;
                }

                logger.Trace($"Save {type} -> {uri} configuration");
                ignorePatterns.AddRange(specialManagers[type].IgnorePatterns);
                var typeConfiguration = new ProjectTypeConfiguration(
                    uri,
                    configuration.Token,
                    configuration.Types.FirstOrDefault(c => c.Type == type)
                );
                foreach (var project in targets)
                    specialManagers[type].Save(project, typeConfiguration);
            }

            logger.Trace($"Update ignore file in {folder}");
            var ignoreFile = Path.Combine(folder, ConfigurationManager.ignoreFile);

            if (File.Exists(ignoreFile))
            {
                var lines = File.ReadAllLines(ignoreFile).ToList();
                if (lines.IndexOf(ignoreHeader) >= 0)
                {
                    lines = lines.Where(line => !ignorePatterns.Contains(line)).ToList();
                    var index = lines.IndexOf(ignoreHeader);
                    lines.InsertRange(index + 1, ignorePatterns);
                }
                else
                {
                    lines.Add(string.Empty);
                    lines.Add(ignoreHeader);
                    lines.AddRange(ignorePatterns);
                }

                File.WriteAllLines(ignoreFile, lines);
            }
            else
            {
                File.WriteAllLines(ignoreFile, new [] { ignoreHeader }.Concat(ignorePatterns));
            }

            void Write(Func<string, string> resolve, string data) => File.WriteAllText(resolve(folder), data);
        }

        public void Delete(string folder, IProject[] projects)
        {
            Delete(ConfigurationFile);
            Delete(CredentialsFile);

            foreach (var project in projects)
                if (specialManagers.ContainsKey(project.Type))
                    specialManagers[project.Type].Delete(project);

            void Delete(Func<string, string> resolve)
            {
                var path = resolve(folder);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private string ConfigurationFile(string folder) => Path.Combine(folder, configurationFile);

        private string CredentialsFile(string folder) => Path.Combine(folder, credentialsFile);

        private class Config
        {
            [DataMember(Order = 0)]
            public Uri Registry { get; private set; } = new Uri("http://localhost");
            [DataMember(Order = 1)]
            public Dictionary<ProjectType, Uri> Servers { get; private set; } = new Dictionary<ProjectType, Uri>();
            [DataMember(Order = 2)]
            public SpecialConfiguration[] Types { get; private set; } = Array.Empty<SpecialConfiguration>();
        }
    }
}