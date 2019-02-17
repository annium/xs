using System.Collections.Generic;

namespace Xs.Cli.Main.Models
{
    public class Configuration
    {
        public List<Registry> Registries { get; set; } = new List<Registry>();
    }
}