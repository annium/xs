using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class WebTestsCommand : Command<WebTestsCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "web.tests";
        public override string Description { get; } = "Create new web tests project.";
        private readonly ITemplateWriter templateWriter;
        private readonly ILogger<WebTestsCommand> logger;

        public WebTestsCommand(
            ITemplateWriter templateWriter,
            ILogger<WebTestsCommand> logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            WebTestsCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create web tests project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.WebTests");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.WriteAll(data);
            templateWriter.EnsureAllWritten();
        }
    }

    public class WebTestsCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}