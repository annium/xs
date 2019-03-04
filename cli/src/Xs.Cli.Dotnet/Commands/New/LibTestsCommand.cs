using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Scriban;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Logging;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class LibTestsCommand : Command<LibTestsCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "libtests";

        public override string Description { get; } = "Create new tests project.";

        private readonly ILogger logger;

        public LibTestsCommand(
            ILogger logger
        )
        {
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

            logger.Debug($"Create tests {name} at {location}");

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var resources = ResourceLoader.Load($"{Group.TemplatesDir}.LibTests");

            // create tests folder
            var folder = Path.Combine(location, name);
            Directory.CreateDirectory(folder);

            // setup data
            var data = new { name };

            // write files
            write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}");
            write("SampleTest.tpl", "SampleTest.cs");

            void write(string resourceName, string fileName)
            {
                var tpl = resources.First(r => r.Name == resourceName);
                var content = Template.Parse(tpl.Content).Render(data);
                File.WriteAllText(Path.Combine(folder, fileName), content);
            }
        }
    }

    public class LibTestsCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}