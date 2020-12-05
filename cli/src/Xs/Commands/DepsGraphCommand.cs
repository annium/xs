using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using QuikGraph;
using QuikGraph.Graphviz;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands
{
    internal class DepsGraphCommand : Command<DiscoverConfiguration>
    {
        public const string IdColumn = "id";
        public override string Id { get; } = "";
        public override string Description { get; } = "Show dependencies graph.";
        private readonly DiscoverProjectsTask _discoverTask;

        public DepsGraphCommand(
            DiscoverProjectsTask discoverTask
        )
        {
            _discoverTask = discoverTask;
        }

        public override void Handle(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = _discoverTask.Run(discoverCfg).ToList();

            var graph = new EdgeListGraph<string, Edge<string>>();
            foreach (var project in projects)
                graph.AddVerticesAndEdgeRange(project.Projects.Select(x => new Edge<string>(project.Name, x.Value.Name)));

            var dot = graph.ToGraphviz(algo => { algo.FormatVertex += (_, args) => { args.VertexFormat.Label = args.Vertex; }; });
            Console.WriteLine(dot);
        }
    }
}