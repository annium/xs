using System.Collections.Generic;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet.Projects;

internal interface IPlatformProject : IProject
{
    PlatformConfiguration Config { get; }
    HashSet<string> Solutions { get; }
    TargetFramework TargetFramework { get; }
    OutputType OutputType { get; }
}
