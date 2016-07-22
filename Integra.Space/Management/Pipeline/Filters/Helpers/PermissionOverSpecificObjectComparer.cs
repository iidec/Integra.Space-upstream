//-----------------------------------------------------------------------
// <copyright file="PermissionOverSpecificObjectComparer.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// System object comparer class.
    /// </summary>
    internal class PermissionOverSpecificObjectComparer : IEqualityComparer<PermissionOverSpecificObject>
    {
        /// <inheritdoc />
        public bool Equals(PermissionOverSpecificObject x, PermissionOverSpecificObject y)
        {
            if (x.Principal.Name == y.Principal.Name && x.SpaceObject.Name == y.SpaceObject.Name)
            {
                if (((x.GrantValue ^ x.DenyValue) & (y.GrantValue ^ y.DenyValue)) == (y.GrantValue ^ y.DenyValue))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(PermissionOverSpecificObject obj)
        {
            return obj.Principal.Name.GetHashCode() + obj.SpaceObject.Name.GetHashCode();
        }
    }
}
