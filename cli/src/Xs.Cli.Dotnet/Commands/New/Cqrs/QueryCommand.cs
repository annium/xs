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
    internal class QueryCommand : Command<QueryCommandConfiguration, DiscoverConfiguration>
    {
        internal const string QueryTemplate = "Query.cs_tpl";
        internal const string RequestTemplate = "Request.cs_tpl";
        internal const string ResponseTemplate = "Response.cs_tpl";
        internal const string Queries = "Queries";
        internal const string Requests = "Requests";
        internal const string Responses = "Responses";

        public override string Id { get; } = "query";
        public override string Description { get; } = "Create query.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ITemplateWriter _templateWriter;
        private readonly ILogger<QueryCommand> _logger;

        public QueryCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<QueryCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _templateWriter = templateWriter;
            _logger = logger;
        }

        public override void Handle(
            QueryCommandConfiguration cfg,
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

            _templateWriter.LoadResources($"{Group.TemplatesDir}.Query");

            var data = GetQueryDescription(applicationProject, viewModelProject, cfg.Area, token);

            _logger.Debug($"Create query {data.Entity}:{data.Name}");

            // write files
            _templateWriter.SetRoot(BuildPath(applicationProject.Directory, cfg.Area, Queries, data.Entity));
            _templateWriter.Write(QueryTemplate, $"{data.Name}Query.cs", data);
            _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Requests, data.Entity));
            _templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
            if (!string.IsNullOrWhiteSpace(data.Response))
            {
                _templateWriter.SetRoot(BuildPath(viewModelProject.Directory, cfg.Area, Responses, data.Entity));
                _templateWriter.Write(ResponseTemplate, $"{data.Response}Response.cs", data);
            }
        }

        private QueryDescription GetQueryDescription(
            IProject applicationProject,
            IProject viewModelProject,
            string? area,
            CancellationToken token
        )
        {
            var data = new QueryDescription
            {
                Entity = CommandLine.Prompt("Entities: "),
            };
            token.ThrowIfCancellationRequested();
            data.Name = CommandLine.Prompt("Query name: ");
            token.ThrowIfCancellationRequested();
            data.Response = CommandLine.Confirm("Add response") ? CommandLine.Prompt("Response name: ") : string.Empty;
            token.ThrowIfCancellationRequested();
            data.RequestFields = PromptFields("Request field");
            token.ThrowIfCancellationRequested();
            data.ComposeFields = PromptFields("Compose field");
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(data.Response))
                data.ResponseFields = PromptFields("Response field");
            token.ThrowIfCancellationRequested();
            data.QueryNamespace = BuildNamespace(applicationProject.Name, area, Queries, data.Entity);
            data.RequestNamespace = BuildNamespace(viewModelProject.Name, area, Requests, data.Entity);
            data.ResponseNamespace = BuildNamespace(viewModelProject.Name, area, Responses, data.Entity);

            return data;
        }

        private class QueryDescription
        {
            public string Entity { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;
            public IList<ValueTuple<string, string>> RequestFields { get; set; } = new List<ValueTuple<string, string>>();
            public IList<ValueTuple<string, string>> ComposeFields { get; set; } = new List<ValueTuple<string, string>>();
            public IList<ValueTuple<string, string>> ResponseFields { get; set; } = new List<ValueTuple<string, string>>();
            public string QueryNamespace { get; set; } = string.Empty;
            public string RequestNamespace { get; set; } = string.Empty;
            public string ResponseNamespace { get; set; } = string.Empty;
        }
    }

    internal class QueryCommandConfiguration
    {
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