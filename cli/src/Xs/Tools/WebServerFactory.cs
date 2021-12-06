using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using Swan.Logging;

namespace Xs.Tools;

public class WebServerFactory
{
    public WebServerFactory()
    {
        Logger.UnregisterLogger<ConsoleLogger>();
    }

    public async Task StartAsync(RequestHandlerCallback callback, CancellationToken ct)
    {
        var url = $"http://localhost:{FreePort()}/";

        using var server = new WebServer(o => o.WithUrlPrefix(url).WithMode(HttpListenerMode.Microsoft))
            .WithModule(new ActionModule("/", HttpVerbs.Get, callback));

        var browser = new Process
        {
            StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
        };
        browser.Start();

        await server.RunAsync(ct);
    }

    private int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint) listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }
}