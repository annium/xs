using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Projects;

namespace Xs.Tools
{
    internal class ProjectsRunner
    {
        private readonly ILogger<ProjectsRunner> _logger;

        public ProjectsRunner(
            ILogger<ProjectsRunner> logger
        )
        {
            _logger = logger;
        }

        public Task RunAsync<TProject>(
            IEnumerable<TProject> projects,
            Func<TProject, CancellationToken, Task> handle,
            bool runOnDependencies,
            CancellationToken ct
        )
        where TProject : IProject
        {
            var locker = new object();
            using var gate = new ManualResetEventSlim(false);

            var pending = new HashSet<TProject>();
            if (runOnDependencies)
                foreach (var project in projects)
                    CollectTargets(project, pending);
            else
                pending = projects.ToHashSet();

            var running = new List<TProject>();
            var errors = new List<Exception>();

            _logger.Trace($"Start run with {pending.Count} project(s).");

            while (pending.Count > 0)
            {
                ct.ThrowIfCancellationRequested();

                TProject[] starting;
                lock(locker)
                {
                    // get projects, that have no pending dependencies and exclude those already running
                    starting = pending
                        .Where(e => e.Projects.All(d => !pending.Any(p => p as IProject == d.Value)))
                        .Except(running)
                        .ToArray();

                    // if there are no projects running and nothing to start - we have a deadlock
                    if (running.Count == 0 && starting.Length == 0)
                        throw new InvalidOperationException($"Deadlock: none of {string.Join(", ", starting.Select(e => e.Name))} can be run.");

                    _logger.Trace($"Selected {starting.Length} for execution: {Environment.NewLine}{string.Join(Environment.NewLine,starting.Select(e => e.Name))}");

                    running.AddRange(starting);
                }

                // run each of starting projects
                foreach (var project in starting)
                    Task.Run(async() =>
                    {
                        try
                        {
                            _logger.Trace($"Starting run for {project}");

                            // handle project
                            await handle(project, ct);

                            // if succeed - remove from pending
                            lock(locker) pending.Remove(project);

                            _logger.Trace($"Finished run for {project}");
                        }
                        catch (Exception e)
                        when(e is TaskCanceledException || e is OperationCanceledException)
                        {
                            // if canceled - clear pending
                            lock(locker) pending.Clear();

                            _logger.Trace($"Cancelled run for {project}");
                        }
                        catch (Exception exception)
                        {
                            // if failed - add exception and clear pending to avoid next iterations
                            errors.Add(exception);
                            lock(locker) pending.Clear();

                            _logger.Trace($"Failed run for {project}:{Environment.NewLine}{exception.Message}");
                        }
                        finally
                        {
                            // remove from running ones
                            lock(locker) running.Remove(project);

                            _logger.Trace($"Finalized run for {project}. Signal.");

                            // signal for next iteration
                            gate.Set();
                        }
                    });

                // wait for next iteration
                _logger.Trace("Waiting for a signal.");
                gate.Wait();
                gate.Reset();
            }

            _logger.Trace($"Finished run of {projects.Count()} with {errors.Count} error(s).");

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
    }
}