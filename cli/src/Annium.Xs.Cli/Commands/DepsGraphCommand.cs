using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Net.Servers.Web;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Tools;
using QuikGraph;
using QuikGraph.Graphviz;

namespace Annium.Xs.Cli.Commands;

internal class DepsGraphCommand : AsyncCommand<DepsGraphCommandConfiguration, DiscoverConfiguration>, ICommandDescriptor
{
    public static string Id => "deps-graph";
    public static string Description => "Show dependencies graph.";
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly WebServer _webServer;
    private readonly IShell _shell;

    public DepsGraphCommand(DiscoverProjectsTask discoverTask, WebServer webServer, IShell shell)
    {
        _discoverTask = discoverTask;
        _webServer = webServer;
        _shell = shell;
    }

    public override async Task HandleAsync(
        DepsGraphCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        await _webServer.RunAsync(new GraphHandler(_discoverTask, cfg, discoverCfg, _shell), ct);
    }
}

file class GraphHandler : IHttpHandler
{
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly DepsGraphCommandConfiguration _cfg;
    private readonly DiscoverConfiguration _discoverCfg;
    private readonly IShell _shell;

    public GraphHandler(
        DiscoverProjectsTask discoverTask,
        DepsGraphCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        IShell shell
    )
    {
        _discoverTask = discoverTask;
        _cfg = cfg;
        _discoverCfg = discoverCfg;
        _shell = shell;
    }

    public async Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        var allProjects = await _discoverTask.RunAsync(_discoverCfg);
        var projects = string.IsNullOrEmpty(_cfg.Mask) ? allProjects : allProjects.FilterMask(_cfg.Mask);

        var graph = new EdgeListGraph<string, Edge<string>>();
        foreach (var project in projects)
            graph.AddVerticesAndEdgeRange(project.Projects.Select(x => new Edge<string>(project.Name, x.Value.Name)));

        var dot = graph.ToGraphviz(algo =>
        {
            algo.FormatVertex += (_, args) =>
            {
                args.VertexFormat.Label = args.Vertex;
            };
        });

        var tempPath = Path.GetTempPath();
        var id = Guid.NewGuid().ToString();
        var dotFile = Path.Combine(tempPath, $"{id}.dot");
        var svgFile = Path.Combine(tempPath, $"{id}.svg");

        await File.WriteAllTextAsync(dotFile, dot, CancellationToken.None);

        await _shell.Cmd($"dot -Tsvg {dotFile}-o {svgFile}").RunAsync(CancellationToken.None);

        ctx.Response.ContentType = "image/svg+xml";
        var content = await File.ReadAllBytesAsync(svgFile, CancellationToken.None);
        await ctx.Response.OutputStream.WriteAsync(content, CancellationToken.None);

        File.Delete(dotFile);
        File.Delete(svgFile);
    }
}

internal class DepsGraphCommandConfiguration
{
    [Position(1)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = string.Empty;
}
