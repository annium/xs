using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Commands.New
{
    public class ClassCommand : Command<ClassCommandConfiguration, DiscoverConfiguration>
    {
        private const string ClassTemplate = "Class.cs_tpl";

        public override string Id { get; } = "class";
        public override string Description { get; } = "Create new classes.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ITemplateWriter templateWriter;
        private readonly ILogger<ClassCommand> logger;

        public ClassCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<ClassCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            ClassCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var output = Path.GetFullPath(Path.Combine(discoverCfg.Root, cfg.Output));
            var project = discoverTask.Run(discoverCfg)
                .FirstOrDefault(p => output.StartsWith(p.Directory));

            if (project is null)
            {
                Console.Write("Can't determine project, class will belong to");
                return;
            }

            var names = new List<string>();
            while (true)
            {
                Console.Write("Class name?: ");
                var name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                    break;

                names.Add(name);
            }

            if (names.Count == 0)
                return;

            logger.Debug($"{names.Count} class(es) to create");

            Directory.CreateDirectory(output);

            foreach (var name in names)
            {
                logger.Debug($"Create class {name} at {output}");

                templateWriter.LoadResources($"{Group.TemplatesDir}.Class");
                templateWriter.SetRoot(output);

                // setup data
                var ns = $"{project.Name}.{Path.GetRelativePath(project.Directory, output).Replace(Path.DirectorySeparatorChar, '.')}";
                var data = new { ns, name };

                // write files
                templateWriter.Write(ClassTemplate, $"{name}.cs", data);
                templateWriter.EnsureAllWritten();
            }
        }
    }

    public class ClassCommandConfiguration
    {
        [Option("o", isRequired: true)]
        [Help("Output directory.")]
        public string Output { get; set; } = string.Empty;
    }
}