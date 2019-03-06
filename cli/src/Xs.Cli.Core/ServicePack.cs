using System;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
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
            services.AddSingleton(
                (LoggerConfiguration) new ConfigurationBuilder()
                .AddCommandLineArgs()
                .Build<RawLoggerConfiguration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            // projects
            services.AddSingleton<IProjectFactory, ProjectFactory>();

            // tools
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IShell, Shell>();
            services.AddTransient<ITemplateWriter, TemplateWriter>();
        }
    }
}