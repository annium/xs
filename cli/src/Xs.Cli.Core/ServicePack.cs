using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                (LoggerConfiguration) new ConfigurationBuilder()
                .AddCommandLineArgs()
                .Build<Logging.LoggerConfiguration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            services.AddConsoleLogger();
            services.AddShell();

            // projects
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IProjectLinker, ProjectLinker>();

            // tools
            services.AddTransient<ITemplateWriter, TemplateWriter>();
        }
    }
}