using System;
using System.Collections.Generic;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Models
{
    public class Configuration
    {
        public Uri Registry { get; private set; }
        public string Token { get; private set; }
        public IReadOnlyDictionary<ProjectType, Uri> Servers { get; private set; } = new Dictionary<ProjectType, Uri>();
        public SpecialConfiguration[] Types { get; private set; } = Array.Empty<SpecialConfiguration>();

        public Configuration()
        {

        }

        public void SetRegistry(Uri registry)
        {
            Registry = registry ??
                throw new ArgumentNullException(nameof(registry));
        }

        public void SetToken(string token)
        {
            Token = token ??
                throw new ArgumentNullException(nameof(token));
        }

        public void SetServers(IReadOnlyDictionary<ProjectType, Uri> servers)
        {
            Servers = servers ??
                throw new ArgumentNullException(nameof(servers));
        }

        public void SetTypes(SpecialConfiguration[] types)
        {
            Types = types ??
                throw new ArgumentNullException(nameof(types));
        }
    }
}