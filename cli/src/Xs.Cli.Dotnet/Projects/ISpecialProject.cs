using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet.Projects;

internal interface ISpecialProject : IProject
{
    SpecialConfiguration Config { get; }
    TargetFramework TargetFramework { get; }
    OutputType OutputType { get; }
}