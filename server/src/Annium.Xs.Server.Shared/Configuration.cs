using System;
using System.Collections.Generic;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Shared;

public sealed record Configuration
{
    public Dictionary<ProjectType, Uri> Servers { get; set; } = new();
}
