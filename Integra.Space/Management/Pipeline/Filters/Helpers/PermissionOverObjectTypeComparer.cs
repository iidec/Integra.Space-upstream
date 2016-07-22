//-----------------------------------------------------------------------
// <copyright file="PermissionOverObjectTypeComparer.cs" company="Integra.Space.Language">
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
    internal class PermissionOverObjectTypeComparer : IEqualityComparer<PermissionOverObjectType>
    {
        /// <inheritdoc />
        public bool Equals(PermissionOverObjectType x, PermissionOverObjectType y)
        {
            if (x.Principal.Name == y.Principal.Name && x.SpaceObjectType == y.SpaceObjectType)
            {
                if (((x.GrantValue ^ x.DenyValue) & (y.GrantValue ^ y.DenyValue)) == (y.GrantValue ^ y.DenyValue))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(PermissionOverObjectType obj)
        {
            return obj.Principal.Name.GetHashCode() + obj.SpaceObjectType.GetHashCode();
        }
    }
}
