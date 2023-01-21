using System.IO;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Server.Shared.Helpers;

namespace Server.Host;

internal class Program
{
    internal static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new ServiceProviderFactory(b => b.UseServicePack<ServicePack>()))
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseKestrel(WebHostBuilderHelper.ConfigureKestrel(9901))
                    .UseStartup<Startup>();
            });
    }
}