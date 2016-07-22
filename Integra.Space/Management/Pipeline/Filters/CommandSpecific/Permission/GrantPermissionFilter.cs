//-----------------------------------------------------------------------
// <copyright file="GrantPermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;
    using Integra.Space.Repos;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    /// <typeparam name="TPermission">Permission type.</typeparam>
    internal class GrantPermissionFilter<TPermission> : PermissionFilter<TPermission> where TPermission : PermissionAssigned
    {
        /// <inheritdoc />
        protected override void ExecutePermissionAction(PermissionCacheRepository<TPermission> repo, TPermission permission)
        {
            repo.Grant(permission);
        }

        /// <inheritdoc />
        protected override void ExecuteReverse(PermissionCacheRepository<TPermission> pr, TPermission permission)
        {
            pr.ReverseGrant(permission);
        }
    }
}
