using System.Collections.Generic;

namespace Xs.Cli.Core.Models
{
    public class Configuration
    {
        public List<Registry> Registries { get; set; } = new List<Registry>();
    }
}