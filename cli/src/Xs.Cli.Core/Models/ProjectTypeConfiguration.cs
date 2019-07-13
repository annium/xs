using System;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Models
{
    public class ProjectTypeConfiguration
    {
        public Uri Server { get; }
        public string Token { get; }
        public SpecialConfiguration Special { get; }

        public ProjectTypeConfiguration(
            Uri server,
            string token,
            SpecialConfiguration special
        )
        {
            Server = server;
            Token = token;
            Special = special;
        }
    }
}