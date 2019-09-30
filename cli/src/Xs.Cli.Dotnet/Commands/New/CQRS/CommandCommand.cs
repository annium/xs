using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;
using CommandLine = Annium.Extensions.CommandLine.Cli;

namespace Xs.Cli.Dotnet.Commands.New.CQRS
{
    internal class CommandCommand : Command<CommandCommandConfiguration, DiscoverConfiguration>
    {
        internal const string CommandTemplate = "Command.cs_tpl";
        internal const string RequestTemplate = "Request.cs_tpl";
        internal const string Commands = "Commands";
        internal const string Requests = "Requests";

        public override string Id { get; } = "command";
        public override string Description { get; } = "Create command.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ITemplateWriter templateWriter;
        private readonly ILogger<CommandCommand> logger;

        public CommandCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<CommandCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            CommandCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg).ToList();

            var applicationProject = projects.FilterMask(cfg.ApplicationProject).FirstOrDefault();
            if (applicationProject is null)
            {
                Console.WriteLine($"Application project {cfg.ApplicationProject} not found");
                return;
            }

            var viewModelProject = projects.FilterMask(cfg.ViewModelProject).FirstOrDefault();
            if (viewModelProject is null)
            {
                Console.WriteLine($"ViewModel project {cfg.ViewModelProject} not found");
                return;
            }

            templateWriter.LoadResources($"{Group.TemplatesDir}.Command");

            var data = GetCommandDescription(applicationProject, viewModelProject, token);

            logger.Debug($"Create command {data.Entity}:{data.Name}");

            // write files
            templateWriter.SetRoot(Path.Combine(applicationProject.Directory, Commands, data.Entity));
            templateWriter.Write(CommandTemplate, $"{data.Name}Command.cs", data);
            templateWriter.SetRoot(Path.Combine(viewModelProject.Directory, data.Entity, Requests));
            templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
            templateWriter.EnsureAllWritten();
        }

        private CommandDescription GetCommandDescription(
            IProject applicationProject,
            IProject viewModelProject,
            CancellationToken token
        )
        {
            var data = new CommandDescription
            {
                Entity = CommandLine.Prompt("Entities: ")
            };
            token.ThrowIfCancellationRequested();
            data.Name = CommandLine.Prompt("Command name: ");
            token.ThrowIfCancellationRequested();
            data.RequestFields = Helper.PromptFields("Request field");
            token.ThrowIfCancellationRequested();
            data.ComposeFields = Helper.PromptFields("Compose field");
            token.ThrowIfCancellationRequested();
            data.CommandNamespace = $"{applicationProject.Name}.{Commands}.{data.Entity}";
            data.RequestNamespace = $"{viewModelProject.Name}.{data.Entity}.{Requests}";

            return data;
        }

        private class CommandDescription
        {
            public string Entity { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public IList<ValueTuple<string, string>> RequestFields { get; set; } = new List<ValueTuple<string, string>>();
            public IList<ValueTuple<string, string>> ComposeFields { get; set; } = new List<ValueTuple<string, string>>();
            public string CommandNamespace { get; set; } = string.Empty;
            public string RequestNamespace { get; set; } = string.Empty;
        }
    }

    internal class CommandCommandConfiguration
    {
        [Option("a")]
        [Help("Application layer to add command to.")]
        public string ApplicationProject { get; set; } = "Application";

        [Option("v")]
        [Help("View model layer to add request/response to.")]
        public string ViewModelProject { get; set; } = "ViewModel";
    }
}