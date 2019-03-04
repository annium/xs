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
    public class WebCommand : Command<WebCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "web";

        public override string Description { get; } = "Create new web project.";

        private readonly ILogger logger;

        public WebCommand(
            ILogger logger
        )
        {
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

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var resources = ResourceLoader.Load($"{Group.TemplatesDir}.Web");

            // create project folder
            var folder = Path.Combine(location, name);
            Directory.CreateDirectory(folder);

            // setup data
            var data = new { name };

            // write files
            write(Group.ProjectTemplate, $"{name}{ProjectFactory.ProjectFileExtension}");
            write("Program.tpl", "Program.cs");
            write("ServicePack.tpl", "ServicePack.cs");
            write("Startup.tpl", "Startup.cs");
            write("Controllers.IndexController.tpl", Path.Combine("Controllers", "IndexController.cs"));

            void write(string resourceName, string fileName)
            {
                var path = Path.GetFullPath(Path.Combine(folder, fileName));
                Directory.CreateDirectory(Directory.GetParent(path).FullName);

                var tpl = resources.First(r => r.Name == resourceName);
                var content = Template.Parse(tpl.Content).Render(data);

                File.WriteAllText(path, content);
            }
        }
    }

    public class WebCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}