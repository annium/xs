using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Xs.Registry.Main
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel((KestrelServerOptions options) =>
                {
                    var port = 9901;
                    options.AddServerHeader = false;

                    var httpsFile = Path.GetFullPath(Path.Combine("certs", "cert.pfx"));
                    if (File.Exists(httpsFile))
                        options.ListenAnyIP(port, listenOptions => listenOptions.UseHttps(httpsFile));
                    else
                        options.ListenAnyIP(port);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup<ServicePack>>();
        }
    }
}