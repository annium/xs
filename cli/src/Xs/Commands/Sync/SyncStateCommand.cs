using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using ConsoleExt = Annium.Extensions.CommandLine.Cli;

namespace Xs.Commands.Sync;

internal class SyncStateCommand : Command<SyncStateCommand.SyncStateCommandConfiguration>, ICommandDescriptor
{
    public static string Id => "state";
    public static string Description => "Show repositories state";
    private readonly SyncConfigurator _configurator;
    private readonly Synchronizer _synchronizer;

    public SyncStateCommand(SyncConfigurator configurator)
    {
        _configurator = configurator;
        _synchronizer = new Synchronizer();
    }

    public override void Handle(SyncStateCommandConfiguration cfg, CancellationToken ct)
    {
        var projects = ResolveProjects(cfg);

        if (projects.Count == 0)
        {
            Console.WriteLine("no project to show state for");
            return;
        }

        try
        {
            Console.WriteLine($"State of {projects.Count} project(s):");
            var visualizer = new Visualizer();
            foreach (var project in projects.OrderBy(x => x.Path))
            {
                var state = _synchronizer.GetProjectState(project);
                visualizer.Show(state);
            }
        }
        catch (SyncException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"State display for {ex.Path} failed:");
            throw ex.Exception;
        }
    }

    private IReadOnlyCollection<SyncProject> ResolveProjects(SyncStateCommandConfiguration cfg)
    {
        var projects = _configurator.Read();

        if (cfg.PathOrGroup == string.Empty)
            return projects;

        var groups = projects.GroupBy(x => x.Group).ToDictionary(x => x.Key);

        if (groups.ContainsKey(cfg.PathOrGroup))
        {
            var group = cfg.PathOrGroup;
            return projects.Where(x => x.Group == group).ToList();
        }

        var path = Path.GetFullPath(cfg.PathOrGroup.TrimEnd('/'));
        var project = projects.SingleOrDefault(x => x.Path == path);

        return project is null ? Array.Empty<SyncProject>() : new[] { project };
    }

    internal class SyncStateCommandConfiguration
    {
        [Position(1, isRequired: false)]
        [Help("Project repository path or group name.")]
        public string PathOrGroup { get; set; } = string.Empty;
    }

    private class Synchronizer
    {
        public SyncProjectState GetProjectState(SyncProject project)
        {
            var changes = Helper.GetProjectChanges(project);

            return new SyncProjectState(project.Path, changes);
        }
    }

    private sealed record SyncProjectState(string Path, IReadOnlyCollection<SyncFileChange> Changes);

    private class Visualizer
    {
        public void Show(SyncProjectState state)
        {
            Line($"{state.Path}:");
            if (state.Changes.Count == 0)
                return;

            using var _ = ConsoleExt.SetColors(foreground: ConsoleColor.Magenta);
            Line("changes in working directory:");
            foreach (var change in state.Changes)
                Line($"  {change.Status}: {change.Description}");
        }

        private void Line(string message) => Console.WriteLine(message);
    }
}
