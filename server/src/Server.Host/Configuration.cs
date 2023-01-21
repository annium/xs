using System;
using System.Collections.Generic;
using Server.Db.Shared.Models;

namespace Server.Host;

public class Configuration
{
    public IReadOnlyDictionary<ProjectType, Uri> Servers { get; set; } = new Dictionary<ProjectType, Uri>();
}

internal class RawConfiguration
{
    public Dictionary<string, Uri> Servers { get; } = new();
}