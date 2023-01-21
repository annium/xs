using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine = Annium.Extensions.CommandLine.Cli;

namespace Xs.Cli.Dotnet.Commands.New.Cqrs;

internal static class Helper
{
    internal const string Areas = "Areas";

    public static IList<ValueTuple<string, string>> PromptFields(string label)
    {
        var fields = new List<ValueTuple<string, string>>();
        while (true)
        {
            var raw = CommandLine.Prompt($"{label}: ");
            if (string.IsNullOrWhiteSpace(raw))
                break;

            var field = raw.Split(' ');
            if (field.Length == 2)
                fields.Add((field[0].Trim(), field[1].Trim()));
        }

        return fields;
    }

    public static string BuildPath(string projectName, string? area, params string[] parts) => string.IsNullOrWhiteSpace(area)
        ? Path.Combine(new[] { projectName }.Concat(parts).ToArray())
        : Path.Combine(new[] { projectName, Areas, area }.Concat(parts).ToArray());

    public static string BuildNamespace(string projectName, string? area, params string[] parts)
        => string.IsNullOrWhiteSpace(area)
            ? string.Join('.', new[] { projectName }.Concat(parts))
            : string.Join('.', new[] { projectName, Areas, area }.Concat(parts));
}