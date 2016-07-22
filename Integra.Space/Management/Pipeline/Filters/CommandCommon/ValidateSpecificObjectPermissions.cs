//-----------------------------------------------------------------------
// <copyright file="ValidateSpecificObjectPermissions.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Models;
    using Ninject;
    using Repos;

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal class ValidateSpecificObjectPermissions : ValidatePermissions<PermissionOverSpecificObject>
    {
        /// <inheritdoc />
        public override bool ValidatePermission(User user, IEnumerable<PermissionOverSpecificObject> commandPermissions, IEnumerable<PermissionOverSpecificObject> userPermissions)
        {
            bool isAllowed = false;

            foreach (PermissionOverSpecificObject commandPermission in commandPermissions)
            {
                userPermissions.Contains(commandPermission, new PermissionOverSpecificObjectComparer());
            }

            return isAllowed;
        }
    }
}