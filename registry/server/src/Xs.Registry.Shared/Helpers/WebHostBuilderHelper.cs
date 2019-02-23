using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace Xs.Registry.Shared.Helpers
{
    public static class WebHostBuilderHelper
    {
        public static Action<KestrelServerOptions> ConfigureKestrel(int port) =>
            (KestrelServerOptions options) =>
            {
                options.AddServerHeader = false;

                var httpsFile = Path.GetFullPath(Path.Combine("certs", "cert.pfx"));
                if (File.Exists(httpsFile))
                    options.ListenAnyIP(port, listenOptions => listenOptions.UseHttps(httpsFile));
                else
                    options.ListenAnyIP(port);
            };

        public static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        }
    }
}