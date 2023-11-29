using System;
using System.Threading.Tasks;
using Annium.Extensions.Shell;

namespace Xs.Cli.Core.Helpers;

public static class ShellInstanceExtensions
{
    public static async Task ExecuteAsync(this IShellInstance shell)
    {
        var result = await shell.RunAsync();
        if (result.IsSuccess)
            return;

        var command = string.Empty;
        shell.Configure(info => command = $"{info.FileName} {info.Arguments}");
        throw new Exception($"{command} failed:{Environment.NewLine}{result.Error}");
    }
}
