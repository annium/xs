using Annium.Core.DependencyInjection;
using Xs.Registry.Db;

namespace Server.Node;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
        Add<TestBaseServicePack>();
    }
}