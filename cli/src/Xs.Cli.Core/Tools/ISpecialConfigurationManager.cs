using System;
using System.Collections.Generic;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Tools
{
    public interface ISpecialConfigurationManager
    {
        ProjectType Type { get; }

        void Save(string folder, IEnumerable<ValueTuple<string, Uri, string>> registries);
    }
}