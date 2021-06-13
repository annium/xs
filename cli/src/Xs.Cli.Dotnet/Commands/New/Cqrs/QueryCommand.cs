using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Core.Primitives;
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
    internal class QueryCommand : Command<QueryCommandConfiguration, DiscoverConfiguration>, ILogSubject
    {
        private const string DomainQueryTemplate = "DomainQuery.cs_tpl";
        private const string ApplicationQueryTemplate = "ApplicationQuery.cs_tpl";
        private const string RequestTemplate = "Request.cs_tpl";
        private const string ResponseTemplate = "Response.cs_tpl";
        private const string Queries = "Queries";
        private const string Requests = "Requests";
        private const string Responses = "Responses";
        public ILogger Logger { get; }
        public override string Id => "query";
        public override string Description => "Create query.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ITemplateWriter _templateWriter;

        public QueryCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<QueryCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _templateWriter = templateWriter;
            Logger = logger;
        }

        public override void Handle(
            QueryCommandConfiguration cfg,
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

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Query");

            var data = GetQueryDescription(domainProject, applicationProject, viewModelProject, cfg.Area, ct);

            this.Debug($"Create query {data.Entity}:{data.Name}");

            // write files
            _templateWriter.SetRoot(BuildPath(domainProject.Directory, cfg.Area, Queries, data.Entity));
            _templateWriter.Write(DomainQueryTemplate, $"{data.Name}Query.cs", data);
            _templateWriter.SetRoot(BuildPath(applicationProject.Directory, cfg.Area, Queries, data.Entity));
            _templateWriter.Write(ApplicationQueryTemplate, $"{data.Name}Query.cs", data);
            _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Requests, data.Entity));
            _templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
            if (!string.IsNullOrWhiteSpace(data.Response))
            {
                _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Responses, data.Entity));
                _templateWriter.Write(ResponseTemplate, $"{data.Response}Response.cs", data);
            }

            _templateWriter.EnsureAllWritten();
        }

        private QueryDescription GetQueryDescription(
            IProject domainProject,
            IProject applicationProject,
            IProject viewModelProject,
            string? area,
            CancellationToken ct
        )
        {
            var data = new QueryDescription
            {
                Entity = CommandLine.Prompt("Entities: "),
            };
            ct.ThrowIfCancellationRequested();
            data.Name = CommandLine.Prompt("Query name: ");
            ct.ThrowIfCancellationRequested();
            data.Response = CommandLine.Confirm("Add response") ? CommandLine.Prompt("Response name: ") : string.Empty;
            ct.ThrowIfCancellationRequested();
            data.RequestFields = PromptFields("Request field");
            ct.ThrowIfCancellationRequested();
            data.ComposeFields = PromptFields("Compose field");
            ct.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(data.Response))
                data.ResponseFields = PromptFields("Response field");
            ct.ThrowIfCancellationRequested();
            data.DomainQueryNamespace = BuildNamespace(domainProject.Name, area, Queries, data.Entity);
            data.ApplicationQueryNamespace = BuildNamespace(applicationProject.Name, area, Queries, data.Entity);
            data.RequestNamespace = BuildNamespace(viewModelProject.Name, area, Requests, data.Entity);
            data.ResponseNamespace = BuildNamespace(viewModelProject.Name, area, Responses, data.Entity);

            return data;
        }

        private class QueryDescription
        {
            public string Entity { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;

            public IList<ValueTuple<string, string>> RequestFields { get; set; } =
                new List<ValueTuple<string, string>>();

            public IList<ValueTuple<string, string>> ComposeFields { get; set; } =
                new List<ValueTuple<string, string>>();

            public IList<ValueTuple<string, string>> ResponseFields { get; set; } =
                new List<ValueTuple<string, string>>();

            public string DomainQueryNamespace { get; set; } = string.Empty;
            public string ApplicationQueryNamespace { get; set; } = string.Empty;
            public string RequestNamespace { get; set; } = string.Empty;
            public string ResponseNamespace { get; set; } = string.Empty;
        }
    }

    internal class QueryCommandConfiguration
    {
        [Option("domain")]
        [Help("Domain layer to add query to.")]
        public string DomainProject { get; set; } = "Domain";

        [Option("app")]
        [Help("Application layer to add query to.")]
        public string ApplicationProject { get; set; } = "Application";

        [Option("view")]
        [Help("View model layer to add request/response to.")]
        public string ViewModelProject { get; set; } = "ViewModel";

        [Option("area")]
        [Help("Optional area to generate within.")]
        public string? Area { get; set; }
    }
}