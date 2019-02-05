using System;
using System.Collections.Generic;


namespace Xs.Registry.Core.Tools
{
    public interface IRegistryManager
    {
        IReadOnlyDictionary<ProjectType, Uri> GetRegistries();

        void AddRegistry(ProjectType type, Uri uri);
    }
}