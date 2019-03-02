using System;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Logging;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibCommand : Command<LibCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "lib";

        public override string Description { get; } = "Create new library project.";

        private readonly ILogger logger;

        public LibCommand(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public override void Handle(
            LibCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cwdCfg.Cwd;
            var name = cfg.Name;

            logger.LogDebug($"Create library {name} at {location}");

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var resources = ResourceLoader.Load($"{Group.TemplatesDir}.Lib");

            // create lib folder
            var folder = Path.Combine(location, name);
            Directory.CreateDirectory(folder);

            // write project file
            var template = resources.First(r => r.Name == Group.ProjectTemplate);
            var projectData = template.Content;
            File.WriteAllText(Path.Combine(folder, $"{name}{ProjectFactory.ProjectFileExtension}"), projectData);
        }
    }

    public class LibCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}