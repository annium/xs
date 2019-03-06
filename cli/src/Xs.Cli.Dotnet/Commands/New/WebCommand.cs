using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class WebCommand : Command<WebCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "web";

        public override string Description { get; } = "Create new web project.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger logger;

        public WebCommand(
            ITemplateWriter templateWriter,
            ILogger logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            WebCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cwdCfg.Cwd;
            var name = cfg.Name;

            logger.Debug($"Create web project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.Web");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.Write("Program.tpl", "Program.cs", data);
            templateWriter.Write("ServicePack.tpl", "ServicePack.cs", data);
            templateWriter.Write("Startup.tpl", "Startup.cs", data);
            templateWriter.Write("Controllers.IndexController.tpl", Path.Combine("Controllers", "IndexController.cs"), data);
        }
    }

    public class WebCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}