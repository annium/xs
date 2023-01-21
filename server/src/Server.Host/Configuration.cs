using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Server.Domain.Models;

namespace Server.Host;

public sealed record Configuration
{
    public WebHostConfiguration Host { get; init; } = new();
    public Dictionary<ProjectType, Uri> Servers { get; set; } = new();
}