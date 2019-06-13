using System;
using Annium.Core.Application.Types;

namespace Xs.Cli.Node.Tools
{
    [ResolveKey(Constants.ProjectTypeString)]
    internal class SpecialConfiguration : Core.Tools.SpecialConfiguration
    {
        public string[] PrivateScopes { get; private set; } = Array.Empty<string>();
    }
}