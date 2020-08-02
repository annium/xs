using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Xs.Tools
{
    internal class Watcher
    {
        private readonly Func<Instant> _getInstant;
        private readonly ILogger<Watcher> _logger;

        public Watcher(
            Func<Instant> getInstant,
            ILogger<Watcher> logger
        )
        {
            _getInstant = getInstant;
            _logger = logger;
        }

        public async Task WatchAsync(
            string root,
            Func<string, bool> filter,
            Func<string, Task> handleChange,
            Func<string, Task> handleDelete,
            CancellationToken token
        )
        {
            var semaphore = new PathSemaphore(_getInstant, Duration.FromMilliseconds(100));
            var tasks = new Queue<ValueTuple<Func<string, Task>, string>>();

            using var watcher = new FileSystemWatcher(root);
            using var gate = new ManualResetEventSlim(false);

            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

            watcher.Created += (sender, args) => AddTask(args.FullPath);
            watcher.Renamed += (sender, args) => { AddTask(args.OldFullPath); AddTask(args.FullPath); };
            watcher.Changed += (sender, args) => AddTask(args.FullPath);
            watcher.Deleted += (sender, args) => AddTask(args.FullPath);
            watcher.Error += (sender, args) => _logger.Error(args.GetException());

            // no tasks -> reset -> wait
            // add task -> set
            // problems: task was added after check and set was called before reset

            while (!token.IsCancellationRequested)
            {
                gate.Reset();
                if (tasks.Count == 0)
                {
                    _logger.Trace("Wait for tasks.");
                    gate.Wait(token);
                }

                _logger.Trace($"Pending {tasks.Count} task(s).");
                // get and execute task
                var(task, path) = tasks.Dequeue();
                try
                {
                    await task(path);
                }
                catch (OperationCanceledException) { }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                }
            }

            void AddTask(string path)
            {
                if (!semaphore.IsAvailable(path) || !filter(path))
                    return;

                var task = File.Exists(path) ? handleChange : handleDelete;
                _logger.Trace($"Enqueue task for {path}");
                tasks.Enqueue((task, path));
                gate.Set();
            }
        }

        private class PathSemaphore
        {
            private readonly IDictionary<string, Instant> _data = new Dictionary<string, Instant>();

            private readonly Func<Instant> _getInstant;

            private readonly Duration _duration;

            public PathSemaphore(Func<Instant> getInstant, Duration duration)
            {
                _getInstant = getInstant;
                _duration = duration;
            }

            public bool IsAvailable(string path)
            {
                var now = _getInstant();

                // if cached, and not yet expired - it's not available
                if (_data.ContainsKey(path) && _data[path] >= now)
                    return false;

                // else, if not used yet, or expired - it's available
                _data[path] = _getInstant() + _duration;

                return true;
            }
        }
    }
}