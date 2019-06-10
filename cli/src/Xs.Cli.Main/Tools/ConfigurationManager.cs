using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            if (!File.Exists(cfgFile))
                return null;

            var configuration = new Configuration();
            configuration.Location = new Uri(File.ReadAllText(cfgFile));

            return configuration;
        }

        public async Task<Configuration> LoadAsync(string folder)
        {
            var cfgFile = ConfigurationFile(folder);

            logger.Trace($"Load configuration from {folder}");
            if (!File.Exists(cfgFile) || !File.Exists(CredentialsFile(folder)))
            {
                logger.Trace($"Configuration or credentials missing in {folder}");
                return null;
            }

            var configuration = new Configuration();
            configuration.Location = new Uri(File.ReadAllText(cfgFile));
            configuration.Token = File.ReadAllText(CredentialsFile(folder));
            if (configuration.Location.IsFile)
                configuration.Servers = ProjectType.List().ToDictionary(type => type, type => configuration.Location);
            else
                configuration.Servers = (await mainClientFactory.Create(configuration.Location).GetRegistryInfoAsync())
                .OrderBy(s => s.Key)
                .ToDictionary(s => ProjectType.Get(s.Key), s => s.Value);

            logger.Trace($"Configuration loaded {folder}");

            return configuration;
        }

        public void Save(string folder, IProject[] projects, Configuration configuration)
        {
            logger.Trace($"Save configuration in {folder}");
            Write(ConfigurationFile, configuration.Location.ToString());
            Write(CredentialsFile, configuration.Token);

            // save configuration for each project
            var ignorePatterns = new List<string>();
            ignorePatterns.Add(FileManager.IgnoreFile);
            ignorePatterns.Add(credentialsFile);
            foreach (var(type, uri) in configuration.Servers.OrderBy(s => s.Key.ToString()))
            {
                if (specialManagers.ContainsKey(type))
                {
                    var targets = projects.Where(p => p.Type == type);
                    if (targets.Count() > 0)
                    {
                        logger.Trace($"Save {type} -> {uri} configuration");
                        ignorePatterns.AddRange(specialManagers[type].IgnorePatterns);
                        foreach (var project in targets)
                            specialManagers[type].Save(project, uri, configuration.Token);
                    }
                    else
                        logger.Trace($"No {type} projects discovered to save configuration for");
                }
                else
                    logger.Trace($"{type} configuration manager not found");
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
    }
}