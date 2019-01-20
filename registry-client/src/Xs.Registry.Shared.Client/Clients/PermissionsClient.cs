using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Extensions.Net.Http;
using Xs.Core.Models;
using Xs.Registry.Core.Client;

namespace Xs.Registry.Shared.Client
{
    public class PermissionsClient : ClientBase
    {
        public Task GrantAsync(
            ProjectType type,
            string name,
            PermissionCategory category,
            Permission permission,
            string token
        )
        {
            EnsureSinglePermission(permission);

            return Http.Open(this.uri)
                .Put($"metadata/{type}/{name}/permissions/{category}/{permission}")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Permission grant failed with {response.StatusCode} ({response.ReasonPhrase})")
                .RunAsync();
        }

        public Task<IReadOnlyDictionary<PermissionCategory, Permission>> GetAsync(
            ProjectType type,
            string name,
            string token
        )
        {
            return Http.Open(this.uri)
                .Get($"metadata/{type}/{name}/permissions")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Permissions load failed with {response.StatusCode} ({response.ReasonPhrase})")
                .AsAsync<IReadOnlyDictionary<PermissionCategory, Permission>>();
        }

        public Task RevokeAsync(
            ProjectType type,
            string name,
            PermissionCategory category,
            Permission permission,
            string token
        )
        {
            EnsureSinglePermission(permission);

            return Http.Open(this.uri)
                .Delete($"metadata/{type}/{name}/permissions/{category}/{permission}")
                .BearerAuthorization(token)
                .EnsureSuccessStatusCode(response => $"Permission revoke failed with {response.StatusCode} ({response.ReasonPhrase})")
                .RunAsync();
        }

        private void EnsureSinglePermission(Permission permission)
        {
            var value = (int) permission;
            if (value == 0 || (value & (value - 1)) > 0)
                throw new InvalidOperationException("Permission manipulation is allowed for single permission per operation");
        }
    }
}