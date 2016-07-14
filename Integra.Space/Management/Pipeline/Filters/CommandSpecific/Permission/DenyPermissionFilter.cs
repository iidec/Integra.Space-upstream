//-----------------------------------------------------------------------
// <copyright file="DenyPermissionFilter.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Integra.Space.Language;
    using Integra.Space.Models;
    using Integra.Space.Repos;
    using Ninject;

    /// <summary>
    /// Grant permission filter class.
    /// </summary>
    internal class DenyPermissionFilter : PermissionFilter
    {
        /// <inheritdoc />
        public override PipelineExecutionCommandContext Execute(PipelineExecutionCommandContext context)
        {
            MethodInfo method = typeof(PermissionCacheRepository).GetMethod("Deny", new Type[] { typeof(Permission) });
            this.ExecuteActionOverPermissions(context, this.GetPermissionsToAssing(context), method);

            // throw new System.Exception("Simulando error");
            return context;
        }
    }
}
