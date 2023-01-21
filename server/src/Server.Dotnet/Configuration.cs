using System.IO;

namespace Server.Dotnet;

internal class Configuration
{
    public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");
}