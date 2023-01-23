using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Shared.Domain.Models;

namespace Server.Main.Services;

public interface IUserSessionService
{
    ValueTuple<Guid, IActionResult?> GetToken();

    Task CreateSession(Guid userId);

    Task RefreshSession(UserSession session);

    Task DeleteCurrentSession();
}