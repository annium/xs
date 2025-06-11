using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Core.Audit;

public static class ServiceContainerExtensions
{
    public static void AddAuditRule<TRule, TProject>(this IServiceContainer container)
        where TRule : class, IAuditRule<TProject>
        where TProject : IProject
    {
        container.Add<IAuditRule<TProject>, TRule>().Singleton();
        container.Add<IAuditRule, TRule>().Singleton();
    }
}
