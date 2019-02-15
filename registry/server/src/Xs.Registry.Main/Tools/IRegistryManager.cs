using System;
using System.Collections.Generic;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main.Tools
{
    public interface IRegistryManager
    {
        IReadOnlyDictionary<ProjectType, Uri> GetRegistries();

        void AddRegistry(ProjectType type, Uri uri);
    }
}