//-----------------------------------------------------------------------
// <copyright file="ValidateObjectTypePermissions.cs" company="Integra.Space">
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

    /// <summary>
    /// Filter lock class.
    /// </summary>
    internal class ValidateObjectTypePermissions : ValidatePermissions<PermissionOverObjectType>
    {
        /// <inheritdoc />
        public override bool ValidatePermission(User user, IEnumerable<PermissionOverObjectType> commandPermissions, IEnumerable<PermissionOverObjectType> userPermissions)
        {
            bool isAllowed = false;

            foreach (PermissionOverObjectType commandPermission in commandPermissions)
            {
                userPermissions.Contains(commandPermission, new PermissionOverObjectTypeComparer());
            }

            return isAllowed;
        }
    }
}