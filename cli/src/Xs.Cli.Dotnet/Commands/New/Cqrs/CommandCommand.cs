using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;
using CommandLine = Annium.Extensions.CommandLine.Cli;
using static Xs.Cli.Dotnet.Commands.New.Cqrs.Helper;

namespace Xs.Cli.Dotnet.Commands.New.Cqrs
{
    internal class CommandCommand : Command<CommandCommandConfiguration, DiscoverConfiguration>
    {
        internal const string CommandTemplate = "Command.cs_tpl";
        internal const string RequestTemplate = "Request.cs_tpl";
        internal const string Commands = "Commands";
        internal const string Requests = "Requests";

        public override string Id { get; } = "command";
        public override string Description { get; } = "Create command.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ITemplateWriter _templateWriter;
        private readonly ILogger<CommandCommand> _logger;

        public CommandCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<CommandCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _templateWriter = templateWriter;
            _logger = logger;
        }

        public override void Handle(
            CommandCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = _discoverTask.Run(discoverCfg).ToList();

            var applicationProject = projects.FilterMask(cfg.ApplicationProject).SingleOrDefault();
            if (applicationProject is null)
            {
                Console.WriteLine($"Application project {cfg.ApplicationProject} not found");
                return;
            }

            var viewModelProject = projects.FilterMask(cfg.ViewModelProject).SingleOrDefault();
            if (viewModelProject is null)
            {
                Console.WriteLine($"ViewModel project {cfg.ViewModelProject} not found");
                return;
            }

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Command");

            var data = GetCommandDescription(applicationProject, viewModelProject, cfg.Area, token);

            _logger.Debug($"Create command {data.Entity}:{data.Name}");

            // write files
            _templateWriter.SetRoot(BuildPath(applicationProject.Directory, cfg.Area, Commands, data.Entity));
            _templateWriter.Write(CommandTemplate, $"{data.Name}Command.cs", data);
            _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Requests, data.Entity));
            _templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
            _templateWriter.EnsureAllWritten();
        }

        private CommandDescription GetCommandDescription(
            IProject applicationProject,
            IProject viewModelProject,
            string? area,
            CancellationToken token
        )
        {
            var data = new CommandDescription
            {
                Entity = CommandLine.Prompt("Entities: "),
            };
            token.ThrowIfCancellationRequested();
            data.Name = CommandLine.Prompt("Command name: ");
            token.ThrowIfCancellationRequested();
            data.RequestFields = PromptFields("Request field");
            token.ThrowIfCancellationRequested();
            data.ComposeFields = PromptFields("Compose field");
            token.ThrowIfCancellationRequested();
            data.CommandNamespace = BuildNamespace(applicationProject.Name, area, Commands, data.Entity);
            data.RequestNamespace = BuildNamespace(viewModelProject.Name, area, Requests, data.Entity);

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
        [Option("app")]
        [Help("Application layer to add command to.")]
        public string ApplicationProject { get; set; } = "Application";

        [Option("view")]
        [Help("View model layer to add request to.")]
        public string ViewModelProject { get; set; } = "ViewModel";

        [Option("area")]
        [Help("Optional area to generate within.")]
        public string? Area { get; set; }
    }
}