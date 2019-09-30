using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class ExeCommand : Command<ExeCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "exe";
        public override string Description { get; } = "Create new exe project.";
        private readonly ITemplateWriter templateWriter;
        private readonly ILogger<ExeCommand> logger;

        public ExeCommand(
            ITemplateWriter templateWriter,
            ILogger<ExeCommand> logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            ExeCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create executable project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.Exe");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.WriteAll(data);
            templateWriter.EnsureAllWritten();
        }
    }

    public class ExeCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; } = string.Empty;
    }
}