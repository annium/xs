using Annium.Core.DependencyInjection;
using Xs.Registry.Db;

namespace Xs.Registry.Node;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
        Add<TestBaseServicePack>();
    }
}