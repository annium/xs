using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibCommand : Command<LibCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "lib";
        public override string Description { get; } = "Create new library project.";
        private readonly ITemplateWriter _templateWriter;
        private readonly ILogger<LibCommand> _logger;

        public LibCommand(
            ITemplateWriter templateWriter,
            ILogger<LibCommand> logger
        )
        {
            _templateWriter = templateWriter;
            _logger = logger;
        }

        public override void Handle(
            LibCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            _logger.Debug($"Create library project {name} at {location}");

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Lib");
            _templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            _templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            _templateWriter.WriteAll(data);
            _templateWriter.EnsureAllWritten();
        }
    }

    public class LibCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; } = string.Empty;
    }
}