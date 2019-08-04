using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Xs.Cli.Main.Tools
{
    internal class Watcher
    {
        private readonly Func<Instant> getInstant;
        private readonly ILogger<Watcher> logger;

        public Watcher(
            Func<Instant> getInstant,
            ILogger<Watcher> logger
        )
        {
            this.getInstant = getInstant;
            this.logger = logger;
        }

        public async Task WatchAsync(
            string root,
            Func<string, bool> filter,
            Func<string, Task> handleChange,
            Func<string, Task> handleDelete,
            CancellationToken token
        )
        {
            var semaphore = new PathSemaphore(getInstant, Duration.FromMilliseconds(100));
            var tasks = new Queue<ValueTuple<Func<string, Task>, string>>();

            using(var watcher = new FileSystemWatcher(root))
            using(var gate = new ManualResetEventSlim(false))
            {
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

                watcher.Created += (sender, args) => AddTask(args.FullPath);
                watcher.Renamed += (sender, args) => { AddTask(args.OldFullPath); AddTask(args.FullPath); };
                watcher.Changed += (sender, args) => AddTask(args.FullPath);
                watcher.Deleted += (sender, args) => AddTask(args.FullPath);
                watcher.Error += (sender, args) => logger.Error(args.GetException());

                // no tasks -> reset -> wait
                // add task -> set
                // problems: task was added after check and set was called before reset

                while (!token.IsCancellationRequested)
                {
                    gate.Reset();
                    if (tasks.Count == 0)
                    {
                        logger.Trace("Wait for tasks.");
                        gate.Wait(token);
                    }

                    logger.Trace($"Pending {tasks.Count} task(s).");
                    // get and execute task
                    var(task, path) = tasks.Dequeue();
                    try
                    {
                        await task(path);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception exception)
                    {
                        logger.Error(exception);
                    }
                }

                void AddTask(string path)
                {
                    if (!semaphore.IsAvailable(path) || !filter(path))
                        return;

                    var task = File.Exists(path) ? handleChange : handleDelete;
                    logger.Trace($"Enqueue task for {path}");
                    tasks.Enqueue((task, path));
                    gate.Set();
                }
            }
        }

        private class PathSemaphore
        {
            private readonly IDictionary<string, Instant> data = new Dictionary<string, Instant>();

            private readonly Func<Instant> getInstant;

            private readonly Duration duration;

            public PathSemaphore(Func<Instant> getInstant, Duration duration)
            {
                this.getInstant = getInstant;
                this.duration = duration;
            }

            public bool IsAvailable(string path)
            {
                var now = getInstant();

                // if cached, and not yet expired - it's not available
                if (data.ContainsKey(path) && data[path] >= now)
                    return false;

                // else, if not used yet, or expired - it's available
                data[path] = getInstant() + duration;

                return true;
            }
        }
    }
}