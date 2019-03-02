using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibCommand : Command<LibCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "lib";

        public override string Description { get; } = "Create new library project.";

        public override void Handle(
            LibCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cwdCfg.Cwd;
            var name = cfg.Name;

            Console.WriteLine($"Create library {name} at {location}");
        }
    }

    public class LibCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}