//-----------------------------------------------------------------------
// <copyright file="DropUserFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;

    /// <summary>
    /// Drop source filter class.
    /// </summary>
    internal class DropUserFilter : DropEntityFilter<User>
    {
        /// <inheritdoc />
        protected override User CloneEntity(User entityToClone)
        {
            return new User(entityToClone.Guid, string.Copy(entityToClone.Identifier), string.Copy(entityToClone.Password), entityToClone.Enable);
        }
    }
}
