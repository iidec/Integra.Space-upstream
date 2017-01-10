//-----------------------------------------------------------------------
// <copyright file="TruncateFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    /// <summary>
    /// Truncate command action class.
    /// </summary>
    internal class TruncateFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext context)
        {
            /* aqui se colocará la funcionalidad para eliminar la data almacenada por la fuente especificada. */

            return context;
        }
    }
}