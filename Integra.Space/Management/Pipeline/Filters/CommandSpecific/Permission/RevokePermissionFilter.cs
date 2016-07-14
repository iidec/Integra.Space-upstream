//-----------------------------------------------------------------------
// <copyright file="RevokePermissionFilter.cs" company="Integra.Space.Language">
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
    internal class RevokePermissionFilter : PermissionFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            MethodInfo method = typeof(PermissionCacheRepository).GetMethod("Revoke", new Type[] { typeof(Permission) });
            this.ExecuteActionOverPermissions(context, this.GetPermissionsToAssing(context), method);

            // throw new System.Exception("Simulando error");
            return context;
        }
        
        /// <inheritdoc />
        public override void OnError(PipelineExecutionCommandContext context)
        {
            if (this.OldPermissions != null)
            {
                PermissionCacheRepository pr = (PermissionCacheRepository)context.Kernel.Get<IRepository<Permission>>();
                foreach (Permission p in this.OldPermissions)
                {
                    pr.ReverseRevoke(p);
                }
            }
        }
    }
}
