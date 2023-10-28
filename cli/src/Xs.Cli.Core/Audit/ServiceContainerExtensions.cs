using Annium.Core.DependencyInjection;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit;

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
