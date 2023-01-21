using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Db.Shared.Models;

namespace Server.Host.Auth;

public interface ISessionManager
{
    ValueTuple<Guid, IActionResult> GetToken();

    Task CreateSession(Guid userId);

    Task RefreshSession(UserSession session);

    Task DeleteCurrentSession();
}