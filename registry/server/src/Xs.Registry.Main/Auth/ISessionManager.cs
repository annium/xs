using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Main.Auth;

public interface ISessionManager
{
    ValueTuple<Guid, IActionResult> GetToken();

    Task CreateSession(Guid userId);

    Task RefreshSession(UserSession session);

    Task DeleteCurrentSession();
}