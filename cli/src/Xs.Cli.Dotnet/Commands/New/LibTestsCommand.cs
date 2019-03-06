using System.IO;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibTestsCommand : Command<LibTestsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "libtests";

        public override string Description { get; } = "Create new tests project.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger logger;

        public LibTestsCommand(
            ITemplateWriter templateWriter,
            ILogger logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            LibTestsCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var location = cwdCfg.Cwd;
            var name = $"{cfg.Name}.Tests";

            logger.Debug($"Create library tests project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.LibTests");
            templateWriter.SetRoot(Path.Combine(location, name));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}", data);
            templateWriter.Write("SampleTest.tpl", "SampleTest.cs", data);
        }
    }

    public class LibTestsCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}