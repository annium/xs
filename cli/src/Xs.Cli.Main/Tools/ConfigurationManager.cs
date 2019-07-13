using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Annium.Extensions.Configuration;
using Annium.Extensions.Mapper;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.RegistryClient.Main;

namespace Xs.Cli.Main.Tools
{
    internal class ConfigurationManager : IConfigurationManager
    {
        private const string configurationFile = ".xs";
        private const string credentialsFile = ".xs.credentials";
        private const string ignoreHeader = "# xs ignore patterns";
        private const string ignoreFile = ".gitignore";
        private readonly MainClientFactory mainClientFactory;
        private readonly IReadOnlyDictionary<ProjectType, ISpecialConfigurationManager> specialManagers;
        private readonly ILogger logger;

        public ConfigurationManager(
            MainClientFactory mainClientFactory,
            IEnumerable<ISpecialConfigurationManager> specialManagers,
            ILogger logger
        )
        {
            this.specialManagers = specialManagers.ToDictionary(e => e.Type, e => e);
            this.mainClientFactory = mainClientFactory;
            this.logger = logger;
        }

        public Configuration LoadBarebone(string folder)
        {
            var cfgFile = ConfigurationFile(folder);
            var credFile = CredentialsFile(folder);

            if (!File.Exists(cfgFile))
                return null;

            var config = new ConfigurationBuilder()
                .AddYamlFile(cfgFile)
                .Build<Config>();
            var configuration = new Configuration();
            if (config.Registry != null)
                configuration.SetRegistry(config.Registry);
            configuration.SetTypes(config.Types);
            if (File.Exists(credFile))
                configuration.SetToken(File.ReadAllText(credFile));

            return configuration;
        }

        public async Task<Configuration> LoadAsync(string folder)
        {
            logger.Trace($"Load configuration from {folder}");
            if (!File.Exists(ConfigurationFile(folder)) || !File.Exists(CredentialsFile(folder)))
            {
                logger.Trace($"Configuration or credentials missing in {folder}");
                return null;
            }

            var configuration = LoadBarebone(folder);
            var servers = configuration.Registry.IsFile ?
                ProjectType.List().ToDictionary(type => type, type => configuration.Registry) :
                (await mainClientFactory.Create(configuration.Registry).GetRegistryInfoAsync())
                .OrderBy(s => s.Key)
                .ToDictionary(s => ProjectType.Get(s.Key), s => s.Value);

            configuration.SetServers(servers);

            logger.Trace($"Configuration loaded {folder}");

            return configuration;
        }

        public void Save(string folder, IProject[] projects, Configuration configuration)
        {
            logger.Trace($"Save configuration in {folder}");
            Write(ConfigurationFile, Yaml.Serializer.Serialize(Mapper.Map<Config>(configuration)));
            Write(CredentialsFile, configuration.Token);

            // save configuration for each project
            var ignorePatterns = new List<string>();
            ignorePatterns.Add(FileManager.IgnoreFile);
            ignorePatterns.Add(credentialsFile);
            foreach (var(type, uri) in configuration.Servers.OrderBy(s => s.Key.ToString()))
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
            public Uri Registry { get; set; }

            [DataMember(Order = 1)]
            public SpecialConfiguration[] Types { get; set; } = Array.Empty<SpecialConfiguration>();
        }
    }
}