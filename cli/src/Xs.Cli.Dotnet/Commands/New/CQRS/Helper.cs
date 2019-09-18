using System;
using System.Collections.Generic;
using CommandLine = Annium.Extensions.CommandLine.Cli;

namespace Xs.Cli.Dotnet.Commands.New.CQRS
{
    internal class Helper
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
    }
}