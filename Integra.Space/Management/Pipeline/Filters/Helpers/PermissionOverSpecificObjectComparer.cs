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
                if (x.SpaceObject is SecureObject && !(y.SpaceObject is SecureObject))
                {
                    return false;
                }
                else if (!(x.SpaceObject is SecureObject) && y.SpaceObject is SecureObject)
                {
                    return false;
                }
                else if (x.SpaceObject is SecureObject && y.SpaceObject is SecureObject)
                {
                    if (!(((SecureObject)x.SpaceObject).Schema.Name == ((SecureObject)y.SpaceObject).Schema.Name))
                    {
                        return false;
                    }
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
        public int GetHashCode(PermissionOverSpecificObject obj)
        {
            return obj.Principal.Name.GetHashCode() + obj.SpaceObject.Name.GetHashCode();
        }
    }
}
