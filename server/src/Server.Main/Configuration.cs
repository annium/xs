using System;
using System.Collections.Generic;
using Server.Domain.Models;

namespace Server.Main;

public sealed record Configuration
{
    public Dictionary<ProjectType, Uri> Servers { get; set; } = new();
}