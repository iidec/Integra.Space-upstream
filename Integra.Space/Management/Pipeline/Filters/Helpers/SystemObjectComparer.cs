//-----------------------------------------------------------------------
// <copyright file="SystemObjectComparer.cs" company="Integra.Space.Language">
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
    internal class SystemObjectComparer : IEqualityComparer<SystemObject>
    {
        /// <inheritdoc />
        public bool Equals(SystemObject x, SystemObject y)
        {
            if (x.Guid == y.Guid && x.Name == y.Name)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(SystemObject obj)
        {
            return obj.Guid.GetHashCode() + obj.Name.GetHashCode();
        }
    }
}
