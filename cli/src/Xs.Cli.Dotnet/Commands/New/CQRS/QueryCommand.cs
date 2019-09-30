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
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ITemplateWriter templateWriter;
        private readonly ILogger<QueryCommand> logger;

        public QueryCommand(
            DiscoverProjectsTask discoverTask,
            ITemplateWriter templateWriter,
            ILogger<QueryCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.templateWriter = templateWriter;
            this.logger = logger;
        }

        public override void Handle(
            QueryCommandConfiguration cfg,
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

            templateWriter.LoadResources($"{Group.TemplatesDir}.Query");

            var data = GetQueryDescription(applicationProject, viewModelProject, token);

            logger.Debug($"Create query {data.Entity}:{data.Name}");

            // write files
            templateWriter.SetRoot(Path.Combine(applicationProject.Directory, Queries, data.Entity));
            templateWriter.Write(QueryTemplate, $"{data.Name}Query.cs", data);
            templateWriter.SetRoot(Path.Combine(viewModelProject.Directory, data.Entity, Requests));
            templateWriter.Write(RequestTemplate, $"{data.Name}Request.cs", data);
            if (!string.IsNullOrWhiteSpace(data.Response))
            {
                templateWriter.SetRoot(Path.Combine(viewModelProject.Directory, data.Entity, Responses));
                templateWriter.Write(ResponseTemplate, $"{data.Response}Response.cs", data);
            }
        }

        private QueryDescription GetQueryDescription(
            IProject applicationProject,
            IProject viewModelProject,
            CancellationToken token
        )
        {
            var data = new QueryDescription
            {
                Entity = CommandLine.Prompt("Entities: ")
            };
            token.ThrowIfCancellationRequested();
            data.Name = CommandLine.Prompt("Query name: ");
            token.ThrowIfCancellationRequested();
            data.Response = CommandLine.Confirm("Add response") ? CommandLine.Prompt("Response name: ") : string.Empty;
            token.ThrowIfCancellationRequested();
            data.RequestFields = Helper.PromptFields("Request field");
            token.ThrowIfCancellationRequested();
            data.ComposeFields = Helper.PromptFields("Compose field");
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(data.Response))
                data.ResponseFields = Helper.PromptFields("Response field");
            token.ThrowIfCancellationRequested();
            data.QueryNamespace = $"{applicationProject.Name}.{Queries}.{data.Entity}";
            data.RequestNamespace = $"{viewModelProject.Name}.{data.Entity}.{Requests}";
            data.ResponseNamespace = $"{viewModelProject.Name}.{data.Entity}.{Responses}";

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
        [Option("a")]
        [Help("Application layer to add query to.")]
        public string ApplicationProject { get; set; } = "Application";

        [Option("v")]
        [Help("View model layer to add request/response to.")]
        public string ViewModelProject { get; set; } = "ViewModel";
    }
}