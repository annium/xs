using System;
using System.Collections.Generic;

namespace Server.Client.Models;

public class Registry
{
    public Dictionary<string, Uri> Servers { get; set; } = new();
}