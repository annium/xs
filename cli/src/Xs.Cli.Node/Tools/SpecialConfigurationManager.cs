using System.IO;
using System.Linq;
using System.Text;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools;

internal class SpecialConfigurationManager : ISpecialConfigurationManager, ILogSubject<SpecialConfigurationManager>
{
    private const string File = ".npmrc";
    public ProjectType Type => Constants.ProjectType;
    public string[] IgnorePatterns { get; } = new[] { File };
    public ILogger<SpecialConfigurationManager> Logger { get; }

    public SpecialConfigurationManager(
        ILogger<SpecialConfigurationManager> logger
    )
    {
        Logger = logger;
    }

    public void Save(IProject project, ProjectTypeConfiguration configuration)
    {
        this.Log().Trace($"Save configuration for {Constants.ProjectType} project {project}");

        // with NPM currently it's not possible to publish unscoped packages privately
        var scope = GetScope(project.Name);
        if (string.IsNullOrWhiteSpace(scope))
        {
            this.Log().Trace($"Skip configuration save for {Constants.ProjectType} project {project}: no scope defined");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"@{scope}:registry={configuration.Server}");
        // add all private scopes
        if (configuration.Special != null)
            foreach (var privateScope in ((SpecialConfiguration) configuration.Special).PrivateScopes.ToHashSet())
                sb.AppendLine($"@{privateScope}:registry={configuration.Server}");
        sb.AppendLine($"//{configuration.Server.Authority}/:_authToken=\"{configuration.Token}\"");
        System.IO.File.WriteAllText(FilePath(project), sb.ToString());

        static string GetScope(string name) => name.StartsWith('@') ? name[1..].Split('/')[0] : string.Empty;
    }

    public void Delete(IProject project)
    {
        var path = FilePath(project);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private string FilePath(IProject project) => Path.Combine(project.Directory, File);
}