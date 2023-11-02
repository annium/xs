using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;

namespace Xs.Tools;

public class WebServer : ILogSubject
{
    private readonly IServiceProvider _sp;
    public ILogger Logger { get; }

    public WebServer(IServiceProvider sp, ILogger logger)
    {
        _sp = sp;
        Logger = logger;
    }

    public async Task RunAsync(IHttpHandler handler, CancellationToken ct)
    {
        var port = FreePort();
        var url = $"http://localhost:{port}/";

        var server = ServerBuilder.New(_sp, port).WithHttpHandler(handler).Build();

        var browser = new Process { StartInfo = new ProcessStartInfo(url) { UseShellExecute = true } };
        browser.Start();

        await server.RunAsync(ct);

        browser.Kill();
    }

    private int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }
}
