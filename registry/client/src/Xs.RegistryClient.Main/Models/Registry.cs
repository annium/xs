using System;
using System.Collections.Generic;

namespace Xs.RegistryClient.Main.Models
{
    public class Registry
    {
        public Dictionary<string, Uri> Servers { get; set; } = new Dictionary<string, Uri>();
    }
}