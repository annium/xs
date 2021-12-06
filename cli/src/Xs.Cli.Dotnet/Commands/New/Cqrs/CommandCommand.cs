using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tools;
using CommandLine = Annium.Extensions.CommandLine.Cli;
using static Xs.Cli.Dotnet.Commands.New.Cqrs.Helper;

namespace Xs.Cli.Dotnet.Commands.New.Cqrs;

internal class CommandCommand : Command<CommandCommandConfiguration, DiscoverConfiguration>, ILogSubject
{
    private const string DomainCommandTemplate = "DomainCommand.cs_tpl";
    private const string ApplicationCommandTemplate = "ApplicationCommand.cs_tpl";
    private const string RequestTemplate = "Request.cs_tpl";
    private const string Commands = "Commands";
    private const string Requests = "Requests";

    public override string Id => "command";
    public override string Description => "Create command.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly ITemplateWriter _templateWriter;

    public CommandCommand(
        DiscoverProjectsTask discoverTask,
        ITemplateWriter templateWriter,
        ILogger<CommandCommand> logger
    )
    {
        _discoverTask = discoverTask;
        _templateWriter = templateWriter;
        Logger = logger;
    }

    public override void Handle(
        CommandCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var projects = _discoverTask.RunAsync(discoverCfg).Await().ToList();

        var domainProject = projects.FilterMask(cfg.DomainProject).SingleOrDefault();
        if (domainProject is null)
        {
            Console.WriteLine($"Domain project {cfg.DomainProject} not found");
            return;
        }

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

        var data = GetCommandDescription(domainProject, applicationProject, viewModelProject, cfg.Area, ct);

        this.Log().Debug($"Create command {data.Entity}:{data.Name}");

        // write files
        _templateWriter.SetRoot(BuildPath(domainProject.Directory, cfg.Area, Commands, data.Entity));
        _templateWriter.Write(DomainCommandTemplate, $"{data.Name}Command.cs", data);
        _templateWriter.SetRoot(BuildPath(applicationProject.Directory, cfg.Area, Commands, data.Entity));
        _templateWriter.Write(ApplicationCommandTemplate, $"{data.Name}Command.cs", data);
        _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Requests, data.Entity));
        _templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
        _templateWriter.EnsureAllWritten();
    }

    private CommandDescription GetCommandDescription(
        IProject domainProject,
        IProject applicationProject,
        IProject viewModelProject,
        string? area,
        CancellationToken ct
    )
    {
        var data = new CommandDescription
        {
            Entity = CommandLine.Prompt("Entities: "),
        };
        ct.ThrowIfCancellationRequested();
        data.Name = CommandLine.Prompt("Command name: ");
        ct.ThrowIfCancellationRequested();
        data.RequestFields = PromptFields("Request field");
        ct.ThrowIfCancellationRequested();
        data.ComposeFields = PromptFields("Compose field");
        ct.ThrowIfCancellationRequested();
        data.DomainCommandNamespace = BuildNamespace(domainProject.Name, area, Commands, data.Entity);
        data.ApplicationCommandNamespace = BuildNamespace(applicationProject.Name, area, Commands, data.Entity);
        data.RequestNamespace = BuildNamespace(viewModelProject.Name, area, Requests, data.Entity);

        return data;
    }

    private class CommandDescription
    {
        public string Entity { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public IList<ValueTuple<string, string>> RequestFields { get; set; } =
            new List<ValueTuple<string, string>>();

        public IList<ValueTuple<string, string>> ComposeFields { get; set; } =
            new List<ValueTuple<string, string>>();

        public string DomainCommandNamespace { get; set; } = string.Empty;
        public string ApplicationCommandNamespace { get; set; } = string.Empty;
        public string RequestNamespace { get; set; } = string.Empty;
    }
}

internal class CommandCommandConfiguration
{
    [Option("domain")]
    [Help("Domain layer to add command to.")]
    public string DomainProject { get; set; } = "Domain";

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