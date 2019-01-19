using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace Xs.Registry.Shared
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel(ConfigureKestrel)
                .ConfigureLogging(ConfigureLogging)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup<ServicePack>>();
        }

        private static void ConfigureKestrel(KestrelServerOptions options)
        {
            options.AddServerHeader = false;
            options.ListenAnyIP(9901);
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        }
    }
}