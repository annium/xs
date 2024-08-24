using System.Collections.Generic;
using Xx.Cli.Core.Projects;
using Xx.Cli.Dotnet.Models;
using Xx.Cli.Dotnet.Tools;

namespace Xx.Cli.Dotnet.Projects;

internal interface IPlatformProject : IProject
{
    PlatformConfiguration Config { get; }
    HashSet<string> Solutions { get; }
    TargetFramework TargetFramework { get; }
    OutputType OutputType { get; }
}
