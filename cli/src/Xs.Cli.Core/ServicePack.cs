using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tasks.Dependencies;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            container.AddMapper();
            container.AddConfiguration<LoggerConfiguration>(builder => builder.AddCommandLineArgs());
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            container.AddJsonSerializers()
                .Configure(opts => opts
                    .ConfigureForOperations()
                    .ConfigureForNodaTime()
                )
                .SetDefault();
            container.AddHttpRequestFactory().SetDefault();
            container.AddLogging(route => route
                .For(BuildLogFilter(provider.Resolve<LoggerConfiguration>()))
                .UseConsole());
            container.AddShell();

            // projects
            container.Add<IProjectFactory, ProjectFactory>().Singleton();
            container.Add<IProjectLinker, ProjectLinker>().Singleton();

            // tasks
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Task"))
                .AsSelf()
                .Singleton();

            RegisterTasks(container);

            // tools
            container.Add<ITemplateWriter, TemplateWriter>().Transient();
        }

        private void RegisterTasks(IServiceContainer container)
        {
            // dependencies
            container.Add<AddPackageDependencyTask>().AsSelf().Singleton();
            container.Add<AddProjectDependencyTask>().AsSelf().Singleton();
            container.Add<DeletePackageDependencyTask>().AsSelf().Singleton();
            container.Add<DeleteProjectDependencyTask>().AsSelf().Singleton();

            // root
            container.Add<DiscoverProjectsTask>().AsSelf().Singleton();
        }

        private Func<LogMessage, bool> BuildLogFilter(LoggerConfiguration cfg)
        {
            if (cfg.Trace)
                return _ => true;

            if (cfg.Debug)
                return m => m.Level >= LogLevel.Debug;

            return m => m.Level >= LogLevel.Info;
        }
    }
}