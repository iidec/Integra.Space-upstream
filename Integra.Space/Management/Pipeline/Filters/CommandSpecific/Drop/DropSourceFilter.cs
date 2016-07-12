//-----------------------------------------------------------------------
// <copyright file="DropSourceFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using Integra.Space.Models;

    /// <summary>
    /// Drop source filter class.
    /// </summary>
    internal class DropSourceFilter : DropEntityFilter<Source>
    {
        /// <inheritdoc />
        protected override Source CloneEntity(Source entityToClone)
        {
            return new Source(entityToClone.Guid, string.Copy(entityToClone.Identifier));
        }
    }
}
