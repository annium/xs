using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Logging;
using Humanizer;
using NuGet.Packaging;

namespace Annium.Xs.Cli.Dotnet.Commands.Nuget;

public class ListContentsCommand : AsyncCommand<ListContentsCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "list-contents";
    public static string Description => "List nuget package contents";
    public ILogger Logger { get; }

    public ListContentsCommand(ILogger logger)
    {
        Logger = logger;
    }

    public override async Task HandleAsync(ListContentsCommandConfiguration cfg, CancellationToken ct)
    {
        if (!File.Exists(cfg.Path))
        {
            this.Error<string>("Package file not found: {path}", cfg.Path);
            return;
        }

        try
        {
            await using var fileStream = File.OpenRead(cfg.Path);
            using var packageReader = new PackageArchiveReader(fileStream);

            var files = packageReader.GetFiles().OrderBy(x => x).ToArray();

            Console.WriteLine($"{files.Length} files found:");
            foreach (var file in files)
            {
                var entry = packageReader.GetEntry(file);
                var size = entry?.Length ?? 0;
                var humanizedSize = size.Bytes().Humanize();
                Console.WriteLine($"- {file} ({humanizedSize})");
            }
        }
        catch (Exception ex)
        {
            this.Error(ex);
        }
    }
}

public class ListContentsCommandConfiguration
{
    [Position(1)]
    [Help("Package path.")]
    public string Path { get; set; } = string.Empty;
}
