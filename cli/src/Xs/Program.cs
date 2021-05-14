using System;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Core.Entrypoint;
using Annium.Core.Internal;

namespace Xs
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            Log.Trace(() => "start");
            var verbose = args.Contains("--verbose");

            try
            {
                new Commander(provider).Run<Commands.Group>(args, ct);
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
            .UseServicePack<ServicePack>()
            .UseServicePack<Xs.RegistryClient.Main.ServicePack>()
            .UseServicePack<Xs.RegistryClient.Server.ServicePack>()
            .UseServicePack<Cli.Core.ServicePack>()
            .UseServicePack<Cli.Dotnet.ServicePack>()
            .UseServicePack<Cli.Node.ServicePack>()
            .Run(Run, args);
    }
}