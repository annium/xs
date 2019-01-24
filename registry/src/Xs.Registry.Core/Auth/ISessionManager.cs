using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Auth
{
    internal interface ISessionManager
    {
        ValueTuple<Guid, IActionResult> GetToken();

        Task SaveSession(User user, Guid token);

        Task DeleteSession(User user);
    }
}