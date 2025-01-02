using System.Linq;
using Annium.Logging;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Core.Tasks.Dependencies;

public class AddProjectDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public AddProjectDependencyTask(ILogger logger)
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, Dependency<IProject> dependency)
    {
        var (_, project) = dependency;

        this.Debug(
            "Add project {project} as {projectType} dependency to {targetsLength} projects.",
            project,
            project.Type,
            targets.Length
        );
        foreach (var target in targets)
        {
            if (target.Projects.Contains(dependency))
            {
                this.Debug(
                    "Skip adding project {project} as dependency of {target}. {target} already uses {project}.",
                    project,
                    target,
                    target,
                    project
                );
                continue;
            }

            if (target.Projects.Any(p => p.Value == project))
            {
                this.Debug(
                    "Delete project {project} as dependency of {target} due to dependency type change.",
                    project,
                    target
                );
                target.Projects.RemoveWhere(p => p.Value == project);
            }

            this.Debug("Add project {project} as dependency of {target}.", project, target);
            target.Projects.Add(dependency);
            target.Save();
        }
    }
}
