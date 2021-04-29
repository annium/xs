using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using EmbedIO;
using QuikGraph;
using QuikGraph.Graphviz;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class DepsGraphCommand : AsyncCommand<DiscoverConfiguration>
    {
        public override string Id { get; } = "deps-graph";
        public override string Description { get; } = "Show dependencies graph.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly WebServerFactory _webServerFactory;
        private readonly IShell _shell;

        public DepsGraphCommand(
            DiscoverProjectsTask discoverTask,
            WebServerFactory webServerFactory,
            IShell shell
        )
        {
            _discoverTask = discoverTask;
            _webServerFactory = webServerFactory;
            _shell = shell;
        }

        public override async Task HandleAsync(
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            await _webServerFactory.StartAsync(HandleRequest(discoverCfg), token);
        }

        private RequestHandlerCallback HandleRequest(DiscoverConfiguration discoverCfg) => async ctx =>
        {
            var projects = _discoverTask.RunAsync(discoverCfg).Await().ToList();

            var graph = new EdgeListGraph<string, Edge<string>>();
            foreach (var project in projects)
                graph.AddVerticesAndEdgeRange(project.Projects.Select(x => new Edge<string>(project.Name, x.Value.Name)));

            var dot = graph.ToGraphviz(algo => { algo.FormatVertex += (_, args) => { args.VertexFormat.Label = args.Vertex; }; });

            var tempPath = Path.GetTempPath();
            var id = Guid.NewGuid().ToString();
            var dotFile = Path.Combine(tempPath, $"{id}.dot");
            var svgFile = Path.Combine(tempPath, $"{id}.svg");

            await File.WriteAllTextAsync(dotFile, dot);

            await _shell.Cmd("dot", "-Tsvg", dotFile, "-o", svgFile).RunAsync();

            ctx.Response.ContentType = "image/svg+xml";
            var content = await File.ReadAllBytesAsync(svgFile);
            await ctx.Response.OutputStream.WriteAsync(content);

            File.Delete(dotFile);
            File.Delete(svgFile);
        };
    }
}