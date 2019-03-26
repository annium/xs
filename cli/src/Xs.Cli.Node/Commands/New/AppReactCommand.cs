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
    public class AppReactCommand : Command<AppReactCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "app.react";

        public override string Description { get; } = "Create new app project, using React.";

        private readonly ITemplateWriter templateWriter;

        private readonly ILogger logger;

        public AppReactCommand(
            ITemplateWriter templateWriter,
            ILogger logger
        )
        {
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            AppReactCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var location = discoverCfg.Root;
            var name = cfg.Name;

            logger.Debug($"Create app project {name} at {location}");

            templateWriter.LoadResources($"{Group.TemplatesDir}.AppReact");
            templateWriter.SetRoot(Path.Combine(location, PackageName.GetPlainName(name)));

            // setup data
            var data = new { name };

            // write files
            templateWriter.Write(Group.ProjectTemplate, ProjectFactory.ProjectFileName, data);

            templateWriter.Copy("public.favicon.ico", Path.Combine("public", "favicon.ico"));
            templateWriter.Write("public.index.html.tpl", Path.Combine("public", "index.html"), data);
            templateWriter.Write("public.manifest.json.tpl", Path.Combine("public", "manifest.json"), data);

            templateWriter.Write("src.App.tsx.tpl", Path.Combine("src", "App.tsx"), data);
            templateWriter.Write("src.index.tsx.tpl", Path.Combine("src", "index.tsx"), data);
            templateWriter.Write("src.react-app-env.d.ts.tpl", Path.Combine("src", "react-app-env.d.ts"), data);

            templateWriter.Write("tsconfig.json.tpl", "tsconfig.json", data);
            templateWriter.Write("tslint.json.tpl", "tslint.json", data);
            templateWriter.Write(".gitignore.tpl", ".gitignore", data);
        }
    }

    public class AppReactCommandConfiguration
    {
        [Position(1)]
        [Help("Project name.")]
        public string Name { get; set; }
    }
}