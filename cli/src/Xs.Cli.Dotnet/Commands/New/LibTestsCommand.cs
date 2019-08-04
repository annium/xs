using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibTestsCommand : Command<LibTestsCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "lib.tests";

        public override string Description { get; } = "Create new library tests project.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger<LibTestsCommand> logger;

        public LibTestsCommand(
            ITemplateWriter templateWriter,
            ILogger<LibTestsCommand> logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            LibTestsCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create library tests project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.LibTests");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.WriteAll(data);
            templateWriter.EnsureAllWritten();
        }
    }

    public class LibTestsCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}