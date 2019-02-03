using System;
using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Core.Tools
{
    internal class RegistryManager : IRegistryManager
    {
        private Dictionary<ProjectType, Uri> registries = new Dictionary<ProjectType, Uri>();

        public void AddRegistry(ProjectType type, Uri uri)
        {
            registries[type] = uri;
        }

        public IReadOnlyDictionary<ProjectType, Uri> GetRegistries()
        {
            return registries;
        }
    }
}