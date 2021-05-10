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
    public class AppReactCommand : Command<AppReactCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "app.react";
        public override string Description { get; } = "Create new app project, using React.";
        private readonly ITemplateWriter _templateWriter;
        private readonly ILogger<AppReactCommand> _logger;

        public AppReactCommand(
            ITemplateWriter templateWriter,
            ILogger<AppReactCommand> logger
        )
        {
            _templateWriter = templateWriter;
            _logger = logger;
        }

        public override void Handle(
            AppReactCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            _logger.Debug($"Create app project {name} at {location}");

            _templateWriter.LoadResources($"{Group.TemplatesDir}.AppReact");
            _templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

            // setup data
            var data = new { name };

            // write files
            _templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
            _templateWriter.WriteAll(data);
            _templateWriter.EnsureAllWritten();
        }
    }

    public class AppReactCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; } = string.Empty;
    }
}