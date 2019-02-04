using System;
using System.Collections.Generic;
using Xs.Core.Models;

namespace Xs.Registry.Main.Tools
{
    public interface IRegistryManager
    {
        IReadOnlyDictionary<ProjectType, Uri> GetRegistries();

        void AddRegistry(ProjectType type, Uri uri);
    }
}