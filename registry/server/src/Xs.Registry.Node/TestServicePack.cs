using Annium.Core.DependencyInjection;

namespace Xs.Registry.Node;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.TestBaseServicePack>();
    }
}