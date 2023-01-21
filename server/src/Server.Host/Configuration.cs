using System;
using System.Collections.Generic;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Main;

public class Configuration
{
    public IReadOnlyDictionary<ProjectType, Uri> Servers { get; set; } = new Dictionary<ProjectType, Uri>();
}

internal class RawConfiguration
{
    public Dictionary<string, Uri> Servers { get; } = new();
}