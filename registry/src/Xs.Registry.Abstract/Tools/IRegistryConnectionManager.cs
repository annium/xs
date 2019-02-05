using Microsoft.AspNetCore.Hosting;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Tools
{
    public interface IRegistryConnectionManager
    {
        void Setup(ProjectType type, IConfiguration configuration, IApplicationLifetime lifetime);
    }
}