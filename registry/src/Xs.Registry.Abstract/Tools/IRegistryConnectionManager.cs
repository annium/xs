using Microsoft.AspNetCore.Hosting;
using Xs.Core.Models;

namespace Xs.Registry.Abstract.Tools
{
    public interface IRegistryConnectionManager
    {
        void Setup(ProjectType type, IConfiguration configuration, IApplicationLifetime lifetime);
    }
}