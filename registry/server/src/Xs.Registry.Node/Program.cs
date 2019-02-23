using System.IO;
using Microsoft.AspNetCore.Hosting;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Node
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
                .UseKestrel(WebHostBuilderHelper.ConfigureKestrel(9901))
                .ConfigureLogging(WebHostBuilderHelper.ConfigureLogging)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup<ServicePack>>();
        }
    }
}