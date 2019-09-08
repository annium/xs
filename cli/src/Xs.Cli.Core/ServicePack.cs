using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(new ConfigurationBuilder().AddCommandLineArgs().Build<LoggerConfiguration>());
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            services.AddLogging(route => route
                .For(buildLogFilter(provider.GetRequiredService<LoggerConfiguration>()))
                .UseConsole());
            services.AddShell();

            // projects
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IProjectLinker, ProjectLinker>();

            // tools
            services.AddTransient<ITemplateWriter, TemplateWriter>();
        }

        private Func<LogMessage, bool> buildLogFilter(LoggerConfiguration cfg)
        {
            if (cfg.Trace)
                return m => true;

            if (cfg.Debug)
                return m => m.Level >= LogLevel.Debug;

            return m => m.Level >= LogLevel.Info;
        }
    }
}