using System;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Extensions.Entrypoint;

namespace Xs.Cli.Main
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            new Commander(provider).Run<Commands.Group>(args, token);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<Annium.Extensions.Arguments.ServicePack>()
            .UseServicePack<Xs.Registry.Dotnet.Client.ServicePack>()
            .UseServicePack<Xs.Registry.Node.Client.ServicePack>()
            .UseServicePack<Xs.Registry.Shared.Client.ServicePack>()
            .UseServicePack<Core.ServicePack>()
            .UseServicePack<Dotnet.ServicePack>()
            .UseServicePack<Node.ServicePack>()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}