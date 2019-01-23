using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tools
{
    internal class ProjectsRunner
    {
        private readonly ILogger logger;

        public ProjectsRunner(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public Task RunAsync<TProject>(
            IEnumerable<TProject> projects,
            Func<TProject, CancellationToken, Task> handle,
            CancellationToken token
        )
        where TProject : IProject
        {
            var locker = new object();
            var gate = new ManualResetEventSlim(false);

            var pending = projects.ToList();
            var running = new List<TProject>();
            var errors = new List<Exception>();

            logger.LogTrace($"Start run with {pending.Count} project(s).");

            while (pending.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                TProject[] starting;
                lock(locker)
                {
                    // get projects, that have no pending dependencies and exclude those already running
                    starting = pending
                        .Where(e => e.ProjectDependencies.All(d => !pending.Any(c => c as IProject == d)))
                        .Except(running)
                        .ToArray();

                    // if there are no projects running and nothing to start - we have a deadlock
                    if (running.Count == 0 && starting.Length == 0)
                        throw new InvalidOperationException($"Deadlock: none of {string.Join(", ", starting.Select(e => e.Name))} can be run.");

                    logger.LogTrace($"Selected {starting.Length} for execution: {Environment.NewLine}{string.Join(Environment.NewLine,starting.Select(e => e.Name))}");

                    running.AddRange(starting);
                }

                // run each of starting projects
                foreach (var project in starting)
                    Task.Run(async() =>
                    {
                        try
                        {
                            logger.LogTrace($"Starting run for {project.Name}");

                            // handle project
                            await handle(project, token);

                            // if succeed - remove from pending
                            lock(locker) pending.Remove(project);

                            logger.LogTrace($"Finished run for {project.Name}");
                        }
                        catch (Exception e)
                        when(e is TaskCanceledException || e is OperationCanceledException)
                        {
                            // if canceled - clear pending
                            lock(locker) pending.Clear();

                            logger.LogTrace($"Cancelled run for {project.Name}");
                        }
                        catch (Exception exception)
                        {
                            // if failed - add exception and clear pending to avoid next iterations
                            errors.Add(exception);
                            lock(locker) pending.Clear();

                            logger.LogTrace($"Failed run for {project.Name}:{Environment.NewLine}{exception.Message}");
                        }
                        finally
                        {
                            // remove from running ones
                            lock(locker) running.Remove(project);

                            logger.LogTrace($"Finalized run for {project.Name}. Signal.");

                            // signal for next iteration
                            gate.Set();
                        }
                    });

                // wait for next iteration
                logger.LogTrace("Waiting for a signal.");
                gate.Wait();
                gate.Reset();
            }

            logger.LogTrace($"Finished run of {projects.Count()} with {errors.Count} error(s).");

            if (errors.Count > 0)
                throw new AggregateException(errors);

            return Task.CompletedTask;
        }
    }
}