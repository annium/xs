using Annium.Core.DependencyInjection;

namespace Xs.Registry.Dotnet;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.TestBaseServicePack>();
    }
}