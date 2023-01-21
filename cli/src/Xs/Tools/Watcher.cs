using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Xs.Tools;

internal class Watcher : ILogSubject<Watcher>
{
    public ILogger<Watcher> Logger { get; }
    private readonly ITimeProvider _timeProvider;

    public Watcher(
        ITimeProvider timeProvider,
        ILogger<Watcher> logger
    )
    {
        _timeProvider = timeProvider;
        Logger = logger;
    }

    public async Task WatchAsync(
        string root,
        Func<string, bool> filter,
        Func<string, Task> handleChange,
        Func<string, Task> handleDelete,
        CancellationToken ct
    )
    {
        var semaphore = new PathSemaphore(_timeProvider, Duration.FromMilliseconds(100));
        var tasks = new Queue<ValueTuple<Func<string, Task>, string>>();

        using var watcher = new FileSystemWatcher(root);
        using var gate = new ManualResetEventSlim(false);

        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

        watcher.Created += (_, args) => AddTask(args.FullPath);
        watcher.Renamed += (_, args) =>
        {
            AddTask(args.OldFullPath);
            AddTask(args.FullPath);
        };
        watcher.Changed += (_, args) => AddTask(args.FullPath);
        watcher.Deleted += (_, args) => AddTask(args.FullPath);
        watcher.Error += (_, args) => this.Log().Error(args.GetException());

        // no tasks -> reset -> wait
        // add task -> set
        // problems: task was added after check and set was called before reset

        while (!ct.IsCancellationRequested)
        {
            gate.Reset();
            if (tasks.Count == 0)
            {
                this.Log().Trace("Wait for tasks.");
                gate.Wait(ct);
            }

            this.Log().Trace($"Pending {tasks.Count} task(s).");
            // get and execute task
            var (task, path) = tasks.Dequeue();
            try
            {
                await task(path);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                this.Log().Error(exception);
            }
        }

        void AddTask(string path)
        {
            if (!semaphore.IsAvailable(path) || !filter(path))
                return;

            var task = File.Exists(path) ? handleChange : handleDelete;
            this.Log().Trace($"Enqueue task for {path}");
            tasks.Enqueue((task, path));
            // ReSharper disable once AccessToDisposedClosure
            gate.Set();
        }
    }

    private class PathSemaphore
    {
        private readonly IDictionary<string, Instant> _data = new Dictionary<string, Instant>();

        private readonly ITimeProvider _timeProvider;

        private readonly Duration _duration;

        public PathSemaphore(ITimeProvider timeProvider, Duration duration)
        {
            _timeProvider = timeProvider;
            _duration = duration;
        }

        public bool IsAvailable(string path)
        {
            var now = _timeProvider.Now;

            // if cached, and not yet expired - it's not available
            if (_data.ContainsKey(path) && _data[path] >= now)
                return false;

            // else, if not used yet, or expired - it's available
            _data[path] = _timeProvider.Now + _duration;

            return true;
        }
    }
}