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
                if (x.Schema != y.Schema)
                {
                    return false;
                }

                int xValue = x.GrantValue ^ x.DenyValue;
                int yValue = y.GrantValue ^ y.DenyValue;

                int valorMayor = xValue;
                int valorMenor = yValue;
                if (xValue < yValue)
                {
                    valorMayor = yValue;
                    valorMenor = xValue;
                }

                if ((valorMayor & valorMenor) == valorMenor)
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
