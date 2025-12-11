using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Threading;

namespace Annium.Xs.Cli.Tools;

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
        await using var server = ServerBuilder.New(_sp).WithHttpHandler(handler).Start().NotNull();

        var browser = new Process
        {
            StartInfo = new ProcessStartInfo(server.HttpUri().ToString()) { UseShellExecute = true },
        };
        browser.Start();

        await ct;

        browser.Kill();
    }
}
