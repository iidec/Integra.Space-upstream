//-----------------------------------------------------------------------
// <copyright file="DropStreamFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;

    /// <summary>
    /// Drop source filter class.
    /// </summary>
    internal class DropStreamFilter : DropEntityFilter<Stream>
    {
        /// <inheritdoc />
        protected override Stream CloneEntity(Stream entityToClone)
        {
            return new Stream(entityToClone.Guid, string.Copy(entityToClone.Identifier), string.Copy(entityToClone.Query));
        }
    }
}
