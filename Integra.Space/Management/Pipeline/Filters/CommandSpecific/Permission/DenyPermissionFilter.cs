//-----------------------------------------------------------------------
// <copyright file="DenyPermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Reflection;
    using Integra.Space.Models;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    /// <typeparam name="TPermission">Permission type.</typeparam>
    internal class DenyPermissionFilter<TPermission> : PermissionFilter<TPermission> where TPermission : PermissionAssigned
    {
        /// <inheritdoc />
        protected override void ExecutePermissionAction(PermissionCacheRepository<TPermission> repo, TPermission permission)
        {
            repo.Deny(permission);
        }

        /// <inheritdoc />
        protected override void ExecuteReverse(PermissionCacheRepository<TPermission> pr, TPermission permission)
        {
            pr.ReverseDeny(permission);
        }
    }
}
