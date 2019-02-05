using System;
using System.Collections.Generic;

namespace Xs.Cli.Core.Models
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