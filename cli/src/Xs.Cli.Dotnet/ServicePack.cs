using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<ISpecialConfigurationManager, SpecialConfigurationManager>();
        }
    }
}