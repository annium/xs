using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class ExeCommand : Command<ExeCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "exe";

        public override string Description { get; } = "Create new exe project.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger logger;

        public ExeCommand(
            ITemplateWriter templateWriter,
            ILogger logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            ExeCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cwdCfg.Cwd;
            var name = cfg.Name;

            logger.Debug($"Create executable project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.Exe");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.Write("Program.tpl", "Program.cs", data);
            templateWriter.Write("ServicePack.tpl", "ServicePack.cs", data);
        }
    }

    public class ExeCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}