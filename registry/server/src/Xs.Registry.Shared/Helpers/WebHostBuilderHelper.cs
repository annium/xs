using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace Xs.Registry.Shared.Helpers;

public static class WebHostBuilderHelper
{
    public static Action<KestrelServerOptions> ConfigureKestrel(int port) => server =>
    {
        server.AddServerHeader = false;
        server.ListenAnyIP(port, listen =>
        {
            var certFile = Path.GetFullPath(Path.Combine("certs", "cert.pfx"));
            if (File.Exists(certFile))
                listen.UseHttps(certFile);
        });
    };

    public static void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConsole();
    }
}