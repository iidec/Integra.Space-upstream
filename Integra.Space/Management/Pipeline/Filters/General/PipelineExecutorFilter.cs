//-----------------------------------------------------------------------
// <copyright file="PipelineExecutorFilter.cs" company="Integra.Space">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Pipeline.Filters
{
    using System;

    /// <summary>
    /// Pipeline executor filter class.
    /// </summary>
    internal class PipelineExecutorFilter : CommandFilter
    {
        /// <inheritdoc />
        public override PipelineContext Execute(PipelineContext input)
        {
            PipelineExecutor cpe = new PipelineExecutor(input.Pipeline);
            PipelineContext result = cpe.Execute(input);

            return input;
        }

        /// <inheritdoc />
        public override void OnError(PipelineContext e)
        {
            throw new NotImplementedException();
        }
    }
}
