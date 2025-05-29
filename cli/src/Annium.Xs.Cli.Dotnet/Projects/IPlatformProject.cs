using System.Collections.Generic;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Dotnet.Models;
using Annium.Xs.Cli.Dotnet.Tools;

namespace Annium.Xs.Cli.Dotnet.Projects;

internal interface IPlatformProject : IProject
{
    PlatformConfiguration Config { get; }
    HashSet<string> Solutions { get; }
    TargetFramework TargetFramework { get; }
    OutputType OutputType { get; }
}
