using System.IO;
using System.Linq;
using System.Text;
using Annium.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools;

internal class PlatformConfigurationManager : IPlatformConfigurationManager, ILogSubject
{
    private const string ConfigFile = ".npmrc";
    public ProjectType Type => Constants.ProjectType;
    public string[] IgnorePatterns { get; } = { ConfigFile };
    public ILogger Logger { get; }

    public PlatformConfigurationManager(ILogger logger)
    {
        Logger = logger;
    }

    public void Save(IProject project, ProjectTypeConfiguration configuration)
    {
        this.Trace($"Save configuration for {Constants.ProjectType} project {project}");

        // with NPM currently it's not possible to publish unscoped packages privately
        var scope = GetScope(project.Name);
        if (string.IsNullOrWhiteSpace(scope))
        {
            this.Trace($"Skip configuration save for {Constants.ProjectType} project {project}: no scope defined");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"@{scope}:registry={configuration.Server}");
        // add all private scopes
        if (configuration.Platform is not null)
            foreach (var privateScope in ((PlatformConfiguration)configuration.Platform).PrivateScopes.ToHashSet())
                sb.AppendLine($"@{privateScope}:registry={configuration.Server}");
        sb.AppendLine($"//{configuration.Server.Authority}/:_authToken=\"{configuration.Token}\"");
        File.WriteAllText(ConfigFilePath(project), sb.ToString());

        static string GetScope(string name) => name.StartsWith('@') ? name[1..].Split('/')[0] : string.Empty;
    }

    public void Delete(IProject project)
    {
        var path = ConfigFilePath(project);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static string ConfigFilePath(IProject project) => Path.Combine(project.Directory, ConfigFile);
}
