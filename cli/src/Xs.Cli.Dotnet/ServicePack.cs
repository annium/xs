using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ISpecialProjectFactory, ProjectFactory>();
            services.AddSingleton<ProjectMapper>();
            services.AddSingleton<IDependencyManager, DependencyManager>();
            services.AddSingleton<ISpecialConfigurationManager, SpecialConfigurationManager>();

            // audit rules
            services.AddAuditRule<FindInconsistentDependenciesRule<ISpecialProject>, ISpecialProject>();
            services.AddAuditRule<FindUselessDependenciesRule<ISpecialProject>, ISpecialProject>();

            RegisterCommands(services);
        }

        private void RegisterCommands(IServiceCollection services)
        {
            // new
            services.AddSingleton<Commands.New.Group>();
            services.AddSingleton<Commands.New.ExeCommand>();
            services.AddSingleton<Commands.New.LibCommand>();
            services.AddSingleton<Commands.New.LibTestsCommand>();
            services.AddSingleton<Commands.New.WebCommand>();
            services.AddSingleton<Commands.New.WebTestsCommand>();
        }
    }
}