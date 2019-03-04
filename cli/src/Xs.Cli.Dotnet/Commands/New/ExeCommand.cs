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
    public class ExeCommand : Command<ExeCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "exe";

        public override string Description { get; } = "Create new exe project.";

        private readonly ILogger logger;

        public ExeCommand(
            ILogger logger
        )
        {
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

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var resources = ResourceLoader.Load($"{Group.TemplatesDir}.Exe");

            // create project folder
            var folder = Path.Combine(location, name);
            Directory.CreateDirectory(folder);

            // setup data
            var data = new { name };

            // write files
            write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}");
            write("Program.tpl", "Program.cs");
            write("ServicePack.tpl", "ServicePack.cs");

            void write(string resourceName, string fileName)
            {
                var tpl = resources.First(r => r.Name == resourceName);
                var content = Template.Parse(tpl.Content).Render(data);
                File.WriteAllText(Path.Combine(folder, fileName), content);
            }
        }
    }

    public class ExeCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}