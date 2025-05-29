using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Tools;

internal class ProjectsRunner : ILogSubject
{
    public ILogger Logger { get; }

    public ProjectsRunner(ILogger logger)
    {
        Logger = logger;
    }

    public Task RunAsync<TProject>(
        IReadOnlyCollection<TProject> projects,
        Func<TProject, CancellationToken, Task> handle,
        Config config,
        CancellationToken ct
    )
        where TProject : IProject
    {
        var locker = new object();
        using var gate = new ManualResetEventSlim(false);

        var pending = new HashSet<TProject>();
        if (config.RunOnDependencies)
            foreach (var project in projects)
                CollectTargets(project, pending);
        else
            pending = projects.ToHashSet();

        var running = new List<TProject>();
        var errors = new List<Exception>();

        this.Trace("Start run with {count} project(s).", pending.Count);

        while (pending.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            TProject[] starting;
            lock (locker)
            {
                // get projects, that have no pending dependencies and exclude those already running
                var startingSet = pending
                    .Where(e => e.Projects.All(d => pending.All(p => p as IProject != d.Value)))
                    .Except(running);
                starting =
                    config.Parallelism > 0 ? startingSet.Take(config.Parallelism).ToArray() : startingSet.ToArray();

                // if there are no projects running and nothing to start - we have a deadlock
                if (running.Count == 0 && starting.Length == 0)
                    throw new InvalidOperationException(
                        $"Deadlock: none of {string.Join(", ", starting.Select(e => e.Name))} can be run."
                    );

                this.Trace<int, string, string>(
                    "Selected {startingLength} for execution: {newLine}{projects}",
                    starting.Length,
                    Environment.NewLine,
                    string.Join(Environment.NewLine, starting.Select(e => e.Name))
                );

                running.AddRange(starting);
            }

            // run each of starting projects
            foreach (var project in starting)
                Task.Run(
                        async () =>
                        {
                            try
                            {
                                this.Trace("Starting run for {project}", project);

                                // handle project
                                await handle(project, ct);

                                // if succeed - remove from pending
                                lock (locker)
                                    pending.Remove(project);

                                this.Trace("Finished run for {project}", project);
                            }
                            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
                            {
                                // if canceled - clear pending
                                lock (locker)
                                    pending.Clear();

                                this.Trace("Cancelled run for {project}", project);
                            }
                            catch (Exception exception)
                            {
                                // if failed - add exception and clear pending to avoid next iterations
                                errors.Add(exception);
                                lock (locker)
                                    pending.Clear();

                                this.Trace<IProject, string, string>(
                                    "Failed run for {project}:{newLine}{exception}",
                                    project,
                                    Environment.NewLine,
                                    exception.Message
                                );
                            }
                            finally
                            {
                                // remove from running ones
                                lock (locker)
                                    running.Remove(project);

                                this.Trace("Finalized run for {project}. Signal.", project);

                                // signal for next iteration
                                // ReSharper disable once AccessToDisposedClosure
                                gate.Set();
                            }
                        },
                        ct
                    )
                    .GetAwaiter();

            // wait for next iteration
            this.Trace("Waiting for a signal.");
            gate.Wait(ct);
            gate.Reset();
        }

        this.Trace("Finished run of {projectsCount} with {errorsCount} error(s).", projects.Count, errors.Count);

        if (errors.Count > 0)
            throw new AggregateException(errors);

        return Task.CompletedTask;
    }

    private void CollectTargets<TProject>(TProject project, HashSet<TProject> targets)
        where TProject : IProject
    {
        // if target not added - it was already handled
        // is used to prevent circular calls
        if (!targets.Add(project))
            return;

        foreach (var dependency in project.Projects.Select(d => d.Value).OfType<TProject>())
            CollectTargets(dependency, targets);
    }

    internal readonly struct Config
    {
        public readonly int Parallelism;
        public readonly bool RunOnDependencies;

        public Config(int parallelism, bool runOnDependencies)
        {
            Parallelism = parallelism;
            RunOnDependencies = runOnDependencies;
        }
    }
}
