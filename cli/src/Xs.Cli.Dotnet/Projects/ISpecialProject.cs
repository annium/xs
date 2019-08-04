using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal interface ISpecialProject : IProject
    {
        TargetFramework TargetFramework { get; }
        OutputType OutputType { get; }
    }
}