using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tasks.Dependencies;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddConfiguration<LoggerConfiguration>(builder => builder.AddCommandLineArgs());
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            services.AddHttpRequestFactory();
            services.AddLogging(route => route
                .For(BuildLogFilter(provider.GetRequiredService<LoggerConfiguration>()))
                .UseConsole());
            services.AddShell();

            // projects
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IProjectLinker, ProjectLinker>();

            // tasks
            services.AddAssemblyTypes(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Task"))
                .AsSelf()
                .SingleInstance();

            RegisterTasks(services);

            // tools
            services.AddTransient<ITemplateWriter, TemplateWriter>();
        }

        private void RegisterTasks(IServiceCollection services)
        {
            // dependencies
            services.AddSingleton<AddPackageDependencyTask>();
            services.AddSingleton<AddProjectDependencyTask>();
            services.AddSingleton<DeletePackageDependencyTask>();
            services.AddSingleton<DeleteProjectDependencyTask>();

            // root
            services.AddSingleton<DiscoverProjectsTask>();
        }

        private Func<LogMessage, bool> BuildLogFilter(LoggerConfiguration cfg)
        {
            if (cfg.Trace)
                return m => true;

            if (cfg.Debug)
                return m => m.Level >= LogLevel.Debug;

            return m => m.Level >= LogLevel.Info;
        }
    }
}