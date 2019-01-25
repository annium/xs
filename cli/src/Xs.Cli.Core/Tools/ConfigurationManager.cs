using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Extensions.Configuration;
using Xs.Cli.Core.Models;
using Xs.Core.Helpers;
using Xs.Core.Models;

namespace Xs.Cli.Core.Tools
{
    internal class ConfigurationManager : IConfigurationManager
    {
        private const string file = ".xs";

        private readonly IReadOnlyDictionary<ProjectType, ISpecialConfigurationManager> specialManagers;

        public ConfigurationManager(
            IEnumerable<ISpecialConfigurationManager> specialManagers
        )
        {
            this.specialManagers = specialManagers.ToDictionary(e => e.Type, e => e);
        }

        public Configuration Load(string folder)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(FilePath(folder), optional : true)
                .Build<Configuration>();
        }

        public void Save(string folder, Configuration configuration)
        {
            Json.WriteFile(FilePath(folder), configuration);

            // prepare lists of registries by types
            var typeRegistries = specialManagers.ToDictionary(
                e => e.Key,
                _ => new List<ValueTuple<string, Uri, string>>()
            );

            // add registries of supported types
            foreach (var registry in configuration.Registries)
                foreach (var(type, uri) in registry.Servers)
                    if (typeRegistries.ContainsKey(type))
                        typeRegistries[type].Add((registry.Name, uri, registry.Token));

            // save registries of each type
            foreach (var(type, registries) in typeRegistries)
                specialManagers[type].Save(folder, registries);
        }

        private string FilePath(string folder) => Path.Combine(folder, file);
    }
}