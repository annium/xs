using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine = Annium.Extensions.CommandLine.Cli;

namespace Xs.Cli.Dotnet.Commands.New.Cqrs
{
    internal static class Helper
    {
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

        public static string BuildPath(params string?[] parts) => Path.Combine(parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToArray());

        public static string BuildNamespace(params string?[] parts) => string.Join('.', parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}