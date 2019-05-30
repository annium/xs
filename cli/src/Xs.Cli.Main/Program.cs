using System;
using System.Linq;
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
            var verbose = args.Contains("--verbose");

            try
            {
                new Commander(provider).Run<Commands.Group>(args, token);
            }
            catch (AggregateException exception)
            {
                LogAggregateException(exception, verbose);
            }
            catch (Exception exception)
            {
                LogException(exception, verbose);
            }
        }

        private static void LogAggregateException(AggregateException aggregateException, bool verbose)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            var exceptions = aggregateException.Flatten().InnerExceptions;
            Console.WriteLine($"Errors ({exceptions.Count}):");
            foreach (var exception in exceptions)
                Console.WriteLine(verbose ? exception.ToString() : exception.Message);

            Console.ForegroundColor = color;
        }

        private static void LogException(Exception exception, bool verbose)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(verbose ? exception.ToString() : exception.Message);

            Console.ForegroundColor = color;
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<Annium.Extensions.Arguments.ServicePack>()
            .UseServicePack<Xs.RegistryClient.Main.ServicePack>()
            .UseServicePack<Xs.RegistryClient.Server.ServicePack>()
            .UseServicePack<Core.ServicePack>()
            .UseServicePack<Dotnet.ServicePack>()
            .UseServicePack<Node.ServicePack>()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}