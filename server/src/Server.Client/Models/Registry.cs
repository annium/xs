using System;
using System.Collections.Generic;

namespace Server.Client.Models;

public sealed record Registry
{
    public Dictionary<string, Uri> Servers { get; init; } = new();
}