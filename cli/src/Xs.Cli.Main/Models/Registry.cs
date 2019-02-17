using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Main.Models
{
    public class Registry
    {
        public string Name { get; set; }

        public Uri Location { get; set; }

        public string Token { get; set; }

        public Dictionary<ProjectType, Uri> Servers { get; set; } = new Dictionary<ProjectType, Uri>();

        public override string ToString() => Name;
    }
}