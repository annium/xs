using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node.Commands.New
{
    public class LibReactCommand : Command<LibReactCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "lib.react";

        public override string Description { get; } = "Create new library project, using React.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger logger;

        public LibReactCommand(
            ITemplateWriter templateWriter,
            ILogger logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            LibReactCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create library project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.LibReact");
            templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);
            templateWriter.Write("tsconfig.json.tpl", "tsconfig.json", data);
            templateWriter.Write("tslint.json.tpl", "tslint.json", data);
            templateWriter.Write(".gitignore.tpl", ".gitignore", data);
        }
    }

    public class LibReactCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}