using Annium.Core.DependencyInjection;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddAuditRule<T, Tp>(this IServiceContainer container)
        where T : class, IAuditRule<Tp> where Tp : IProject
    {
        container.Add<IAuditRule<Tp>, T>().Singleton();
        container.Add<IAuditRule, T>().Singleton();

        return container;
    }
}