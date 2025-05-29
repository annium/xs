using System;
using System.Linq;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Annium.Xs.Cli.Node;
using Group = Annium.Xs.Cli.Commands.Group;

await using var entry = Entrypoint
    .Default.UseServicePack<Annium.Xs.Cli.ServicePack>()
    .UseServicePack<Annium.Xs.Server.Client.ServicePack>()
    .UseServicePack<Annium.Xs.Cli.Core.ServicePack>()
    .UseServicePack<Annium.Xs.Cli.Dotnet.ServicePack>()
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;
var verbose = args.Contains("-verbose");

try
{
    await Commander.RunAsync<Group>(provider, args, ct);
}
catch (AggregateException exception)
{
    LogAggregateException(exception);
}
catch (Exception exception)
{
    LogException(exception);
}

void LogAggregateException(AggregateException aggregateException)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;

    var exceptions = aggregateException.Flatten().InnerExceptions;
    Console.WriteLine($"Errors ({exceptions.Count}):");
    foreach (var exception in exceptions)
        Console.WriteLine(verbose ? exception.ToString() : exception.Message);

    Console.ForegroundColor = color;
}

void LogException(Exception exception)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine(verbose ? exception.ToString() : exception.Message);

    Console.ForegroundColor = color;
}
