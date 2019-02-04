using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Main.Auth
{
    public interface ISessionManager
    {
        ValueTuple<Guid, IActionResult> GetToken();

        Task CreateSession(Guid userId);

        Task RefreshSession(Guid token);

        Task DeleteCurrentSession();
    }
}