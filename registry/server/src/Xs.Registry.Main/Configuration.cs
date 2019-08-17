using System;
using System.Collections.Generic;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main
{
    internal class Configuration
    {
        public IReadOnlyDictionary<ProjectType, Uri> Servers { get; set; }
    }

    internal class RawConfiguration
    {
        public Dictionary<string, Uri> Servers { get; set; }
    }
}