using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node.Commands.New
{
    public class LibCommand : Command<LibCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "lib";

        public override string Description { get; } = "Create new library project.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger<LibCommand> logger;

        public LibCommand(
            ITemplateWriter templateWriter,
            ILogger<LibCommand> logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            LibCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create library project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.Lib");
            templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
            templateWriter.WriteAll(data);
            templateWriter.EnsureAllWritten();
        }
    }

    public class LibCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}