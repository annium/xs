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
    public class LibReactCommand : Command<LibReactCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "lib.react";
        public override string Description { get; } = "Create new library project, using React.";
        private readonly ITemplateWriter _templateWriter;
        private readonly ILogger<LibReactCommand> _logger;

        public LibReactCommand(
            ITemplateWriter templateWriter,
            ILogger<LibReactCommand> logger
        )
        {
            _templateWriter = templateWriter;
            _logger = logger;
        }

        public override void Handle(
            LibReactCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            _logger.Debug($"Create library project {name} at {location}");

            _templateWriter.LoadResources($"{Group.TemplatesDir}.LibReact");
            _templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

            // setup data
            var data = new { name };

            // write files
            _templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
            _templateWriter.WriteAll(data);
            _templateWriter.EnsureAllWritten();
        }
    }

    public class LibReactCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; } = string.Empty;
    }
}