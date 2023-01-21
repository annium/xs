using System;
using System.Linq;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Xs.Cli.Node;
using Group = Xs.Commands.Group;

await using var entry = Entrypoint.Default
    .UseServicePack<Xs.ServicePack>()
    .UseServicePack<Xs.RegistryClient.Main.ServicePack>()
    .UseServicePack<Xs.Cli.Core.ServicePack>()
    .UseServicePack<Xs.Cli.Dotnet.ServicePack>()
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;
var verbose = args.Contains("--verbose");

try
{
    Commander.Run<Group>(provider, args, ct);
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