//-----------------------------------------------------------------------
// <copyright file="DropRoleFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;

    /// <summary>
    /// Drop source filter class.
    /// </summary>
    internal class DropRoleFilter : DropEntityFilter<Role>
    {
        /// <inheritdoc />
        protected override Role CloneEntity(Role entityToClone)
        {
            return new Role(entityToClone.Guid, string.Copy(entityToClone.Identifier), Common.SpaceRoleTypeEnum.None);
        }
    }
}
