//-----------------------------------------------------------------------
// <copyright file="SpaceRoleTypeEnum.cs" company="Integra.Space.Common">
//     Copyright (c) Integra.Space.Common. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Common
{
    /// <summary>
    /// Space role enumerator.
    /// </summary>
    internal enum SpaceRoleTypeEnum
    {
        /// <summary>
        /// None role type.
        /// </summary>
        None = 0,

        /// <summary>
        /// System reader role. Can read all the streams of the system.
        /// </summary>
        SysReader = 1,

        /// <summary>
        /// System administrator role.
        /// </summary>
        SysAdmin = 2
    }
}
