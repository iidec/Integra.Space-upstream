//-----------------------------------------------------------------------
// <copyright file="PermissionsEnum.cs" company="Integra.Space.Common">
//     Copyright (c) Integra.Space.Common. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Common
{
    /// <summary>
    /// Space permissions enumerable.
    /// </summary>
    internal enum PermissionsEnum
    {
        /// <summary>
        /// Permission alter.
        /// </summary>
        Alter = 1,

        /// <summary>
        /// Permission read.
        /// </summary>
        Read = 2,

        /// <summary>
        /// Permission create.
        /// </summary>
        Create = 4,

        /// <summary>
        /// Permission stop.
        /// </summary>
        Stop = 8,

        /// <summary>
        /// Permission start.
        /// </summary>
        Start = 16,

        /// <summary>
        /// Permission drop.
        /// </summary>
        Drop = 32,

        /// <summary>
        /// Permission all.
        /// </summary>
        All = 63,

        /// <summary>
        /// Permission owner.
        /// </summary>
        Owner = 64
    }
}
