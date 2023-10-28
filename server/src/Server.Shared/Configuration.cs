using System;
using System.Collections.Generic;
using Server.Shared.Domain.Models;

namespace Server.Shared;

public sealed record Configuration
{
    public Dictionary<ProjectType, Uri> Servers { get; set; } = new();
}
